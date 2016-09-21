using UnityEngine;

/// <summary>
/// Provides information about the Android Intent that started the current Activity.
/// </summary>
public static class GvrIntent {

#if UNITY_ANDROID
  private const string PACKAGE_UNITY_PLAYER = "com.unity3d.player.UnityPlayer";
  private const string METHOD_CURRENT_ACTIVITY = "currentActivity";
  private const string METHOD_GET_INTENT = "getIntent";
  private const string METHOD_INTENT_GET_DATA_STRING = "getDataString";
  private const string METHOD_INTENT_HAS_CATEGORY = "hasCategory";

  private const string CATEGORY_DAYDREAM = "com.google.intent.category.DAYDREAM";

  // Returns the string representation of the data URI on which this activity's intent is
  // operating. See Intent.getDataString() in the Android documentation.
  public static string GetData() {
#if UNITY_EDITOR
    return null;
#else
    AndroidJavaObject androidIntent = GetIntent();
    if (androidIntent == null) {
      Debug.Log("Intent on current activity was null");
      return null;
    }
    return androidIntent.Call<string>(METHOD_INTENT_GET_DATA_STRING);
#endif  // UNITY_EDITOR
  }

  // Returns true if the intent category contains com.google.intent.category.DAYDREAM.
  public static bool IsLaunchedFromVr() {
#if UNITY_EDITOR
    return false;
#else
    AndroidJavaObject androidIntent = GetIntent();
    if (androidIntent == null) {
      Debug.Log("Intent on current activity was null");
      return false;
    }
    return androidIntent.Call<bool>(METHOD_INTENT_HAS_CATEGORY, CATEGORY_DAYDREAM);
#endif  // UNITY_EDITOR
  }

  private static AndroidJavaObject GetIntent() {
#if UNITY_EDITOR
    return null;
#else
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
#endif  //!UNITY_EDITOR
  }

#endif  // UNITY_ANDROID
}
