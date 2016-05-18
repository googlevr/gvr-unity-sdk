# Google VR SDK for Unity - Demo Scenes

Copyright (c) 2016 Google Inc. All rights reserved.

## Headset Demo Scene

File: **Assets/Gvr/DemoScenes/HeadsetDemo/DemoScene.unity**

This demo is a simple scene that uses the headset with gaze input (without a
controller). The "game" consists of locating the cube and clicking the viewer's
trigger button.  Compatible with a traditional Cardboard viewer.

## Controller Demo Scene

File: **Assets/Gvr/DemoScenes/ControllerDemo/ControllerDemo.unity**

This is a demo of how to use the controller. Running this scene on an
actual phone requires an Android N build running on a supported VR device (for
example, a Nexus 6P), and a controller or controller emulator.

If you want to run the scene in the Unity Editor only, you only need a
phone with the controller emulator installed.

For instructions on how download and set up your controller emulator, see
the information about controller support in
[https://developers.google.com/vr](https://developers.google.com/vr).

Summary:

  * The controller phone can be running any version of Android supported
    by the controller emulator app (Lollipop and above, at the time of this
    writing).
  * Install the controller emulator app on the controller phone.

To run the scene in the Unity Editor:

  * Make sure you have the Android SDK installed, and that the ``adb``
    command is in your PATH environment variable.
  * Connect your controller phone to your computer with a USB cable.
  * Verify that you are correctly set up by typing ``adb devices`` on
    a terminal. That should list your device.
  * Click Play on the Unity Editor.

To run the scene on your headset phone:

  * You will need two phones, one for the headset and one to use as a
    controller emulator.
  * The headset phone must be running an Android N build.
  * Set up a WiFi hotspot on your controller phone, and have your headset
    phone connect to it (remove your SIM card if you want to avoid mobile
    data charges).
  * Make sure you've gone through the necessary setup steps to enable
    controller emulator support on your headset phone (in particular,
    verify that **Enable Controller Emulator** is enabled in
    **Google VR Services Settings**).
  * Launch the Controller Demo scene on the headset phone.

How to play:

  * Point at cubes using your controller or controller emulator.
  * Hold the controller's touchpad as you move your controller to drag the
    cubes around the scene.
  * Repeat until happiness is achieved.

Note: the controller can only be used on a supported VR-enabled Android device
(at the time of this writing, the Nexus 6P) running a VR-enabled build of
Android N (the latest Android N developer preview build). On any other
device or platform (e.g. unsupported Android device, iOS, desktop, etc), the
controller API will still be present, but will always report the controller as
being disconnected.

