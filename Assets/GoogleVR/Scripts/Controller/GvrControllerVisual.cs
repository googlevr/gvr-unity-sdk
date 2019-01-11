//-----------------------------------------------------------------------
// <copyright file="GvrControllerVisual.cs" company="Google Inc.">
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
// </copyright>
//-----------------------------------------------------------------------

// The controller is not available for versions of Unity without the
// GVR native integration.

using UnityEngine;
using System.Collections;
using Gvr.Internal;

/// Provides visual feedback for the daydream controller.
[RequireComponent(typeof(Renderer))]
[HelpURL("https://developers.google.com/vr/unity/reference/class/GvrControllerVisual")]
public class GvrControllerVisual : MonoBehaviour, IGvrArmModelReceiver, IGvrControllerInputDeviceReceiver
{
    /// <summary>The controller display state data structure.</summary>
    [System.Serializable]
    public struct ControllerDisplayState
    {
        /// <summary>The current battery level.</summary>
        public GvrControllerBatteryLevel batteryLevel;

        /// <summary>True if the battery is charging.</summary>
        public bool batteryCharging;

        /// <summary>True if the touch pad button is down.</summary>
        public bool clickButton;

        /// <summary>True if the app button is down.</summary>
        public bool appButton;

        /// <summary>True if the system button is down.</summary>
        public bool homeButton;

        /// <summary>True if the controller is registering a touch.</summary>
        public bool touching;

        /// <summary>The touch position.</summary>
        public Vector2 touchPos;
    }

    /// Struct that describes a mesh, material pair used for rendering a controller visual.
    [System.Serializable]
    public struct VisualAssets
    {
        /// @cond
        public Mesh mesh;
        public Material material;

        /// @endcond
    }

    /// An array of prefabs that will be instantiated and added as children
    /// of the controller visual when the controller is created. Used to
    /// attach tooltips or other additional visual elements to the control dynamically.
    [SerializeField]
    private GameObject[] attachmentPrefabs;

    [SerializeField] private Color touchPadColor =
        new Color(200f / 255f, 200f / 255f, 200f / 255f, 1);

    [SerializeField] private Color appButtonColor =
        new Color(200f / 255f, 200f / 255f, 200f / 255f, 1);

    [SerializeField] private Color systemButtonColor =
        new Color(20f / 255f, 20f / 255f, 20f / 255f, 1);

    /// Determines if the displayState is set from GvrControllerInputDevice.
    [Tooltip("Determines if the displayState is set from GvrControllerInputDevice.")]
    public bool readControllerState = true;

    /// Used to set the display state of the controller visual.
    /// This can be used for tutorials that visualize the controller or other use-cases that require
    /// displaying the controller visual without the state being determined by controller input.
    /// Additionally, it can be used to preview the controller visual in the editor.
    /// NOTE: readControllerState must be disabled to set the display state.
    public ControllerDisplayState displayState;

    /// This is the preferred, maximum alpha value the object should have
    /// when it is a comfortable distance from the head.
    [Range(0.0f, 1.0f)]
    public float maximumAlpha = 1.0f;

    /// <summary>The arm model used to position the controller.</summary>
    public GvrBaseArmModel ArmModel { get; set; }

    /// <summary>The controller device reference.</summary>
    public GvrControllerInputDevice ControllerInputDevice { get; set; }

    /// <summary>The preferred alpha value for the controller.</summary>
    public virtual float PreferredAlpha
    {
        get
        {
            return ArmModel != null ? maximumAlpha * ArmModel.PreferredAlpha : maximumAlpha;
        }
    }

    /// <summary>The touchpad color.</summary>
    public Color TouchPadColor
    {
        get
        {
            return touchPadColor;
        }

        set
        {
            touchPadColor = value;
            if (materialPropertyBlock != null)
            {
                materialPropertyBlock.SetColor(touchPadId, touchPadColor);
            }
        }
    }

