//-----------------------------------------------------------------------
// <copyright file="DemoObjectController6DoF.cs" company="Google Inc.">
// Copyright 2018 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//         http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleVR.Hello6DoFController
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using System.Collections.Generic;

    public class DemoObjectController6DoF : MonoBehaviour
    {
        private Vector3 startingPosition;
        private Vector3 startingScale;
        private bool isLockedToController;
        private Renderer myRenderer;

        public Material inactiveMaterial;
        public Material gazedAtMaterial;

        private GvrTrackedController grabController;

        void Start()
        {
            startingPosition = transform.position;
            startingScale = transform.localScale;
            myRenderer = GetComponent<Renderer>();
            SetGazedAt(false);
        }

        public void UpdateStartPosition()
        {
            startingPosition = transform.position;
            startingScale = transform.localScale;
        }

        private void Update()
        {
            Vector3 targetPos = startingPosition;
            Quaternion targetRotation = Quaternion.identity;
            Vector3 targetScale = startingScale;

            if (grabController != null)
            {
                targetRotation = grabController.transform.rotation;
                targetPos = grabController.transform.position;

                // Offset the object 15cm down the pointing axis of the controller
                // to place it in front of the controller.
                targetPos += targetRotation * Vector3.forward * 0.20f;

                // Shrink the object down a bit while "gripped".
                targetScale *= 0.5f;
                if (Vector3.Distance(targetPos, transform.position) < 0.01f)
                {
                    isLockedToController = true;
                }
            }

            float interpAmount = 1;
            if (!isLockedToController)
            {
                interpAmount = 1 - Mathf.Pow(0.01f, 4 * Time.deltaTime);
            }

            transform.position = Vector3.Lerp(transform.position, targetPos, interpAmount);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, interpAmount);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, interpAmount);
        }

        // Hook this up to Event Trigger PointerEnter with the checkbox checked
        // and PointerExit with the checkbox unchecked.
        public void SetGazedAt(bool gazedAt)
        {
            if (inactiveMaterial != null && gazedAtMaterial != null)
            {
                myRenderer.material = gazedAt ? gazedAtMaterial : inactiveMaterial;
            }
        }

        // Hook this up to Event Trigger PointerDown.
        public void GripStartGrab(BaseEventData eventData)
        {
            PointerEventData ptrEventData = eventData as PointerEventData;
            if (ptrEventData != null &&
                    (ptrEventData.GvrGetButtonsDown() & GvrControllerButton.Grip) != 0)
            {
                grabController = GvrPointerInputModule.Pointer.GetComponentInParent<GvrTrackedController>();
                isLockedToController = false;
            }
        }

        // Hook this up to Event Trigger PointerUp.
        public void GripEndGrab(BaseEventData eventData)
        {
            PointerEventData ptrEventData = eventData as PointerEventData;
            if (ptrEventData != null &&
                (ptrEventData.GvrGetButtonsDown() & GvrControllerButton.Grip) != 0)
            {
                grabController = null;
                isLockedToController = false;
            }
        }

        // Hook this up to Event Trigger PointerClick.
        public void ClickTeleport(BaseEventData eventData)
        {
            PointerEventData ptrEventData = eventData as PointerEventData;
            if (ptrEventData != null &&
                    ptrEventData.button == PointerEventData.InputButton.Left)
            {
                TeleportRandomly();
            }
        }

        private void TeleportRandomly()
        {
            // Pick a random sibling, move them somewhere random, activate them,
            // deactivate ourself.
            int sibIdx = transform.GetSiblingIndex();
            int numSibs = transform.parent.childCount;
            sibIdx = (sibIdx + Random.Range(1, numSibs)) % numSibs;
            GameObject randomSib = transform.parent.GetChild(sibIdx).gameObject;

            // Move to random new location ±90˚ horzontal.
            Vector3 direction = Quaternion.Euler(
                    0,
                    Random.Range(-90, 90),
                    0) * Vector3.forward;

            // New location between 1m and 2m.
            float distance = Random.Range(1, 2);
            Vector3 newPos = direction * distance;

            // Limit vertical position to be fully in the room.
            newPos.y = Mathf.Clamp(newPos.y, -1.2f, 4f);
            randomSib.transform.localPosition = newPos;
            randomSib.GetComponent<DemoObjectController6DoF>().UpdateStartPosition();
            randomSib.SetActive(true);
            gameObject.SetActive(false);
            SetGazedAt(false);
        }
    }
}
