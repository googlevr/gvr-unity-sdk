//-----------------------------------------------------------------------
// <copyright file="ScrubberEvents.cs" company="Google Inc.">
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

namespace GoogleVR.VideoDemo
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class ScrubberEvents : MonoBehaviour
    {
        private GameObject newPositionHandle;

        private Vector3[] corners;
        private Slider slider;

        private VideoControlsManager mgr;

        public VideoControlsManager ControlManager
        {
            set
            {
                mgr = value;
            }
        }

        void Start()
        {
            foreach (Image im in GetComponentsInChildren<Image>(true))
            {
                if (im.gameObject.name == "newPositionHandle")
                {
                    newPositionHandle = im.gameObject;
                    break;
                }
            }

            corners = new Vector3[4];
            GetComponent<Image>().rectTransform.GetWorldCorners(corners);
            slider = GetComponentInParent<Slider>();
        }

        void Update()
        {
            bool setPos = false;
            if (GvrPointerInputModule.Pointer != null)
            {
                RaycastResult r = GvrPointerInputModule.Pointer.CurrentRaycastResult;
                if (r.gameObject != null)
                {
                    newPositionHandle.transform.position = new Vector3(
                        r.worldPosition.x,
                        newPositionHandle.transform.position.y,
                        newPositionHandle.transform.position.z);
                    setPos = true;
                }
            }

            if (!setPos)
            {
                newPositionHandle.transform.position = slider.handleRect.transform.position;
            }
        }

        public void OnPointerEnter(BaseEventData data)
        {
            if (GvrPointerInputModule.Pointer != null)
            {
                RaycastResult r = GvrPointerInputModule.Pointer.CurrentRaycastResult;
                if (r.gameObject != null)
                {
                    newPositionHandle.transform.position = new Vector3(
                        r.worldPosition.x,
                        newPositionHandle.transform.position.y,
                        newPositionHandle.transform.position.z);
                }
            }

            newPositionHandle.SetActive(true);
        }

        public void OnPointerExit(BaseEventData data)
        {
            newPositionHandle.SetActive(false);
        }

        public void OnPointerClick(BaseEventData data)
        {
            float minX = corners[0].x;
            float maxX = corners[3].x;

            float pct = (newPositionHandle.transform.position.x - minX) / (maxX - minX);

            if (mgr != null)
            {
                long p = (long)(slider.maxValue * pct);
                mgr.Player.CurrentPosition = p;
            }
        }
    }
}
