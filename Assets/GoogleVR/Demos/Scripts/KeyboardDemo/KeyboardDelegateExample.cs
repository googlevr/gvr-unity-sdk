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
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// An example `GvrKeyboardDelegateBase` implementation for use in the Keyboard Demo scene.
    /// </summary>
    public class KeyboardDelegateExample : GvrKeyboardDelegateBase
    {
        /// <summary>The GUI text associated with this keyboard.</summary>
        public Text keyboardText;

        /// <summary>The canvas associated with the `keyboardText` and this keyboard.</summary>
        public Canvas updateCanvas;

        private const string DD_KEYBOARD_NOT_INSTALLED_MSG =
            "Please update the Daydream Keyboard app from the Play Store.";

        /// <summary>Occurs when keyboard hidden.</summary>
        public override event EventHandler KeyboardHidden;

        /// <summary>Occurs when keyboard shown.</summary>
        public override event EventHandler KeyboardShown;

        /// <summary>Handles the keyboard show event.</summary>
        public override void OnKeyboardShow()
        {
            Debug.Log("Calling Keyboard Show Delegate!");
            EventHandler handler = KeyboardShown;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        /// <summary>
        /// An event which triggers the `KeyboardHidden` event when the keyboard is no longer in
        /// use.
        /// </summary>
        public override void OnKeyboardHide()
        {
            Debug.Log("Calling Keyboard Hide Delegate!");
            EventHandler handler = KeyboardHidden;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        /// <summary>
        /// A keyboard update event which sets the `keyboardText` to the keyboard-entered string.
        /// </summary>
        /// <param name="text">The text currently typed in the Keyboard.</param>
        public override void OnKeyboardUpdate(string text)
        {
            if (keyboardText != null)
            {
                keyboardText.text = text;
            }
            else
            {
                Debug.Log("Keyboard text is null....");
            }
        }

        /// <summary>
        /// A KeyboardEnterPressed event which triggers when the ENTER button is pressed.
        /// </summary>
        /// <param name="text">The text currently typed in the keyboard.</param>
        public override void OnKeyboardEnterPressed(string text)
        {
            Debug.Log("Calling Keyboard Enter Pressed Delegate: " + text);
        }

        /// <summary>
        /// A KeyboardError event which triggers when an error occurs related to the keyboard.
        /// </summary>
        /// <param name="errCode">The GvrKeyboardError which triggered this event.</param>
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
                    if (keyboardText != null)
                    {
                        keyboardText.text = DD_KEYBOARD_NOT_INSTALLED_MSG;
                    }

                    if (updateCanvas != null)
                    {
                        updateCanvas.gameObject.SetActive(true);
                    }

                    break;
            }
        }

        /// <summary>Launches the Play Store to download the necessary Keyboard apps.</summary>
        public void LaunchPlayStore()
        {
            if (updateCanvas != null)
            {
                updateCanvas.gameObject.SetActive(false);
#if !UNITY_ANDROID
                Debug.LogError("GVR Keyboard available only on Android.");
#else
                GvrKeyboardIntent.Instance.LaunchPlayStore();
#endif  // !UNITY_ANDROID
            }
        }

        private void Awake()
        {
            if (updateCanvas != null)
            {
                updateCanvas.gameObject.SetActive(false);
            }
        }
    }
}
