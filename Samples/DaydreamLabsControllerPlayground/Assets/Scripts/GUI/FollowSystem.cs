using UnityEngine;
using System.Collections;

public class FollowSystem : MonoBehaviour {

  private const float CURSOR_UPDATE_THRESHOLD = 0.09f;

  public GameObject follow_slow;
  private Vector3 last_pos;
  private float lastUpdateRealTimeSinceStartup;

  public float t;
	
  void Start() {
    lastUpdateRealTimeSinceStartup = Time.realtimeSinceStartup;
  }

	void Update () {
    if ((Time.realtimeSinceStartup - lastUpdateRealTimeSinceStartup) <= CURSOR_UPDATE_THRESHOLD) {
      transform.position = Vector3.Lerp(last_pos, follow_slow.transform.position, t);
    }
    last_pos = follow_slow.transform.position;
    lastUpdateRealTimeSinceStartup = Time.realtimeSinceStartup;
  }
}
