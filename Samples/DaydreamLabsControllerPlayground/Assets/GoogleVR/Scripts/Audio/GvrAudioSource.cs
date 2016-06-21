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
using UnityEngine.Audio;
using System.Collections;

/// GVR audio source component that enhances AudioSource to provide advanced spatial audio features.
[AddComponentMenu("GoogleVR/Audio/GvrAudioSource")]
public class GvrAudioSource : MonoBehaviour {
  /// Denotes whether the room effects should be bypassed.
  public bool bypassRoomEffects = false;

  /// Directivity pattern shaping factor.
  public float directivityAlpha = 0.0f;

  /// Directivity pattern order.
  public float directivitySharpness = 1.0f;

  /// Input gain in decibels.
  public float gainDb = 0.0f;

  /// Occlusion effect toggle.
  public bool occlusionEnabled = false;

  /// Play source on awake.
  public bool playOnAwake = true;

  /// Volume rolloff model with respect to the distance.
  public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

  /// Spread in degrees.
  [Range(0.0f, 360.0f)]
  public float spread = 0.0f;

  /// The default AudioClip to play.
  public AudioClip clip {
    get { return sourceClip; }
    set {
      sourceClip = value;
      if (audioSource != null) {
        audioSource.clip = sourceClip;
      }
    }
  }
  [SerializeField]
  private AudioClip sourceClip = null;

  /// Is the clip playing right now (Read Only)?
  public bool isPlaying {
    get {
      if (audioSource != null) {
        return audioSource.isPlaying;
      }
      return false;
    }
  }

  /// Is the audio clip looping?
  public bool loop {
    get { return sourceLoop; }
    set {
      sourceLoop = value;
      if (audioSource != null) {
        audioSource.loop = sourceLoop;
      }
    }
  }
  [SerializeField]
  private bool sourceLoop = false;

  /// Un- / Mutes the source. Mute sets the volume=0, Un-Mute restore the original volume.
  public bool mute {
    get { return sourceMute; }
    set {
      sourceMute = value;
      if (audioSource != null) {
        audioSource.mute = sourceMute;
      }
    }
  }
  [SerializeField]
  private bool sourceMute = false;

  /// The pitch of the audio source.
  public float pitch {
    get { return sourcePitch; }
    set {
      sourcePitch = value;
      if (audioSource != null) {
        audioSource.pitch = sourcePitch;
      }
    }
  }
  [SerializeField]
  [Range(-3.0f, 3.0f)]
  private float sourcePitch = 1.0f;

  /// The volume of the audio source (0.0 to 1.0).
  public float volume {
    get { return sourceVolume; }
    set {
      sourceVolume = value;
      if (audioSource != null) {
        audioSource.volume = sourceVolume;
      }
    }
  }
  [SerializeField]
  [Range(0.0f, 1.0f)]
  private float sourceVolume = 1.0f;

  /// MaxDistance is the distance a sound stops attenuating at.
  public float maxDistance {
    get { return sourceMaxDistance; }
    set {
      sourceMaxDistance = Mathf.Clamp(sourceMaxDistance,
                                      sourceMinDistance + GvrAudio.distanceEpsilon,
                                      GvrAudio.maxDistanceLimit);
    }
  }
  [SerializeField]
  private float sourceMaxDistance = 500.0f;

  /// Within the Min distance the GvrAudioSource will cease to grow louder in volume.
  public float minDistance {
    get { return sourceMinDistance; }
    set {
      sourceMinDistance = Mathf.Clamp(value, 0.0f, GvrAudio.minDistanceLimit);
    }
  }
  [SerializeField]
  private float sourceMinDistance = 0.0f;

  /// Binaural (HRTF) rendering toggle.
  [SerializeField]
  private bool hrtfEnabled = true;

  // Unique source id.
  private int id = -1;

  // Current occlusion value;
  private float currentOcclusion = 0.0f;

  // Next occlusion update time in seconds.
  private float nextOcclusionUpdate = 0.0f;

  // Unity audio source attached to the game object.
  private AudioSource audioSource = null;

  // Denotes whether the source is currently paused or not.
  private bool isPaused = false;

  void Awake () {
    audioSource = gameObject.AddComponent<AudioSource>();
    audioSource.enabled = false;
    audioSource.hideFlags = HideFlags.HideInInspector | HideFlags.HideAndDontSave;
    audioSource.playOnAwake = false;
    audioSource.bypassReverbZones = true;
    audioSource.spatialBlend = 0.0f;
    OnValidate();
    // Route the source output to |GvrAudioMixer|.
    AudioMixer mixer = (Resources.Load("GvrAudioMixer") as AudioMixer);
    if(mixer != null) {
      audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
    } else {
      Debug.LogError("GVRAudioMixer could not be found in Resources. Make sure that the GVR SDK" +
                     "Unity package is imported properly.");
    }
  }

  void OnEnable () {
    audioSource.enabled = true;
    if (playOnAwake && !isPlaying) {
      Play();
    }
  }

  void Start () {
    if (playOnAwake && !isPlaying) {
      Play();
    }
  }

  void OnDisable () {
    Stop();
    audioSource.enabled = false;
  }

  void OnDestroy () {
    Destroy(audioSource);
  }

