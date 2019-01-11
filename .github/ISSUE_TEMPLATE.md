<!-- Use this issue tracker to file bugs and feature requests
related to the Google VR SDK for Unity.

For advice and general questions, please use the `google-cardboard` and
`daydream` tags on Stack Overflow:
- https://stackoverflow.com/questions/tagged/google-cardboard
- https://stackoverflow.com/questions/tagged/daydream
-->


### SPECIFIC ISSUE ENCOUNTERED


### HARDWARE/SOFTWARE VERSIONS
- Unity:
- Google VR SDK for Unity:
- Device manufacturer, model, and O/S:
- Device fingerprint:
  Use `adb shell getprop ro.build.fingerprint`
- Device display metrics:
  Output of `adb shell dumpsys display | grep mBaseDisplayInfo`
- Google VR Services:
  On Windows, use: `adb shell pm dump com.google.vr.vrcore | findstr /i "packages: versionName"`
  On macOS, use: `adb shell pm dump com.google.vr.vrcore | egrep -i versionName\|packages:`
- Viewer manufacturer & model:
- Link to Unity project that reproduces the issue:


### STEPS TO REPRODUCE THE ISSUE
 1.
 1.
 1.

### WORKAROUNDS (IF ANY)


### ADDITIONAL COMMENTS
