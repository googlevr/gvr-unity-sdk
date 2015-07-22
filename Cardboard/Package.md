The Unity plugin package contains the following:

**Scripts**

* _Cardboard.cs_ - Singleton connection to the native code VR device.
* _CardboardEye.cs_ - Applies an eye view/projection to a stereo camera.
* _CardboardHead.cs_ - Applies the Cardboard head view to its transform.
* _GazeInputModule.cs_ - Control uGUI elements with gaze and trigger.
* _Pose3D.cs_ - Contains a rotation and translation.
* _RadialUndistortionEffect.cs_ - Image Effect simulating distortion correction.
* _StereoController.cs_ - Controls mono vs stereo rendering.

**Editor Scripts**

* _CardboardEditor.cs_ - Customize parameters of the Cardboard object.
* _StereoControllerEditor.cs_ - Adds a button to update the stereo eye cameras.

**Prefabs**

* _CardboardMain.prefab_ - A drop-in replacement for a Main Camera object.
* _CardboardHead.prefab_ - A drop-in replacement for other cameras.
* _CardboardAdapter.prefab_ - Adds stereo rendering to an existing camera.

**Shaders**

* _RadialUndistortionEffect.cs_ - Shader for simulating distortion correction.

**Demo**

* _DemoScene.unity_ - Simple demonstration of the plugin.

**Legacy**

Features intended for supporting existing projects in older versions of Unity.

* _CardboardOnGUI.cs_ - Capture OnGUI onto a texture.
* _CardboardOnGUIWindow.cs_ - Show the OnGUI texture on a mesh in scene.
* _CardboardOnGUIMouse.cs_ - Control mouse by user's gaze; draw mouse pointer.
* _CardboardOnGUI.prefab_ prefab - Make it easy to show and interact with OnGUI in stereo.
* _SkyboxMesh.cs_ - Converts a Camera's skybox to a textured mesh at runtime.
* _StereoLensFlare.cs_ - Support directional lens flares but with parallax.
* _SkyboxMesh.cs_ shader - Unlit textured background.
* _GUIScreen.cs_ shader - Unlit textured overlay with transparency.

@defgroup Scripts
@brief This module covers the specific purpose of the individual scripts and
how they work together, whether within the provided prefabs or if you add them
to the scene yourself.

@defgroup EditorScripts Editor Scripts
@brief This module includes scripts that customize the Inspector panels
for some Cardboard components.

@defgroup LegacyScripts Legacy Scripts
@brief This module describes scripts provided for legacy purposes.
