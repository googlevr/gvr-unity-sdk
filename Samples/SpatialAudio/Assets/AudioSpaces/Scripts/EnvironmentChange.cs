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
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class EnvironmentChange : MonoBehaviour {

	private GameObject[] environments;
	private PlaySound[] sources = new PlaySound[4];
	private int currentEnvIndex = 0;

	public StereoController mainCamera;
	public string[] levels;
	public PlaySound Guitar;
	public PlaySound Phone;
	public PlaySound Bird;
	public PlaySound Clock;

	void Awake() {
		int numlevels = levels.Length;
		for (int levelIndex = 0; levelIndex < numlevels; ++levelIndex) {
			Application.LoadLevelAdditive(levels[levelIndex]);
		}
	}

	void Start() {
		int numlevels = levels.Length;
		environments = new GameObject[numlevels];
		bool activatedScene = false;
		for (int levelIndex = 0; levelIndex < numlevels; ++levelIndex) {
			string levelName = levels[levelIndex];
			GameObject environment = GameObject.Find(levelName + "_Root");
			if(environment) {
				if(!activatedScene) {
					activatedScene = true;
				}
				else {
					environment.SetActive(false);
				}
			}
			environments[levelIndex] = environment;
			GameObject extra = GameObject.Find(levelName + "_Extra");
			if(extra) {
				GameObject.Destroy(extra);
			}
		}
		sources[0] = Guitar;
		sources[1] = Phone;
		sources[2] = Bird;
		sources[3] = Clock;
		UpdateReferences();
	}


	void Update () {
		RaycastHit hit;
		Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit);
		PlaySound focus = hit.collider ? hit.collider.GetComponent<PlaySound>() : null;

		int numSources = sources.Length;
		for (int sourceIndex = 0; sourceIndex < numSources; ++sourceIndex) {
			PlaySound source = sources[sourceIndex];
			if(source) {
				if(source.pedestal) {
					source.pedestal.material.SetColor("_Color", source == focus ? new Color(3, 3, 3) : Color.white);
				}
				if(source.source) {
					source.sound.transform.position = source.source.transform.position;
					int numMaterials = source.source.materials.Length;
					for(int materialIndex = 0; materialIndex < numMaterials; ++materialIndex) {
						source.source.materials[materialIndex].SetColor("_Color", source.sound.isPlaying ? new Color(3, 3, 3) : Color.white);
					}
				}
			}
		}

	    if (Cardboard.SDK.Triggered) {
			if(focus) {
				if (focus.sound.isPlaying) {
					focus.sound.Stop();
				} else {
					focus.sound.Play();
				}
			}
			else
			{
				// Change environment
				currentEnvIndex = (currentEnvIndex + 1) % environments.Length;
				for (int i = 0; i < environments.Length; i++) {
					environments[i].SetActive(i == currentEnvIndex);
				}
				UpdateReferences();
			}
		}

		if (Cardboard.SDK.BackButtonPressed) {
			Application.Quit();
		}
	}

	private void UpdateReferences() {
		GameObject currentEnv = environments[currentEnvIndex];
		EnvRoot currentEnvRoot = currentEnv.GetComponent<EnvRoot>();
		if(Guitar) {
			Guitar.source = currentEnvRoot.Guitar;
			Guitar.pedestal = currentEnvRoot.GuitarPedestal;
		}
		if(Bird) {
			Bird.source = currentEnvRoot.Bird;
			Bird.pedestal = currentEnvRoot.BirdPedestal;
		}
		if(Clock) {
			Clock.source = currentEnvRoot.Clock;
			Clock.pedestal = currentEnvRoot.ClockPedestal;
		}
		if(Phone) {
			Phone.source = currentEnvRoot.Phone;
			Phone.pedestal = currentEnvRoot.PhonePedestal;
		}
	}
	
	#if UNITY_EDITOR
	[MenuItem("Audio Scenes/Bake Levels")]
	static void BakeLevels()
	{
		EnvironmentChange envChange = GameObject.FindObjectOfType<EnvironmentChange> ();
		if (envChange && envChange.levels != null) {
			int numLevels = envChange.levels.Length;

			for (int levelIndex = 0; levelIndex < numLevels; ++levelIndex) {
				EditorUtility.DisplayProgressBar("Baking Lighting", "Baking " + envChange.levels[levelIndex], (float)levelIndex / (float)numLevels);
				string[] levels = new string[1];
				levels[0] = "Assets/AudioSpaces/Scenes/" + envChange.levels[levelIndex] + ".unity";
				Lightmapping.BakeMultipleScenes (levels);
			}
			EditorUtility.ClearProgressBar();
		}
	}
#endif
}
