//-----------------------------------------------------------------------
// <copyright file="EmulatorKeyboardProvider.cs" company="Google Inc.">
// Copyright 2017 Google Inc. All rights reserved.
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
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------
// This is a version of the keyboard that runs directly in the Unity Editor.
// It is meant to simply be a placeholder so developers can test their games
// without having to use actual devices.
using UnityEngine;
using System;

/// @cond
namespace Gvr.Internal
{
    /// Keyboard subclass to run in the Unity editor
    public class EmulatorKeyboardProvider : IKeyboardProvider
    {
        private GameObject stub;
        private bool showing;

        GvrKeyboard.KeyboardCallback keyboardCallback;

        private string editorText = string.Empty;
        private GvrKeyboardInputMode mode = GvrKeyboardInputMode.DEFAULT;
        private Matrix4x4 worldMatrix;
        private bool isValid = false;

        public string EditorText
        {
            get { return editorText; }
            set { editorText = value; }
        }

        public void SetInputMode(GvrKeyboardInputMode mode)
        {
            this.mode = mode;
        }

        public EmulatorKeyboardProvider()
        {
            Debug.Log("Creating stub keyboard");

            // Set default data;
            showing = false;
            isValid = true;
        }

        public void OnPause()
        {
        }

        public void OnResume()
        {
        }

        public void ReadState(KeyboardState outState)
        {
            outState.mode = mode;
            outState.editorText = editorText;
            outState.worldMatrix = worldMatrix;
            outState.isValid = isValid;
            outState.isReady = true;
        }

        public bool Create(GvrKeyboard.KeyboardCallback keyboardEvent)
        {
            keyboardCallback = keyboardEvent;

            if (!isValid)
            {
                keyboardCallback(IntPtr.Zero, GvrKeyboardEvent.GVR_KEYBOARD_ERROR_SERVICE_NOT_CONNECTED);
            }

            return true;
        }

        public void Show(Matrix4x4 controllerMatrix, bool useRecommended, float distance, Matrix4x4 model)
        {
            if (!showing && isValid)
            {
                showing = true;
                worldMatrix = controllerMatrix;
                keyboardCallback(IntPtr.Zero, GvrKeyboardEvent.GVR_KEYBOARD_SHOWN);
            }
        }

        public void UpdateData()
        {
            // Can skip if keyboard not available
            if (!showing)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                keyboardCallback(IntPtr.Zero, GvrKeyboardEvent.GVR_KEYBOARD_TEXT_COMMITTED);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (editorText.Length > 0)
                {
                    editorText = editorText.Substring(0, editorText.Length - 1);
                    SendUpdateNotification();
                }

                return;
            }

            if (Input.inputString.Length <= 0)
            {
                return;
            }

            switch (mode)
            {
                case GvrKeyboardInputMode.DEFAULT:
                    editorText += Input.inputString;
                    break;
                case GvrKeyboardInputMode.NUMERIC:
                    foreach (char n in Input.inputString)
                    {
                        if (n >= '0' && n <= '9')
                        {
                            editorText += n;
                        }
                    }

                    break;
                default:
                    break;
            }

            SendUpdateNotification();
        }

        public void Render(int eye, Matrix4x4 modelview, Matrix4x4 projection, Rect viewport)
        {
        }

        public void Hide()
        {
            if (showing)
            {
                showing = false;
                keyboardCallback(IntPtr.Zero, GvrKeyboardEvent.GVR_KEYBOARD_HIDDEN);
            }
        }

        private void SendUpdateNotification()
        {
            keyboardCallback(IntPtr.Zero, GvrKeyboardEvent.GVR_KEYBOARD_TEXT_UPDATED);
        }
    }
}
