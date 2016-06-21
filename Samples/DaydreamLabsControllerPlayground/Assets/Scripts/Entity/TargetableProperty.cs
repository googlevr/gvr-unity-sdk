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

using GVR.Gfx;
using UnityEngine;

namespace GVR.Entity {
  /// <summary>
  ///  This property switches the material on an Interactable Object when the remote
  ///  pointer is focused on it. It works with OutlinedObjectFX to get the effect of
  ///  the object being outlined.
  /// </summary>
  [RequireComponent(typeof(OutlinedObject))]
  public class TargetableProperty : InteractableProperty {
    [Tooltip("Reference to the renderer of the model used for this object.")]
    public Renderer Renderer;

    [Tooltip("The outlined object component attached to this object.")]
    public OutlinedObject OutlinedObjectComponent;

    [Tooltip("The material used to display the model normally.")]
    public Material NormalMaterial;

    [Tooltip("A material used for OutlinedObjectFX highlighting. It's string tag map needs to " +
             "contain 'Oultine'")]
    public Material HighlightMaterial;

    [Tooltip("Reference to the OutlinedObjectFX in the scene.")]
    public OutlinedObjectFX FX_Outline;

    public override bool OnRemotePointEnter() {
      base.OnRemotePointEnter();
      Renderer.material = HighlightMaterial;
      FX_Outline.SetActiveObject(OutlinedObjectComponent);
      return true;
    }

    public override bool OnRemotePointExit() {
      base.OnRemotePointExit();
      Renderer.material = NormalMaterial;
      if (OutlinedObjectComponent.Equals(FX_Outline.GetActiveObject())) {
        FX_Outline.SetActiveObject(null);
      }
      return true;
    }

    public override void TransferOwnership(InteractableObject interactableObject) {
      base.TransferOwnership(interactableObject);
      InteractableObj = interactableObject;
      InteractableObj.Properties.Add(this);
      Renderer.material = NormalMaterial;
    }

    public override void Destruct() {
      FX_Outline.SetActiveObject(null);
      Renderer.material = NormalMaterial;
      base.Destruct();
    }
  }
}
