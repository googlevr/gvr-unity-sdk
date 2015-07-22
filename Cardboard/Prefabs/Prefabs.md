# Prefabs

This section describes the prefabs that are provided by the package.

## CardboardMain

This prefab is intended to be a drop-in replacement for a normal Unity camera,
such as _Main Camera_, primarily when the camera is simple, i.e. not already
festooned with scripts.  A brand new Unity project is a good time to use it.  To
apply it in the Editor, delete the camera in question and replace it with an
instance of this prefab.

The prefab contains a top-level object called _CardboardMain_, which has a
Cardboard script attached to it for controlling VR Mode settings.  Under this is
the _Head_ object, which has a _%CardboardHead_ attached for tracking the user's
head motion.  That in turn has a _Main Camera_ child object, with the
StereoController script attached.  This camera is tagged as a _MainCamera_ so
that the Unity property `Camera.main` will find it.  Finally there are the Left
and Right stereo eye cameras at the bottom of the hierarchy.

## %CardboardHead

This prefab is a replacement for other cameras in the scene that also need
head-tracking.  It has a top-level object called _Head_ with a CardboardHead
script.  Under that is the _Camera_ child which has the StereoController script,
and then the Left and Right stereo eye cameras.  Unlike _CardboardMain_, the
camera in this prefab is not tagged _MainCamera_.

## CardboardAdapter

This prefab is for when you wish to keep your existing Camera object, usually
because it is heavily wired with game logic and already moves exactly as needed
for non-VR game play.  Place this prefab as a child of the camera, and then
execute _Update Stereo Cameras_ (or manually attach StereoController to the
camera) to complete the rig.

Unlike the other prefabs, this setup places the head-tracked node below the Main
Camera.  Therefore, only the stereo cameras are affected by the user's head
motion.

## General Prefab Notes

Each prefab is provided as a starting point for customization.  It is a
framework on which to attach additional scripts for your own needs.  Elements
under a Head node will maintain a fixed position on screen, thus acting like a
Heads-up Display (HUD).  Elements that are siblings of Head will maintain a
fixed position and orientation relative to the overall point of view, thus
acting more like a cockpit instrument panel.
