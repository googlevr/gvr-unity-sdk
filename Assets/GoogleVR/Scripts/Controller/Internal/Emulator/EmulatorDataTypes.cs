//-----------------------------------------------------------------------
// <copyright file="EmulatorDataTypes.cs" company="Google Inc.">
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
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using proto;

/// @cond
namespace Gvr.Internal
{
    struct EmulatorGyroEvent
    {
        public readonly long timestamp;
        public readonly Vector3 value;

        public EmulatorGyroEvent(PhoneEvent.Types.GyroscopeEvent proto)
        {
            timestamp = proto.Timestamp;
            value = new Vector3(proto.X, proto.Y, proto.Z);
        }
    }

    struct EmulatorAccelEvent
    {
        public readonly long timestamp;
        public readonly Vector3 value;

        public EmulatorAccelEvent(PhoneEvent.Types.AccelerometerEvent proto)
        {
            timestamp = proto.Timestamp;
            value = new Vector3(proto.X, proto.Y, proto.Z);
        }
    }

    struct EmulatorTouchEvent
    {
        // Action constants. These should match the constants in the Android
        // MotionEvent:
        // http://developer.android.com/reference/android/view/MotionEvent.html#ACTION_CANCEL
        public enum Action
        {
            kActionDown = 0,
            kActionUp = 1,
            kActionMove = 2,
            kActionCancel = 3,
            kActionPointerDown = 5,
            kActionPointerUp = 6,
            kActionHoverMove = 7,
            kActionHoverEnter = 9,
            kActionHoverExit = 10
        }

        // Use getActionMasked() and getActionPointer() instead.
        private readonly int action;
        public readonly int relativeTimestamp;
        public readonly List<Pointer> pointers;

        public struct Pointer
        {
            public readonly int fingerId;
            public readonly float normalizedX;
            public readonly float normalizedY;

            public Pointer(int fingerId, float normalizedX, float normalizedY)
            {
                this.fingerId = fingerId;
                this.normalizedX = normalizedX;
                this.normalizedY = normalizedY;
            }

            public override string ToString()
            {
                return string.Format("({0}, {1}, {2})", fingerId, normalizedX,
                    normalizedY);
            }
        }

        public EmulatorTouchEvent(PhoneEvent.Types.MotionEvent proto, long lastDownTimeMs)
        {
            action = proto.Action;
            relativeTimestamp =
                (Action)(proto.Action & ACTION_MASK) == Action.kActionDown ?
                    0 : (int)(proto.Timestamp - lastDownTimeMs);
            pointers = new List<Pointer>();
            foreach (PhoneEvent.Types.MotionEvent.Types.Pointer pointer in
                     proto.PointersList)
            {
                pointers.Add(
                    new Pointer(pointer.Id, pointer.NormalizedX, pointer.NormalizedY));
            }
        }

        public EmulatorTouchEvent(Action action, int pointerId, int relativeTimestamp,
                                  List<Pointer> pointers)
        {
            int fingerIndex = 0;
            if (action == Action.kActionPointerDown
                   || action == Action.kActionPointerUp)
            {
                fingerIndex = findPointerIndex(pointerId, pointers);
                if (fingerIndex == -1)
                {
                    Debug.LogWarning("Could not find specific fingerId " + pointerId +
                        " in the supplied list of pointers.");
                    fingerIndex = 0;
                }
            }

            this.action = getActionUnmasked(action, fingerIndex);
            this.relativeTimestamp = relativeTimestamp;
            this.pointers = pointers;
        }

        // See Android's getActionMasked() and getActionIndex().
        private static readonly int ACTION_POINTER_INDEX_SHIFT = 8;
        private static readonly int ACTION_POINTER_INDEX_MASK = 0xff00;
        private static readonly int ACTION_MASK = 0xff;

        public Action getActionMasked()
        {
            return (Action)(action & ACTION_MASK);
        }

        public Pointer getActionPointer()
        {
            int index =
                (action & ACTION_POINTER_INDEX_MASK) >> ACTION_POINTER_INDEX_SHIFT;
            return pointers[index];
        }

        private static int getActionUnmasked(Action action, int fingerIndex)
        {
            return ((int)action) | (fingerIndex << ACTION_POINTER_INDEX_SHIFT);
        }

        private static int findPointerIndex(int fingerId, List<Pointer> pointers)
        {
            // Encode the fingerId info into the action, as Android does. See Android's
            // getActionMasked() and getActionIndex().
            int fingerIndex = -1;
            for (int i = 0; i < pointers.Count; i++)
            {
                if (fingerId == pointers[i].fingerId)
                {
                    fingerIndex = i;
                    break;
                }
            }

            return fingerIndex;
        }

        public override string ToString()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendFormat("t = {0}; A = {1}; P = {2}; N = {3}; [",
                relativeTimestamp, getActionMasked(), getActionPointer().fingerId,
                pointers.Count);
            for (int i = 0; i < pointers.Count; i++)
            {
                builder.Append(pointers[i]).Append(", ");
            }

            builder.Append("]");
            return builder.ToString();
        }
    }

    struct EmulatorOrientationEvent
    {
        public readonly long timestamp;
        public readonly Quaternion orientation;

        public EmulatorOrientationEvent(PhoneEvent.Types.OrientationEvent proto)
        {
            timestamp = proto.Timestamp;

            // Convert from right-handed coordinates to left-handed.
            orientation = new Quaternion(proto.X, proto.Y, -proto.Z, proto.W);
        }
    }

    struct EmulatorButtonEvent
    {
        // Codes as reported by the IC app (reuses Android KeyEvent codes).
        public enum ButtonCode
        {
            kNone = 0,

            // android.view.KeyEvent.KEYCODE_HOME
            kHome = 3,

            // android.view.KeyEvent.KEYCODE_VOLUME_UP
            kVolumeUp = 25,

            // android.view.KeyEvent.KEYCODE_VOLUME_DOWN
            kVolumeDown = 24,

            // android.view.KeyEvent.KEYCODE_ENTER
            kClick = 66,

            // android.view.KeyEvent.KEYCODE_MENU
            kApp = 82,
        }

        public readonly ButtonCode code;
        public readonly bool down;

        public EmulatorButtonEvent(PhoneEvent.Types.KeyEvent proto)
        {
            code = (ButtonCode)proto.Code;
            down = proto.Action == 0;
        }
    }
}

/// @endcond
