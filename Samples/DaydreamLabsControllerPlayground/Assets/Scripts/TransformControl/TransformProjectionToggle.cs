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

using UnityEngine;
using UnityEngine.Events;

namespace GVR.TransformControl {
  /// <summary>
  /// Allows a transform to toggle between it's starting depth and a projection onto a plane.
  /// </summary>
  [RequireComponent(typeof(TransformProjector))]
  public class TransformProjectionToggle : MonoBehaviour {
    public UnityEvent OnExtended;

    [Tooltip("Projected plane to extend toward")]
    public TransformProjector Other;

    [Header("Layer Restriction")]
    [Tooltip("If true, only extend the transform if in front of an object in a specified layer")]
    public bool ExtendToLayer;

    [Tooltip("Maximum distance to check if we're in front of the planes we're projecting")]
    public float MaxDistance = 10f;

    [Tooltip("Layer containing the plane/object we are extending to.")]
    public LayerMask Layer;

    void Start() {
      _first = GetComponent<TransformProjector>();
      Retract();
    }

    public void ToggleExtension() {
      if (CanExtend) {
        _extended = !_extended;
        UpdateExtension();
      }
    }

    public void Extend() {
      if (CanExtend) {
        _extended = true;
        UpdateExtension();
        OnExtended.Invoke();
      }
    }

    public void Retract() {
      _extended = false;
      UpdateExtension();
    }

    private bool InFrontOfLayer() {
      Vector3 start = transform.position;
      Vector3 end = transform.TransformDirection(Vector3.forward * MaxDistance);
      return Physics.Raycast(start, end, MaxDistance, Layer);
    }

    private void UpdateExtension() {
      _first.enabled = !_extended;
      Other.enabled = _extended;
    }

    private bool CanExtend {
      get { return !ExtendToLayer || InFrontOfLayer(); }
    }

    private bool _extended;
    private TransformProjector _first;
  }
}
