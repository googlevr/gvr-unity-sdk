//-----------------------------------------------------------------------
// <copyright file="KeyboardDelegateExample.cs" company="Google Inc.">
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

namespace GoogleVR.KeyboardDemo
{
    using UnityEngine;
    using UnityEngine.UI;
    using System;

    public class KeyboardDelegateExample : GvrKeyboardDelegateBase
    {
        public Text KeyboardText;
        public Canvas UpdateCanvas;

        public override event EventHandler KeyboardHidden;

        public override event EventHandler KeyboardShown;

        private const string DD_KEYBOARD_NOT_INSTALLED_MSG = "Please update the Daydream Keyboard app from the Play Store.";

        void Awake()
        {
            if (UpdateCanvas != null)
            {
                UpdateCanvas.gameObject.SetActive(false);
            }
        }

        public override void OnKeyboardShow()
        {
            Debug.Log("Calling Keyboard Show Delegate!");
            EventHandler handler = KeyboardShown;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        public override void OnKeyboardHide()
        {
            Debug.Log("Calling Keyboard Hide Delegate!");
            EventHandler handler = KeyboardHidden;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        public override void OnKeyboardUpdate(string text)
        {
            if (KeyboardText != null)
            {
                KeyboardText.text = text;
            }
            else
            {
                Debug.Log("Keyboard text is null....");
            }
        }

        public override void OnKeyboardEnterPressed(string text)
        {
            Debug.Log("Calling Keyboard Enter Pressed Delegate: " + text);
        }

        public override void OnKeyboardError(GvrKeyboardError errCode)
        {
            Debug.Log("Calling Keyboard Error Delegate: ");
            switch (errCode)
            {
                case GvrKeyboardError.UNKNOWN:
                    Debug.Log("Unknown Error");
                    break;
                case GvrKeyboardError.SERVICE_NOT_CONNECTED:
                    Debug.Log("Service not connected");
                    break;
                case GvrKeyboardError.NO_LOCALES_FOUND:
                    Debug.Log("No locales found");
                    break;
                case GvrKeyboardError.SDK_LOAD_FAILED:
                    Debug.LogWarning(DD_KEYBOARD_NOT_INSTALLED_MSG);
                    if (KeyboardText != null)
                    {
                        KeyboardText.text = DD_KEYBOARD_NOT_INSTALLED_MSG;
                    }

                    if (UpdateCanvas != null)
                    {
                        UpdateCanvas.gameObject.SetActive(true);
                    }

                    break;
            }
        }

        public void LaunchPlayStore()
        {
            if (UpdateCanvas != null)
            {
                UpdateCanvas.gameObject.SetActive(false);
#if !UNITY_ANDROID
                Debug.LogError("GVR Keyboard available only on Android.");
#else
                GvrKeyboardIntent.Instance.LaunchPlayStore();
#endif  // !UNITY_ANDROID
            }
        }
    }
}
