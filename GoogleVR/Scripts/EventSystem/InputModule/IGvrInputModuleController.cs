using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// Interface for manipulating an InputModule used by _GvrPointerInputModuleImpl_
public interface IGvrInputModuleController {
  EventSystem eventSystem { get; }
  List<RaycastResult> RaycastResultCache { get; }

  bool ShouldActivate();
  void Deactivate();
  GameObject FindCommonRoot(GameObject g1, GameObject g2);
  BaseEventData GetBaseEventData();
  RaycastResult FindFirstRaycast(List<RaycastResult> candidates);
}
