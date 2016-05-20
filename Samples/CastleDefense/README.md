# Google VR Game Sample

## Description
This example shows how to make a game for Daydream and Cardboard.

## Instructions
To run this you need to make sure you have the Google VR Unity SDK and Unity's
Particle Effects in your project.

### Google VR Unity SDK
To get the Google VR SDK download it from:  
[https://developers.google.com/vr/unity/download](https://developers.google.com/vr/unity/download)

### Unity's Particle Systems
Inside the Unity Project select at the toolbar:
_Edit -> Import Package -> ParticleSystems._

That's all!

## Using the Unity Package
If you want to import the package to a new project, there are a few things you
will need to do to get it to work.

Start by going through the instructions above to ensure Daydream support and
that the particles aren't missing, then follow the steps in the next sections.

### Enabling Audio
The package is using Google VR's spatial audio.  

Inside the Unity Project use the toolbar to go to Go to:  
_Edit -> Project Settings -> Audio_  
Change the _Spatializer Plugin_ to _GVR Audio Spatializer_.

### Performance Optimizations & Settings
The project was optimized for specific settings shown below. Please feel free  
to experiment with other setings.

#### Player Settings
Inside the Unity Project use the toolbar to go to Go to:  
_Edit -> Project Settings -> Player_

Inside _Resolution and Presentation_ select the following settings:  
- **Default Orientation:** Landscape Left.
- **Use 32-bit Display Buffer:** Disabled.

Inside _Other Settings_ are the following:
- **Rendering Path:** Legacy Vertex Lit.
- **Multithreaded Rendering:** Enabled.
- **Static Batching:** Enabled.
- **Dynamic Batching:** Enabled.

#### Quality Settings
Inside the Unity Project use the toolbar to go to Go to:  
_Edit -> Project Settings -> Quality_
  
- **Anti Aliasing:** 2x Multi Sampling.
- **Soft Particles:** Disabled.
- **Realtime Reflection Probes:** Disabled.
- **Shadows:** Disable Shadows.

#### GvrMain
Inside the _Google VR SDK_, the _GvrMain_ (camera) object, consider setting  
the _Distortion Correction_ to **Unity** and _Stereo Screen Scale_ to **0.8**.