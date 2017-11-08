// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using UnityEditor;
using System.Collections;

#pragma warning disable 0618 // Ignore GvrAudio* deprecation

/// A custom editor for properties on the GvrAudioSoundfield script. This appears in the Inspector
/// window of a GvrAudioSoundfield object.
[CustomEditor(typeof(GvrAudioSoundfield))]
[CanEditMultipleObjects]
public class GvrAudioSoundfieldEditor : Editor {
  private SerializedProperty clip0102 = null;
  private SerializedProperty clip0304 = null;
  private SerializedProperty loop = null;
  private SerializedProperty mute = null;
  private SerializedProperty pitch = null;
  private SerializedProperty playOnAwake = null;
  private SerializedProperty priority = null;
  private SerializedProperty spatialBlend = null;
  private SerializedProperty volume = null;
  private SerializedProperty dopplerLevel = null;
  private SerializedProperty rolloffMode = null;
  private SerializedProperty maxDistance = null;
  private SerializedProperty minDistance = null;
  private SerializedProperty bypassRoomEffects = null;
  private SerializedProperty gainDb = null;

  private GUIContent clip0102Label = new GUIContent("Channels 1 & 2 (WY)",
      "The AudioClip asset for the 1 & 2 channels (W & Y components) of the first-order " +
      "ambisonic soundfield. Channels must be in Ambix (ACN/SN3D) format.");
  private GUIContent clip0304Label = new GUIContent("Channels 3 & 4 (ZX)",
      "The AudioClip asset for the 3 & 4 channels (Z & X components) of the first-order " +
      "ambisonic soundfield. Channels must be in Ambix (ACN/SN3D) format.");
  private GUIContent loopLabel = new GUIContent("Loop",
      "Sets the soundfield to loop.");
  private GUIContent muteLabel = new GUIContent("Mute",
      "Mutes the sound.");
  private GUIContent pitchLabel = new GUIContent("Pitch",
      "Sets the frequency of the sound. Use this to slow down or speed up the sound.");
  private GUIContent priorityLabel = new GUIContent("Priority",
      "Sets the priority of the soundfield. Note that a sound with a larger priority value will " +
      "more likely be stolen by sounds with smaller priority values.");
  private GUIContent spatialBlendLabel = new GUIContent("Spatial Blend",
      "Sets how much this soundfield is treated as a 3D source. Setting this value to 0 will " +
      "ignore distance attenuation and doppler effects. However, it does not affect panning the " +
      "sound around the listener.");
  private GUIContent volumeLabel = new GUIContent("Volume",
      "Sets the overall volume of the soundfield.");
  private GUIContent dopplerLevelLabel = new GUIContent("Doppler Level",
      "Specifies how much the pitch is changed based on the relative velocity between the " +
      "soundfield and the listener.");
  private GUIContent rolloffModeLabel = new GUIContent("Volume Rolloff",
      "Which type of rolloff curve to use.");
  private GUIContent maxDistanceLabel = new GUIContent("Max Distance",
      "Max distance is the distance a sound stops attenuating at.");
  private GUIContent minDistanceLabel = new GUIContent("Min Distance",
      "Within the min distance, the volume will stay at the loudest possible. " +
      "Outside this min distance it will begin to attenuate.");
  private GUIContent playOnAwakeLabel = new GUIContent("Play On Awake",
      "Play the sound when the scene loads.");
  private GUIContent bypassRoomEffectsLabel = new GUIContent("Bypass Room Effects",
    "Sets whether the room effects for the soundfield should be bypassed.");
  private GUIContent gainLabel = new GUIContent("Gain (dB)",
      "Applies a gain to the soundfield for adjustment of relative loudness.");

  void OnEnable () {
    clip0102 = serializedObject.FindProperty("soundfieldClip0102");
    clip0304 = serializedObject.FindProperty("soundfieldClip0304");
    loop = serializedObject.FindProperty("soundfieldLoop");
    mute = serializedObject.FindProperty("soundfieldMute");
    pitch = serializedObject.FindProperty("soundfieldPitch");
    playOnAwake = serializedObject.FindProperty("playOnAwake");
    priority = serializedObject.FindProperty("soundfieldPriority");
    spatialBlend = serializedObject.FindProperty("soundfieldSpatialBlend");
    volume = serializedObject.FindProperty("soundfieldVolume");
    dopplerLevel = serializedObject.FindProperty("soundfieldDopplerLevel");
    rolloffMode = serializedObject.FindProperty("soundfieldRolloffMode");
    maxDistance = serializedObject.FindProperty("soundfieldMaxDistance");
    minDistance = serializedObject.FindProperty("soundfieldMinDistance");
    bypassRoomEffects = serializedObject.FindProperty("bypassRoomEffects");
    gainDb = serializedObject.FindProperty("gainDb");
  }

  /// @cond
  public override void OnInspectorGUI () {
    serializedObject.Update();

    // Add clickable script field, as would have been provided by DrawDefaultInspector()
    MonoScript script = MonoScript.FromMonoBehaviour (target as MonoBehaviour);
    EditorGUI.BeginDisabledGroup (true);
    EditorGUILayout.ObjectField ("Script", script, typeof(MonoScript), false);
    EditorGUI.EndDisabledGroup ();

    EditorGUILayout.LabelField("AudioClip");
    EditorGUI.indentLevel++;
    EditorGUILayout.PropertyField(clip0102, clip0102Label);
    EditorGUILayout.PropertyField(clip0304, clip0304Label);
    EditorGUI.indentLevel--;

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(mute, muteLabel);
    EditorGUILayout.PropertyField(bypassRoomEffects, bypassRoomEffectsLabel);
    EditorGUILayout.PropertyField(playOnAwake, playOnAwakeLabel);
    EditorGUILayout.PropertyField(loop, loopLabel);

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(priority, priorityLabel);

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(volume, volumeLabel);

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(pitch, pitchLabel);

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(spatialBlend, spatialBlendLabel);

    EditorGUILayout.Separator();

    EditorGUILayout.Slider(gainDb, GvrAudio.minGainDb, GvrAudio.maxGainDb, gainLabel);

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(dopplerLevel, dopplerLevelLabel);
    EditorGUILayout.PropertyField(rolloffMode, rolloffModeLabel);
    ++EditorGUI.indentLevel;
    EditorGUILayout.PropertyField(minDistance, minDistanceLabel);
    EditorGUILayout.PropertyField(maxDistance, maxDistanceLabel);
    --EditorGUI.indentLevel;
    if (rolloffMode.enumValueIndex == (int)AudioRolloffMode.Custom) {
      EditorGUILayout.HelpBox("Custom rolloff mode is not supported, no distance attenuation " +
                              "will be applied.", MessageType.Warning);
    }

    EditorGUILayout.Separator();

    serializedObject.ApplyModifiedProperties();
  }
  /// @endcond
}

#pragma warning restore 0618 // Restore warnings
