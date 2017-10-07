# Google VR SDK for Unity

Enables Daydream and Cardboard app development in Unity.

Copyright (c) 2016 Google Inc. All rights reserved.

For updates, known issues, and upgrade instructions, see the
[release-notes](//github.com/googlevr/gvr-unity-sdk/releases).

For first time users, see the Get Started Guides:
 * [Android Cardboard](//developers.google.com/vr/unity/get-started-android)
 * [Android Daydream](//developers.google.com/vr/unity/get-started-controller)
 * [iOS Cardboard](//developers.google.com/vr/unity/get-started-ios)

Please note, we do not accept pull requests.

## Migration to Unity 5.6 from GVR SDK for Unity 1.40 or lower
__Unity versions 5.6 or newer are required as of v1.40 of the GVR SDK for Unity.__

Migration steps:

1. Update the GVR SDK for Unity to [1.40](//github.com/googlevr/gvr-unity-sdk/blob/a3d1033260dab57cb0f4a62a770796fbd09fe37a/GoogleVRForUnity.unitypackage).
2. Migrate to Unity 5.6.0f3, or any newer version. The SDK will import or remove the unnecessary GVR libraries.
3. Update the GVR SDK for Unity to 1.70 (or latest).

## Usage Guide
As of the 1.70 release, the `gvr-unity-sdk` git repo can be cloned and used directly in a Unity project.

* __Downloads__: Get the latest `GoogleVRForUnity_*.unitypackage` from the [releases](//github.com/googlevr/gvr-unity-sdk/releases) page
* __Samples__: [Daydream Elements](//github.com/googlevr/daydream-elements)
* __Workflow__: [Instant Preview](//github.com/googlevr/gvr-instant-preview)

## Pod update to the latest GVR SDK for iOS
As of Unity 5.6, the generated Cocoapod can be updated to the latest GVR SDK for iOS by following these steps.

* Build an Xcode project from Unity.
* In a terminal, change directories into the Xcode project folder.
* Run the following commands
  * ``pod deintegrate``
  * ``pod cache clean --all``
* Change the number in the Podfile to 1.70
* Open the project in Xcode, and delete the Pods directory and Pods.xcodeproj if they exist.
* In the Xcode project settings, change the deployment target to 8.0.
* Quit Xcode
* In Terminal, do ``pod update``
  * "Installing GVRSDK (1.70.0)" should appear in the console.
* ``pod install``

