using UnityEngine;

/// <summary>
/// Provides information about the Android Intent that started the current Activity.
/// </summary>
public static class GvrIntent {

  private const string PACKAGE_UNITY_PLAYER = "com.unity3d.player.UnityPlayer";
  private const string METHOD_CURRENT_ACTIVITY = "currentActivity";
  private const string METHOD_GET_INTENT = "getIntent";
  private const string METHOD_HASH_CODE = "hashCode";
  private const string METHOD_INTENT_GET_DATA_STRING = "getDataString";
  private const string METHOD_INTENT_GET_BOOLEAN_EXTRA = "getBooleanExtra";

  private const string EXTRA_VR_LAUNCH = "android.intent.extra.VR_LAUNCH";

  // Returns the string representation of the data URI on which this activity's intent is
  // operating. See Intent.getDataString() in the Android documentation.
  public static string GetData() {
#if UNITY_EDITOR || !UNITY_ANDROID
    return null;
#else
    AndroidJavaObject androidIntent = GetIntent();
    if (androidIntent == null) {
      Debug.Log("Intent on current activity was null");
      return null;
    }
    return androidIntent.Call<string>(METHOD_INTENT_GET_DATA_STRING);
#endif  // UNITY_EDITOR || !UNITY_ANDROID
  }

  // Returns true if the intent category contains com.google.intent.category.DAYDREAM.
  public static bool IsLaunchedFromVr() {
#if UNITY_EDITOR || !UNITY_ANDROID
    return false;
#else
    AndroidJavaObject androidIntent = GetIntent();
    if (androidIntent == null) {
      Debug.Log("Intent on current activity was null");
      return false;
    }
    return androidIntent.Call<bool>(METHOD_INTENT_GET_BOOLEAN_EXTRA, EXTRA_VR_LAUNCH, false);
#endif  // UNITY_EDITOR || !UNITY_ANDROID
  }

  // Returns the hash code of the Java intent object.  Useful for discerning whether
  // you have a new intent on un-pause.
  public static int GetIntentHashCode() {
#if UNITY_EDITOR || !UNITY_ANDROID
    return 0;
#else
    AndroidJavaObject androidIntent = GetIntent();
    if (androidIntent == null) {
      Debug.Log("Intent on current activity was null");
      return 0;
    }
    return androidIntent.Call<int>(METHOD_HASH_CODE);
#endif  // UNITY_EDITOR || !UNITY_ANDROID
  }

#if !UNITY_EDITOR && UNITY_ANDROID
  private static AndroidJavaObject GetIntent() {
    AndroidJavaObject androidActivity = null;
    try {
      using (AndroidJavaObject unityPlayer = new AndroidJavaClass(PACKAGE_UNITY_PLAYER)) {
        androidActivity = unityPlayer.GetStatic<AndroidJavaObject>(METHOD_CURRENT_ACTIVITY);
      }
    } catch (AndroidJavaException e) {
      Debug.LogError("Exception while connecting to the Activity: " + e);
      return null;
    }
    return androidActivity.Call<AndroidJavaObject>(METHOD_GET_INTENT);
  }
#endif  // !UNITY_EDITOR && UNITY_ANDROID
}
