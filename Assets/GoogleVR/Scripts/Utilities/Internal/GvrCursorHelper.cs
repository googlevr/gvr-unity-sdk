//-----------------------------------------------------------------------
// <copyright file="GvrCursorHelper.cs" company="Google Inc.">
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

using System;
using UnityEngine;

namespace Gvr.Internal
{
    /// Manages cursor lock state while developer is using editor head and controller emulation.
    public class GvrCursorHelper
    {
        // Whether MouseControllerProvider is currently tracking mouse movement.
        private static bool cachedHeadEmulationActive;

        // Whether GvrEditorEmulator is currently tracking mouse movement.
        private static bool cachedControllerEmulationActive;

        public static bool HeadEmulationActive
        {
            set
            {
                cachedHeadEmulationActive = value;
                UpdateCursorLockState();
            }
        }

        public static bool ControllerEmulationActive
        {
            set
            {
                cachedControllerEmulationActive = value;
                UpdateCursorLockState();
            }
        }

        private static void UpdateCursorLockState()
        {
            bool active = cachedHeadEmulationActive || cachedControllerEmulationActive;
            Cursor.lockState = active ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}