    /// <summary>The app button color.</summary>
    public Color AppButtonColor
    {
        get
        {
            return appButtonColor;
        }

        set
        {
            appButtonColor = value;
            if (materialPropertyBlock != null)
            {
                materialPropertyBlock.SetColor(appButtonId, appButtonColor);
            }
        }
    }

    /// <summary>The system button color.</summary>
    public Color SystemButtonColor
    {
        get
        {
            return systemButtonColor;
        }

        set
        {
            systemButtonColor = value;
            if (materialPropertyBlock != null)
            {
                materialPropertyBlock.SetColor(systemButtonId, systemButtonColor);
            }
        }
    }

    private Renderer controllerRenderer;
    private MeshFilter meshFilter;
    private MaterialPropertyBlock materialPropertyBlock;

    private int alphaId;
    private int touchId;
    private int touchPadId;
    private int appButtonId;
    private int systemButtonId;
    private int batteryColorId;

    private bool wasTouching;
    private float touchTime;

    // Data passed to shader, (xy) touch position, (z) touch duration, (w) battery state.
    private Vector4 controllerShaderData;

    // Data passed to shader, (x) overall alpha, (y) touchpad click duration,
    //  (z) app button click duration, (w) system button click duration.
    private Vector4 controllerShaderData2;
    private Color currentBatteryColor;

    /// <summary>App button animation duration when pressed.</summary>
    public const float APP_BUTTON_ACTIVE_DURATION_SECONDS = 0.111f;

    /// <summary>App button animation duration when released.</summary>
    public const float APP_BUTTON_RELEASE_DURATION_SECONDS = 0.0909f;

    /// <summary>System button animation duration when pressed.</summary>
    public const float SYSTEM_BUTTON_ACTIVE_DURATION_SECONDS = 0.111f;

    /// <summary>System button animation duration when released.</summary>
    public const float SYSTEM_BUTTON_RELEASE_DURATION_SECONDS = 0.0909f;

    /// <summary>Touchpad animation duration when pressed.</summary>
    public const float TOUCHPAD_CLICK_DURATION_SECONDS = 0.111f;

    /// <summary>Touchpad animation duration when released.</summary>
    public const float TOUCHPAD_RELEASE_DURATION_SECONDS = 0.0909f;

    /// @deprecated
    public const float TOUCHPAD_CLICK_SCALE_DURATION_SECONDS = 0.075f;

    /// <summary>Duration of the visual bubble on the controller
    /// to grow to its full size when clicked.</summary>
    public const float TOUCHPAD_POINT_SCALE_DURATION_SECONDS = 0.15f;

    // These values are used by the shader to control battery display
    // Only modify these values if you are also modifying the shader.
    private const float BATTERY_FULL = 0;
    private const float BATTERY_ALMOST_FULL = .125f;
    private const float BATTERY_MEDIUM = .225f;
    private const float BATTERY_LOW = .325f;
    private const float BATTERY_CRITICAL = .425f;
    private const float BATTERY_HIDDEN = .525f;

    private readonly Color GVR_BATTERY_CRITICAL_COLOR = new Color(1, 0, 0, 1);
    private readonly Color GVR_BATTERY_LOW_COLOR = new Color(1, 0.6823f, 0, 1);
    private readonly Color GVR_BATTERY_MED_COLOR = new Color(0, 1, 0.588f, 1);
    private readonly Color GVR_BATTERY_FULL_COLOR = new Color(0, 1, 0.588f, 1);

    // How much time to use as an 'immediate update'.
    // Any value large enough to instantly update all visual animations.
    private const float IMMEDIATE_UPDATE_TIME = 10f;

    void Awake()
    {
        Initialize();
        CreateAttachments();
    }

    void OnEnable()
    {
        GvrControllerInput.OnPostControllerInputUpdated += OnPostControllerInputUpdated;
    }