  void Update () {
    // Update occlusion state.
    if (!occlusionEnabled) {
      currentOcclusion = 0.0f;
    } else if (Time.time >= nextOcclusionUpdate) {
      nextOcclusionUpdate = Time.time + GvrAudio.occlusionDetectionInterval;
      currentOcclusion = GvrAudio.ComputeOcclusion(transform);
    }
    // Update source.
    if (!isPlaying && !isPaused) {
      Stop();
    } else {
      GvrAudio.UpdateAudioSource(id, transform, bypassRoomEffects, gainDb, spread, rolloffMode,
                                 sourceMinDistance, sourceMaxDistance, directivityAlpha,
                                 directivitySharpness, currentOcclusion);
    }
  }

  /// Provides a block of the currently playing source's output data.
  ///
  /// @note The array given in samples will be filled with the requested data before spatialization.
  public void GetOutputData(float[] samples, int channel) {
    if (audioSource != null) {
      audioSource.GetOutputData(samples, channel);
    }
  }

  /// Pauses playing the clip.
  public void Pause () {
    if (audioSource != null) {
      isPaused = true;
      audioSource.Pause();
    }
  }

  /// Plays the clip.
  public void Play () {
    if (audioSource != null && InitializeSource()) {
      audioSource.Play();
      isPaused = false;
    }
  }

  /// Plays the clip with a delay specified in seconds.
  public void PlayDelayed (float delay) {
    if (audioSource != null && InitializeSource()) {
      audioSource.PlayDelayed(delay);
      isPaused = false;
    }
  }

  /// Plays an AudioClip.
  public void PlayOneShot (AudioClip clip) {
    PlayOneShot(clip, 1.0f);
  }

  /// Plays an AudioClip, and scales its volume.
  public void PlayOneShot (AudioClip clip, float volume) {
    if (audioSource != null && InitializeSource()) {
      audioSource.PlayOneShot(clip, volume);
      isPaused = false;
    }
  }

  /// Stops playing the clip.
  public void Stop () {
    if (audioSource != null) {
      audioSource.Stop();
      ShutdownSource();
      isPaused = false;
    }
  }

  /// Unpauses the paused playback.
  public void UnPause () {
    if (audioSource != null) {
      audioSource.UnPause();
      isPaused = false;
    }
  }

  // Initializes the source.
  private bool InitializeSource () {
    if (id < 0) {
      id = GvrAudio.CreateAudioSource(hrtfEnabled);
      if (id >= 0) {
        GvrAudio.UpdateAudioSource(id, transform, bypassRoomEffects, gainDb, spread, rolloffMode,
                                   sourceMinDistance, sourceMaxDistance, directivityAlpha,
                                   directivitySharpness, currentOcclusion);
        audioSource.spatialize = true;
        audioSource.SetSpatializerFloat(0, id);
      }
    }
    return id >= 0;
  }

  // Shuts down the source.
  private void ShutdownSource () {
    if (id >= 0) {
      audioSource.SetSpatializerFloat(0, -1.0f);
      audioSource.spatialize = false;
      GvrAudio.DestroyAudioSource(id);
      id = -1;
    }
  }

  void OnDidApplyAnimationProperties () {
    OnValidate();
  }

  void OnValidate () {
    clip = sourceClip;
    loop = sourceLoop;
    mute = sourceMute;
    pitch = sourcePitch;
    volume = sourceVolume;
    minDistance = sourceMinDistance;
    maxDistance = sourceMaxDistance;
  }

  void OnDrawGizmosSelected () {
    Gizmos.color = new Color(0.75f, 0.75f, 1.0f, 0.5f);
    DrawDirectivityGizmo(180);
  }

  // Draws a 3D gizmo in the Scene View that shows the selected directivity pattern.
  private void DrawDirectivityGizmo (int resolution) {
    Vector2[] points =
        GvrAudio.Generate2dPolarPattern(directivityAlpha, directivitySharpness, resolution);
    // Compute |vertices| from the polar pattern |points|.
    int numVertices = resolution + 1;
    Vector3[] vertices = new Vector3[numVertices];
    vertices[0] = Vector3.zero;
    for (int i = 0; i < points.Length; ++i) {
      vertices[i + 1] = new Vector3(points[i].x, 0.0f, points[i].y);
    }
    // Generate |triangles| from |vertices|. Two triangles per each sweep to avoid backface culling.
    int[] triangles = new int[6 * numVertices];
    for (int i = 0; i < numVertices - 1; ++i) {
      int index = 6 * i;
      if (i < numVertices - 2) {
        triangles[index] = 0;
        triangles[index + 1] = i + 1;
        triangles[index + 2] = i + 2;
      } else {
        // Last vertex is connected back to the first for the last triangle.
        triangles[index] = 0;
        triangles[index + 1] = numVertices - 1;
        triangles[index + 2] = 1;
      }
      // The second triangle facing the opposite direction.
      triangles[index + 3] = triangles[index];
      triangles[index + 4] = triangles[index + 2];
      triangles[index + 5] = triangles[index + 1];
    }
    // Construct a new mesh for the gizmo.
    Mesh directivityGizmoMesh = new Mesh();
    directivityGizmoMesh.hideFlags = HideFlags.DontSaveInEditor;
    directivityGizmoMesh.vertices = vertices;
    directivityGizmoMesh.triangles = triangles;
    directivityGizmoMesh.RecalculateNormals();
    // Draw the mesh.
    Vector3 scale = 2.0f * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z) * Vector3.one;
    Gizmos.DrawMesh(directivityGizmoMesh, transform.position, transform.rotation, scale);
  }
}
