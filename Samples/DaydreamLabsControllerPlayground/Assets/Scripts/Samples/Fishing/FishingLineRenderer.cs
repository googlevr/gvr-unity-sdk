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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GVR.Samples.Fishing {
  /// <summary>
  /// Renders a smooth line between a fishing line and fishing rod.
  /// </summary>
  public class FishingLineRenderer : MonoBehaviour {
    [Tooltip("Number of samples taken to smooth the line.")]
    public int SmoothingSize = 50;

    [Tooltip("Y-Axis World position of the water (for dropping the line).")]
    public float WaterY;

    [Header("Line Visuals")]
    public Color LineColor = Color.white;
    public Material LineMaterial;
    public float LineWidth = 0.005f;

    [Header("Line Anchors")]
    public Transform RodTop;
    public Transform MidRod;
    public Transform RodBase;

    void Awake() {
      _state = FishingLureState.ReeledIn;
      InitLineRenderer();
    }

    private void InitLineRenderer() {
      _smoothingBuffer = new Vector3[SmoothingSize];
      _lineRenderer = gameObject.AddComponent<LineRenderer>();
      _lineRenderer.material = LineMaterial;
      _lineRenderer.SetColors(LineColor, LineColor);
      _lineRenderer.SetWidth(LineWidth, LineWidth);
      _linePositions = new List<Vector3>();
      AnchorLineToRod();
    }

    void LateUpdate() {
      UpdateFishingLine();
    }

    /// <summary>
    /// Event listener for fishing lure state changes.
    /// </summary>
    /// <param name="state">Active state</param>
    public void OnFishingLineStateChange(FishingLureState state) {
      _state = state;
    }

    private void UpdateFishingLine() {
      // How much has the tip of the fishing rod moved?
      Vector3 tipDelta = RodTop.transform.position - _linePositions[2];
      AnchorLineToRod();
      _lineRenderer.SetVertexCount(_linePositions.Count);
      for (int i = 0; i < _linePositions.Count; i++) {
        _lineRenderer.SetPosition(i, _linePositions[i]);
      }
      // Sample Position
      if (_state == FishingLureState.Casting) {
        UpdateCasting(tipDelta);
      } else if (_state == FishingLureState.Reeling) {
        UpdateReeling();
      } else if (_state == FishingLureState.InWater ||
               _state == FishingLureState.OnGround) {
        UpdateLanded();
      } else if (_state == FishingLureState.ReeledIn) {
        UpdateForReeledIn();
      }
    }

    private void AnchorLineToRod() {
      if (_linePositions.Count < 3) {
        _linePositions = new List<Vector3> {
          RodBase.position,
          MidRod.position,
          RodTop.position
        };
      } else {
        _linePositions[0] = RodBase.position;
        _linePositions[1] = MidRod.position;
        _linePositions[2] = RodTop.position;
      }
    }

    private void UpdateCasting(Vector3 tipDelta) {
      // Only add a new point if there's been enough distance.
      float minDistance = 0.05f * 0.05f;
      if ((_linePositions[_linePositions.Count - 1] - transform.position).sqrMagnitude >= minDistance) {
        _linePositions.Add(transform.position);
      }
      // Take the movement of the tip and spread it out evenly along all of the line's points.
      for (int i = 3; i < _linePositions.Count; i++) {
        // interp should be 0.0 at the linePositions.Count-1 and 1.0 at index 2.
        float interp = 1.0f - (float)(i - 2) / (_linePositions.Count - 3);
        _linePositions[i] += tipDelta * interp;
      }
      // Do some line smoothing near start of the line to "increase the tension".
      for (int i = 3; i + 1 < _linePositions.Count && i < SmoothingSize; i++) {
        _smoothingBuffer[i] = 0.5f * (_linePositions[i - 1] + _linePositions[i + 1]);
      }
      for (int i = 3; i + 1 < _linePositions.Count && i < SmoothingSize; i++) {
        _linePositions[i] = _smoothingBuffer[i];
      }
    }

    private void UpdateReeling() {
      for (int i = 3; i < _linePositions.Count; i++) {
        _linePositions.RemoveAt(i);
      }
      _linePositions.Add(transform.position);
    }

    private void UpdateLanded() {
      // Gravity on line
      for (int i = 3; i < _linePositions.Count; i++) {
        if (_linePositions[i].y > WaterY) {
          float lineFallPerFrame = .03f;
          _linePositions[i] = new Vector3 {
            x = _linePositions[i].x,
            y = _linePositions[i].y - lineFallPerFrame,
            z = _linePositions[i].z
          };
        }
      }
      // Do some line smoothing near start of the line to "increase the tension".
      for (int i = 3; i + 1 < _linePositions.Count && i < SmoothingSize; i++) {
        _smoothingBuffer[i] = 0.5f * (_linePositions[i - 1] + _linePositions[i + 1]);
      }
      for (int i = 3; i + 1 < _linePositions.Count && i < SmoothingSize; i++) {
        _linePositions[i] = _smoothingBuffer[i];
      }
    }

    private void UpdateForReeledIn() {
      if (_linePositions.Count > 3) {
        _linePositions.RemoveAt(_linePositions.Count - 1);
      }
    }

    private FishingLureState _state;
    private LineRenderer _lineRenderer;
    private Vector3[] _smoothingBuffer;
    private List<Vector3> _linePositions;
  }
}