    void OnDisable()
    {
        GvrControllerInput.OnPostControllerInputUpdated -= OnPostControllerInputUpdated;
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            Initialize();
            OnVisualUpdate(true);
        }
    }

    private void OnPostControllerInputUpdated()
    {
        OnVisualUpdate();
    }

    private void CreateAttachments()
    {
        if (attachmentPrefabs == null)
        {
            return;
        }

        for (int i = 0; i < attachmentPrefabs.Length; i++)
        {
            GameObject prefab = attachmentPrefabs[i];
            GameObject attachment = Instantiate(prefab);
            attachment.transform.SetParent(transform, false);
        }
    }

    private void Initialize()
    {
        if (controllerRenderer == null)
        {
            controllerRenderer = GetComponent<Renderer>();
        }

        if (materialPropertyBlock == null)
        {
            materialPropertyBlock = new MaterialPropertyBlock();
        }

        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        alphaId = Shader.PropertyToID("_GvrControllerAlpha");
        touchId = Shader.PropertyToID("_GvrTouchInfo");
        touchPadId = Shader.PropertyToID("_GvrTouchPadColor");
        appButtonId = Shader.PropertyToID("_GvrAppButtonColor");
        systemButtonId = Shader.PropertyToID("_GvrSystemButtonColor");
        batteryColorId = Shader.PropertyToID("_GvrBatteryColor");

        materialPropertyBlock.SetColor(appButtonId, appButtonColor);
        materialPropertyBlock.SetColor(systemButtonId, systemButtonColor);
        materialPropertyBlock.SetColor(touchPadId, touchPadColor);
        controllerRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private void UpdateControllerState()
    {
        // Return early when the application isn't playing to ensure that the serialized displayState
        // is used to preview the controller visual instead of the default GvrControllerInputDevice
        // values.
#if UNITY_EDITOR
    if (!Application.isPlaying)
    {
      return;
    }
#endif

        if (ControllerInputDevice != null)
        {
            displayState.batteryLevel = ControllerInputDevice.BatteryLevel;
            displayState.batteryCharging = ControllerInputDevice.IsCharging;

            displayState.clickButton = ControllerInputDevice.GetButton(GvrControllerButton.TouchPadButton);
            displayState.appButton = ControllerInputDevice.GetButton(GvrControllerButton.App);
            displayState.homeButton = ControllerInputDevice.GetButton(GvrControllerButton.System);
            displayState.touching = ControllerInputDevice.GetButton(GvrControllerButton.TouchPadTouch);
            displayState.touchPos = ControllerInputDevice.TouchPos;
        }
    }

    /// Override this method to customize the visual's assets. This method is called
    /// once per frame in the visual update process. Return a VisualAssets struct with
    /// the assets to change.  Call this base method to get the current assets.
    protected virtual VisualAssets GetVisualAssets()
    {
        return new VisualAssets()
        {
            mesh = meshFilter.sharedMesh,
            material = controllerRenderer.sharedMaterial
        };
    }

    private void OnVisualUpdate(bool updateImmediately = false)
    {
        // Update the visual display based on the controller state
        if (readControllerState)
        {
            UpdateControllerState();
        }

        VisualAssets newAssets = GetVisualAssets();
        if (newAssets.mesh != meshFilter.sharedMesh)
        {
            meshFilter.sharedMesh = newAssets.mesh;
        }

        if (newAssets.material != controllerRenderer.sharedMaterial)
        {
            controllerRenderer.sharedMaterial = newAssets.material;
        }

        float deltaTime = Time.deltaTime;

        // If flagged to update immediately, set deltaTime to an arbitrarily large value.
        // This is particularly useful in editor, but also for resetting state quickly.
        if (updateImmediately)
        {
            deltaTime = IMMEDIATE_UPDATE_TIME;
        }

        if (displayState.clickButton)
        {
            controllerShaderData2.y = Mathf.Min(1, controllerShaderData2.y + deltaTime / TOUCHPAD_CLICK_DURATION_SECONDS);
        }
        else
        {
            controllerShaderData2.y = Mathf.Max(0, controllerShaderData2.y - deltaTime / TOUCHPAD_RELEASE_DURATION_SECONDS);
        }

        if (displayState.appButton)
        {
            controllerShaderData2.z = Mathf.Min(1, controllerShaderData2.z + deltaTime / APP_BUTTON_ACTIVE_DURATION_SECONDS);
        }
        else
        {
            controllerShaderData2.z = Mathf.Max(0, controllerShaderData2.z - deltaTime / APP_BUTTON_RELEASE_DURATION_SECONDS);
        }

        if (displayState.homeButton)
        {
            controllerShaderData2.w = Mathf.Min(1, controllerShaderData2.w + deltaTime / SYSTEM_BUTTON_ACTIVE_DURATION_SECONDS);
        }
        else
        {
            controllerShaderData2.w = Mathf.Max(0, controllerShaderData2.w - deltaTime / SYSTEM_BUTTON_RELEASE_DURATION_SECONDS);
        }

        // Set the material's alpha to the multiplied preferred alpha.
        controllerShaderData2.x = PreferredAlpha;
        materialPropertyBlock.SetVector(alphaId, controllerShaderData2);

        controllerShaderData.x = displayState.touchPos.x;
        controllerShaderData.y = displayState.touchPos.y;

        if (displayState.touching || displayState.clickButton)
        {
            if (!wasTouching)
            {
                wasTouching = true;
            }

            if (touchTime < 1)
            {
                touchTime = Mathf.Min(touchTime + deltaTime / TOUCHPAD_POINT_SCALE_DURATION_SECONDS, 1);
            }
        }
        else
        {
            wasTouching = false;
            if (touchTime > 0)
            {
                touchTime = Mathf.Max(touchTime - deltaTime / TOUCHPAD_POINT_SCALE_DURATION_SECONDS, 0);
            }
        }

        controllerShaderData.z = touchTime;

        UpdateBatteryIndicator();

        materialPropertyBlock.SetVector(touchId, controllerShaderData);
        materialPropertyBlock.SetColor(batteryColorId, currentBatteryColor);

        // Update the renderer
        controllerRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private void UpdateBatteryIndicator()
    {
        GvrControllerBatteryLevel level = displayState.batteryLevel;
        bool charging = displayState.batteryCharging;

        switch (level)
        {
            case GvrControllerBatteryLevel.Full:
                controllerShaderData.w = BATTERY_FULL;
                currentBatteryColor = GVR_BATTERY_FULL_COLOR;
                break;
            case GvrControllerBatteryLevel.AlmostFull:
                controllerShaderData.w = BATTERY_ALMOST_FULL;
                currentBatteryColor = GVR_BATTERY_FULL_COLOR;
                break;
            case GvrControllerBatteryLevel.Medium:
                controllerShaderData.w = BATTERY_MEDIUM;
                currentBatteryColor = GVR_BATTERY_MED_COLOR;
                break;
            case GvrControllerBatteryLevel.Low:
                controllerShaderData.w = BATTERY_LOW;
                currentBatteryColor = GVR_BATTERY_LOW_COLOR;
                break;
            case GvrControllerBatteryLevel.CriticalLow:
                controllerShaderData.w = BATTERY_CRITICAL;
                currentBatteryColor = GVR_BATTERY_CRITICAL_COLOR;
                break;
            default:
                controllerShaderData.w = BATTERY_HIDDEN;
                currentBatteryColor.a = 0;
                break;
        }

        if (charging)
        {
            controllerShaderData.w = -controllerShaderData.w;
            currentBatteryColor = GVR_BATTERY_FULL_COLOR;
        }
    }

    /// <summary>Sets the controller texture.</summary>
    [SuppressMemoryAllocationError(IsWarning = true, Reason = "Pending documentation.")]
    public void SetControllerTexture(Texture newTexture)
    {
        controllerRenderer.material.mainTexture = newTexture;
    }
}
