using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class TerrainBehaviour : MonoBehaviour {
  public void OnClick(BaseEventData data){
    if (CannonBehaviour.player_ != null) {
      PointerEventData ped = (PointerEventData)data;
      Vector3 target_pos = ped.pointerCurrentRaycast.worldPosition;
      target_pos.y += 0.25f;
      CannonBehaviour.player_.FireAtTarget(target_pos);
    }
  }
}
