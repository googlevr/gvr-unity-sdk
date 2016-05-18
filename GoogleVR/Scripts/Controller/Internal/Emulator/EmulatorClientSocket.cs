// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissioßns and
// limitations under the License.

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

using proto;

/// @cond
namespace Gvr.Internal {
  class EmulatorClientSocket : MonoBehaviour {
    private static readonly int kPhoneEventPort = 7003;
    private const int kSocketReadTimeoutMillis = 5000;

    // Minimum interval, in seconds, between attempts to reconnect the socket.
    private const float kMinReconnectInterval = 5.0f;

    private TcpClient phoneMirroringSocket;

    private Thread phoneEventThread;
    //private TcpClient phoneEventSocket;
    //private NetworkStream phoneEventStream;

    private volatile bool shouldStop = false;

    private EmulatorManager phoneRemote;
    public bool connected { get; private set; }

    public void Init(EmulatorManager remote) {
      Debug.Log ("Setting up socket.");

      phoneRemote = remote;

      if (EmulatorConfig.Instance.PHONE_EVENT_MODE != EmulatorConfig.Mode.OFF) {
        phoneEventThread = new Thread(phoneEventSocketLoop);
        phoneEventThread.Start();
      }
    }

    private void setupPortForwarding(int port) {
      string adbCommand = string.Format("adb forward tcp:{0} tcp:{0}", port);
      System.Diagnostics.Process myProcess;

#if UNITY_EDITOR_WIN
      string cmd = @"/k " + adbCommand + " & exit";
      Debug.Log ("Executing: [" + cmd + "]");
      myProcess = new System.Diagnostics.Process();
      System.Diagnostics.ProcessStartInfo myProcessStartInfo =
        new System.Diagnostics.ProcessStartInfo("CMD.exe", cmd);
      myProcessStartInfo.UseShellExecute = false;
      myProcessStartInfo.RedirectStandardOutput = true;
      myProcessStartInfo.CreateNoWindow = true;
      myProcess.StartInfo = myProcessStartInfo;
      myProcess.Start();
#else
      Debug.LogFormat("Trying to launch adb: {0}", adbCommand);
      myProcess = System.Diagnostics.Process.Start("bash", string.Format("-l -c \"{0}\"",
                                                                         adbCommand));
#endif
      myProcess.WaitForExit();
      int exitCode = myProcess.ExitCode;
      myProcess.Close();
      if (exitCode == 0) {
        Debug.LogFormat("adb process succeeded (exit code 0).");
      } else {
        Debug.LogErrorFormat("adb process FAILED (exit code {0}). Check that the Android SDK " +
            "is installed and that the adb command is in your PATH environment variable.",
            exitCode);
      }
    }

    private void phoneEventSocketLoop() {
      long lastConnectionAttemptTime = 0;
      bool isFirstTry = true;

      while (!shouldStop) {
        string addr = EmulatorConfig.Instance.PHONE_EVENT_MODE == EmulatorConfig.Mode.USB
          ? EmulatorConfig.USB_SERVER_IP : EmulatorConfig.WIFI_SERVER_IP;

        // Wait a while in order to enforce the minimum time between connection attempts.
        if (!isFirstTry) {
          TimeSpan elapsed = new TimeSpan(DateTime.Now.Ticks - lastConnectionAttemptTime);
          float toWait = kMinReconnectInterval - (float) elapsed.TotalSeconds;
          if (toWait > 0) {
            Thread.Sleep((int) (toWait * 1000));
          }
        }

        isFirstTry = false;
        lastConnectionAttemptTime = DateTime.Now.Ticks;

        try {
          if (EmulatorConfig.Instance.PHONE_EVENT_MODE == EmulatorConfig.Mode.USB) {
            Debug.LogFormat("Attempting to set up port forwarding.");
            setupPortForwarding(kPhoneEventPort);
          }
          Debug.LogFormat("Attempting to connect to {0}:{1}.", addr, kPhoneEventPort);
          TcpClient tcpClient = new TcpClient(addr, kPhoneEventPort);
          Debug.Log("Connected to phone event socket: " + addr + ":" + kPhoneEventPort);
          connected = true;
          ProcessConnection(tcpClient);
          tcpClient.Close();
          Debug.Log("Disconnected.");
          connected = false;
          continue;
        } catch (Exception e) {
          Debug.LogWarning("Error connecting to phone event socket: " + addr + ":"
              + kPhoneEventPort + ". " + e);
        }
      }
    }

    private void ProcessConnection(TcpClient tcpClient) {
      byte[] buffer = new byte[4];
      NetworkStream stream = tcpClient.GetStream();
      stream.ReadTimeout = kSocketReadTimeoutMillis;
      tcpClient.ReceiveTimeout = kSocketReadTimeoutMillis;
      while (!shouldStop) {
        int bytesRead = blockingRead(stream, buffer, 0, 4);
        if (bytesRead < 4) {
          Debug.LogWarning("Socket read failed, considering socket to have been disconnected.");
          return;
        }
        int msgLen = unpack32bits(correctEndianness(buffer), 0);

        byte[] dataBuffer = new byte[msgLen];
        bytesRead = blockingRead(stream, dataBuffer, 0, msgLen);
        if (bytesRead < msgLen) {
          Debug.LogWarning("Socket read failed, considering socket to have been disconnected.");
          return;
        }

        PhoneEvent proto =
            PhoneEvent.CreateBuilder().MergeFrom(dataBuffer).Build();
        phoneRemote.OnPhoneEvent(proto);

        // Debug.Log(BitConverter.ToString(dataBuffer));
        // Debug.Log(proto.Type);
      }
    }

    private int blockingRead(NetworkStream stream, byte[] buffer, int index,
        int count) {
      int bytesRead = 0;
      while (!shouldStop && bytesRead < count) {
        try {
          int n = stream.Read(buffer, index + bytesRead, count - bytesRead);
          if (n <= 0) {
            // Failed to read.
            return -1;
          }
          bytesRead += n;
        } catch (IOException) {
          // Read failed or timed out.
          return -1;
        } catch (ObjectDisposedException) {
          // Socket closed.
          return -1;
        }
      }
      return bytesRead;
    }

    void OnDestroy() {
      Debug.Log("Shutting down ClientSocket.");
      shouldStop = true;

      if (phoneMirroringSocket != null) {
        phoneMirroringSocket.Close ();
        phoneMirroringSocket = null;
      }

      if (phoneEventThread != null) {
        phoneEventThread.Join();
      }
    }

    private int unpack32bits(byte[] array, int offset) {
      int num = 0;
      for (int i = 0; i < 4; i++) {
        num += array [offset + i] << (i * 8);
      }
      return num;
    }

    static private byte[] correctEndianness(byte[] array) {
      if (BitConverter.IsLittleEndian)
        Array.Reverse(array);

      return array;
    }
  }
}
/// @endcond
