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

using System.Collections.Generic;
using GVR.Gfx;
using UnityEngine;
using UnityEngine.Events;

namespace GVR.Samples.DragonTeam {
  /// <summary>
  /// Primary Input handler for the RTS control scheme demonstrated in the Dragon Team demo scene.
  /// This handles adding and removing objects in the world from the SelectionManager, as well as
  /// issuing simple commands to all selected units. This system includes Drag Selection modes and
  /// handling. Understanding the use of this class depends largely on viewing the rig in the
  /// Dragon Team scene. A "Drag Selection" object with a Hitbox is responsible for maintaining
  /// the list of potential selections in this class.
  /// </summary>
  public class Selection_Input : MonoBehaviour {
    [Tooltip("Reference to the GVR Head object's transform. Used to orient the Drag Selection graphic.")]
    public Transform HeadRotationReference;

    [Tooltip("Default visual for this object, when nothing is highlighted.")]
    public GameObject DefaultVisual;

    [Tooltip("Hovered visual for this object when a selectable object is hovered over.")]
    public GameObject HoveredVisual;

    [Tooltip("The gameobject containing the Collider for the Drag Selection bounds.")]
    public GameObject DragSelectBounds;

    [Tooltip("Reference to the world space projector component that creates the projected visual " +
             "for the Selection box.")]
    public WorldSpaceProjectorBounds DragSelectProjection;

    [Tooltip("Minimum drag distance from the down click position for the system to switch into " +
             "Drag Selection mode.")]
    public float MinDragDistance = 0.5f;

    [Tooltip("Event called when a new object is registered for Selection. Used for FX and SFX.")]
    public UnityEvent OnSelectionInput;

    [Tooltip("Event called when a move command is issued to selected units. Used for FX and SFX.")]
    public UnityEvent OnMoveCommand;

    [Tooltip("Called once per unit of distance dragged, while dragging is occuring.")]
    public UnityEvent WhileDragging;

    [Tooltip("The minimum distance dragged before WhileDragging is called, and the counter resets. " +
             "This is typically used for the dragging SFX.")]
    public float DistancePerTick = 0.1f;

    //Used for the WhileDragging event. Updates based on motion & distance while a drag is occuring.
    private int ticksFromDownClick = 0;

    private bool clicked = false;
    private bool dragSelectMode = false;

    private Vector3 defaultSelectionScale;

    private List<Selectable> potentialSelections = new List<Selectable>();

    private Vector3 downClickPos;

    void Start() {
      defaultSelectionScale = DragSelectBounds.transform.localScale;
    }

    void Update() {
      //If we're clicked, determine if we should be in Drag select mode or not.
      if (clicked) {
        dragSelectMode = (Vector3.Distance(downClickPos, transform.position) >= MinDragDistance);
      } else {
        //Update the rotation of our object to match the Heads.
        Vector3 newAngles = transform.localEulerAngles;
        newAngles.y = HeadRotationReference.rotation.eulerAngles.y;
        transform.localEulerAngles = newAngles;
      }

      //Always Update our Drag Selection Visuals.
      UpdateDragSelectionVisual();

      //Determine if we're highlighting a potential selection right now, and update our visuals accordingly.
      bool hovered = potentialSelections.Count > 0;

      DefaultVisual.SetActive(!hovered);
      HoveredVisual.SetActive(hovered);
    }

    public void OnHoverEnter(Transform target) {
      Selectable selectable = target.GetComponent<Selectable>();
      if (selectable && selectable.IsSelectable && !potentialSelections.Contains(selectable)) {
        potentialSelections.Add(selectable);
        selectable.SetHighlightStatus(true);
      }
    }

    public void OnHoverExit(Transform target) {
      Selectable selectable = target.GetComponent<Selectable>();
      if (selectable) {
        potentialSelections.Remove(selectable);
        selectable.SetHighlightStatus(false);
      }
    }

    public void DownClick() {
      clicked = true;
      dragSelectMode = false;
      downClickPos = transform.position;
    }

    public void CancelDragSelect() {
      dragSelectMode = false;
      clicked = false;
      potentialSelections.Clear();
    }

    void ResetSelectionObject() {
      DragSelectBounds.transform.localScale = defaultSelectionScale;
      DragSelectBounds.transform.localPosition = Vector3.zero;
    }

    public void UpClick() {
      //If we've canceled our clicked state, also cancel the click up.
      if (clicked == false)
        return;
      if (!dragSelectMode) {
        ProcessSingleClick();
      } else {
        SelectAllPotential();
      }
      clicked = false;
      dragSelectMode = false;
    }

    void ProcessSingleClick() {
      if (potentialSelections.Count > 0) {
        SelectSingleUnit();
      } else {
        IssueMoveCommand();
      }
    }

    void UpdateDragSelectionVisual() {
      if (dragSelectMode) {
        //Update the Drag Selection Visuals and Projections
        Vector3 posA = downClickPos;
        Vector3 posB = transform.position;
        Vector3 center = (posA + posB) / 2;
        center.y = Mathf.Min(posA.y, posB.y);

        Vector3 localA = transform.InverseTransformPoint(downClickPos);
        Vector3 localB = Vector3.zero;
        Vector3 localScale = new Vector3(Mathf.Abs(localB.x - localA.x),
                                         DragSelectBounds.transform.localScale.y,
                                         Mathf.Abs(localB.z - localA.z));

        DragSelectBounds.transform.position = center;
        DragSelectBounds.transform.localScale = localScale;
        DragSelectProjection.transform.localEulerAngles =
            new Vector3(DragSelectProjection.transform.localEulerAngles.x,
                        transform.localEulerAngles.y,
                        DragSelectProjection.transform.localEulerAngles.z);
        DragSelectProjection.transform.position = center;
        DragSelectProjection.size.x = localScale.x;
        DragSelectProjection.size.y = localScale.z;

        //Update the Tick Count for the While Dragging Event, to determine how much we've moved.
        float distanceFromClick = Vector3.Distance(downClickPos, transform.position);
        int newTickCount = Mathf.FloorToInt(distanceFromClick / DistancePerTick);
        if (newTickCount != ticksFromDownClick) {
          ticksFromDownClick = newTickCount;
          WhileDragging.Invoke();
        }
      } else {
        ResetSelectionObject();
        DragSelectProjection.transform.position = new Vector3(-10000, 0, -10000);
      }
    }

    public void IssueMoveCommand() {
      Selectable[] selections = SelectionManager.current.GetSelected();

      for (int i = 0; i < selections.Length; i++) {
        Dragonling dragon = selections[i].GetComponent<Dragonling>();
        if (dragon) {
          if (i == 0) {
            dragon.MoveTo(transform.position);
          } else {
            float invertX = Random.Range(-1f, 1f);
            invertX = invertX < 0 ? -1f : 1f;

            float invertZ = Random.Range(-1f, 1f);
            invertZ = invertZ < 0 ? -1f : 1f;

            dragon.MoveTo(transform.position + new Vector3(Random.Range(0.5f, 1.2f) * invertX, 0,
                                                           Random.Range(0.5f, 1.2f) * invertZ));
          }
        }
      }
      OnMoveCommand.Invoke();
    }

    public void IssueSingCommand() {
      Selectable[] selections = SelectionManager.current.GetSelected();
      for (int i = 0; i < selections.Length; i++) {
        Dragonling dragon = selections[i].GetComponent<Dragonling>();
        if (dragon) {
          dragon.Sing();
        }
      }
    }

    void SelectAllPotential() {
      //Only clear selections if we can select something else.
      if (potentialSelections.Count > 0) {
        SelectionManager.current.ClearSelection();
        for (int i = 0; i < potentialSelections.Count; i++) {
          potentialSelections[i].SetHighlightStatus(false);
          SelectionManager.current.AddToSelected(potentialSelections[i]);
        }
        OnSelectionInput.Invoke();
      }
      potentialSelections.Clear();
    }

    void SelectSingleUnit() {
      Selectable closest = null;
      float bestDist = 0f;
      //Iterate through all potential selections and select the closest.
      for (int i = 0; i < potentialSelections.Count; i++) {
        Selectable sel = potentialSelections[i];
        if (closest == null || Vector3.Distance(closest.transform.position, transform.position) < bestDist) {
          closest = sel;
          bestDist = Vector3.Distance(closest.transform.position, transform.position);
        }
      }
      if (closest) {
        SelectionManager.current.SetSelected(closest);
        OnSelectionInput.Invoke();
      }
    }
  }
}
