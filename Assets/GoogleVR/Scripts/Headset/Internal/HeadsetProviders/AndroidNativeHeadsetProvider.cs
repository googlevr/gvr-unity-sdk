// Copyright 2017 Google Inc. All rights reserved.
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
// See the License for the specific language governing permissioßns and
// limitations under the License.

// This provider is only available on an Android device.
#if UNITY_ANDROID && !UNITY_EDITOR
using Gvr;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using XRDevice = UnityEngine.VR.VRDevice;
#endif  // UNITY_2017_2_OR_NEWER

/// @cond
namespace Gvr.Internal {
  class AndroidNativeHeadsetProvider : IHeadsetProvider {
    private IntPtr gvrContextPtr = XRDevice.GetNativePtr();
    private GvrValue gvrValue = new GvrValue();
    private gvr_event_header gvrEventHeader = new gvr_event_header();
    private gvr_recenter_event_data gvrRecenterEventData = new gvr_recenter_event_data();
    // |gvr_event| C struct is spec'd to be up to 512 bytes in size.
    private byte[] gvrEventBuffer = new byte[512];
    private GCHandle gvrEventHandle;
    private IntPtr gvrEventPtr;
    private IntPtr gvrPropertiesPtr;
    private int supportsPositionalTracking = -1;

    /// Used only as a temporary placeholder to avoid allocations.
    /// Do not use this for storing state.
    private MutablePose3D transientRecenteredPose3d = new MutablePose3D();

    private readonly Matrix4x4 MATRIX4X4_IDENTITY = Matrix4x4.identity;

    public bool SupportsPositionalTracking {
      get {
        if (supportsPositionalTracking < 0) {
          supportsPositionalTracking =
            gvr_is_feature_supported(gvrContextPtr, (int)gvr_feature.HeadPose6dof) ? 1 : 0;
        }
        return supportsPositionalTracking > 0;
      }
    }

    internal AndroidNativeHeadsetProvider() {
      gvrEventHandle = GCHandle.Alloc(gvrEventBuffer, GCHandleType.Pinned);
      gvrEventPtr = gvrEventHandle.AddrOfPinnedObject();
    }

    ~AndroidNativeHeadsetProvider() {
      gvrEventHandle.Free();
    }

    public void PollEventState(ref HeadsetState state) {
      GvrErrorType eventType;
      try {
       eventType = (GvrErrorType)gvr_poll_event(gvrContextPtr, gvrEventPtr);
      } catch (EntryPointNotFoundException) {
          Debug.LogError("GvrHeadset not supported by this version of Unity. " +
                         "Support starts in 5.6.3p3 and 2017.1.1p1.");
          throw;
      }
      if (eventType == GvrErrorType.NoEventAvailable) {
        state.eventType = GvrEventType.Invalid;
        return;
      }

      Marshal.PtrToStructure(gvrEventPtr, gvrEventHeader);
      state.eventFlags = gvrEventHeader.flags;
      state.eventTimestampNs = gvrEventHeader.timestamp;
      state.eventType = (GvrEventType) gvrEventHeader.type;
      // Event data begins after header.
      IntPtr eventDataPtr = new IntPtr(gvrEventPtr.ToInt64() + Marshal.SizeOf(gvrEventHeader));

      if (state.eventType == GvrEventType.Recenter) {
        Marshal.PtrToStructure(eventDataPtr, gvrRecenterEventData);
        _HandleRecenterEvent(ref state, gvrRecenterEventData);
        return;
      }
    }

    public bool TryGetFloorHeight(ref float floorHeight) {
      if (!_GvrGetProperty(gvr_property_type.TrackingFloorHeight, gvrValue)) {
        return false;
      }
      floorHeight = gvrValue.ToFloat();
      return true;
    }

    public bool TryGetRecenterTransform(ref Vector3 position, ref Quaternion rotation) {
      if (!_GvrGetProperty(gvr_property_type.RecenterTransform, gvrValue)) {
        return false;
      }
      transientRecenteredPose3d.Set(gvrValue.ToMatrix4x4());
      position = transientRecenteredPose3d.Position;
      rotation = transientRecenteredPose3d.Orientation;
      return true;
    }

    public bool TryGetSafetyRegionType(ref GvrSafetyRegionType safetyType) {
      if (!_GvrGetProperty(gvr_property_type.SafetyRegion, gvrValue)) {
        return false;
      }
      safetyType = (GvrSafetyRegionType) gvrValue.ToInt32();
      return true;
    }

    public bool TryGetSafetyCylinderInnerRadius(ref float innerRadius) {
      if (!_GvrGetProperty(gvr_property_type.SafetyCylinderInnerRadius, gvrValue)) {
        return false;
      }
      innerRadius = gvrValue.ToFloat();
      return true;
    }

    public bool TryGetSafetyCylinderOuterRadius(ref float outerRadius) {
      if (!_GvrGetProperty(gvr_property_type.SafetyCylinderOuterRadius, gvrValue)) {
        return false;
      }
      outerRadius = gvrValue.ToFloat();
      return true;
    }

#region PRIVATE_HELPERS
    /// Returns true if a property was available and retrieved.
    private bool _GvrGetProperty(gvr_property_type propertyType, GvrValue valueOut) {
      gvr_value_type valueType = GetPropertyValueType(propertyType);
      if (valueType == gvr_value_type.None) {
        Debug.LogError("Unknown gvr property " + propertyType + ".  Unable to type check.");
      }

      if (gvrPropertiesPtr == IntPtr.Zero) {
        try {
          gvrPropertiesPtr = gvr_get_current_properties(gvrContextPtr);
        } catch (EntryPointNotFoundException) {
            Debug.LogError("GvrHeadset not supported by this version of Unity. " +
                           "Support starts in 5.6.3p3 and 2017.1.1p1.");
            throw;
        }
      }
      if (gvrPropertiesPtr == IntPtr.Zero) {
        return false;
      }

      // Assumes that gvr_properties_get (C API) will only ever return
      // GvrErrorType.None or GvrErrorType.NoEventAvailable.
      bool success =
        (GvrErrorType.None ==
          (GvrErrorType) gvr_properties_get(gvrPropertiesPtr, propertyType, valueOut.BufferPtr));
      if (success) {
        valueOut.Parse();
        success = (valueType == gvr_value_type.None || valueOut.TypeEnum == valueType);
        if (!success) {
          Debug.LogError("GvrGetProperty " + propertyType + " type mismatch, expected "
                         + valueType + " got " + valueOut.TypeEnum);
        }
      }

      return success;
    }

    private void _HandleRecenterEvent(ref HeadsetState state, gvr_recenter_event_data eventData) {
      state.recenterEventType = (GvrRecenterEventType) eventData.recenter_event_type;
      state.recenterEventFlags = eventData.recenter_event_flags;

      Matrix4x4 poseTransform = MATRIX4X4_IDENTITY;
      float[] poseRaw = eventData.pose_transform;
      for (int i = 0; i < 4; i++) {
        int j = i * 4;
        Vector4 row = new Vector4(poseRaw[j], poseRaw[j + 1], poseRaw[j + 2], poseRaw[j + 3]);
        poseTransform.SetRow(i, row);
      }

      // Invert the matrix to go from row-major (GVR) to column-major (Unity),
      // and change from LHS to RHS coordinates.
      transientRecenteredPose3d.SetRightHanded(poseTransform.transpose);
      state.recenteredPosition = transientRecenteredPose3d.Position;
      state.recenteredRotation = transientRecenteredPose3d.Orientation;
    }
#endregion  // PRIVATE_HELPERS

#region GVR_TYPE_HELPERS
    private gvr_value_type GetPropertyValueType(gvr_property_type propertyType) {
      gvr_value_type propType = gvr_value_type.None;
      switch(propertyType) {
        case gvr_property_type.TrackingFloorHeight:
          propType = gvr_value_type.Float;
          break;
        case gvr_property_type.RecenterTransform:
          propType = gvr_value_type.Mat4f;
          break;
        case gvr_property_type.SafetyRegion:
          propType = gvr_value_type.Int;
          break;
        case gvr_property_type.SafetyCylinderInnerRadius:
          propType = gvr_value_type.Float;
          break;
        case gvr_property_type.SafetyCylinderOuterRadius:
          propType = gvr_value_type.Float;
          break;
      }
      return propType;
    }

    /// Helper class to parse |gvr_value| structs into the varied data types it could contain.
    /// NOTE: Does NO type checking on value conversions.  |_GvrGetProperty| checks types.
    private class GvrValue {
      private static readonly int HEADER_SIZE = Marshal.SizeOf(typeof(gvr_value_header));
      private gvr_value_header valueHeader = new gvr_value_header();
      // |gvr_value| C struct is spec'd to be up to 256 bytes in size.
      private byte[] buffer = new byte[256];
      private IntPtr bufferPtr;
      private IntPtr valuePtr;
      private GCHandle bufferHandle;

      public GvrValue() {
        bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        bufferPtr = bufferHandle.AddrOfPinnedObject();
        // Value portion starts after the header.
        valuePtr = new IntPtr(bufferPtr.ToInt64() + HEADER_SIZE);
      }

      ~GvrValue() {
        bufferHandle.Free();
      }

      /// Gets the ptr to a buffer that can be used as an argument to |gvr_properties_get|.
      public IntPtr BufferPtr {
        get {
          return bufferPtr;
        }
      }

      public gvr_value_type TypeEnum {
        get {
          return valueHeader.value_type;
        }
      }

      /// Parse the header of the current buffer.  This should be called after the contents of
      /// the buffer have been altered e.g. by a call to |gvr_properties_get|.
      public void Parse() {
        Marshal.PtrToStructure(bufferPtr, valueHeader);
      }

      public int ToInt32() {
        return BitConverter.ToInt32(buffer, HEADER_SIZE);
      }

      public long ToInt64() {
        return BitConverter.ToInt64(buffer, HEADER_SIZE);
      }

      public float ToFloat() {
        return BitConverter.ToSingle(buffer, HEADER_SIZE);
      }

      public double ToDouble() {
        return BitConverter.ToDouble(buffer, HEADER_SIZE);
      }

      public Vector2 ToVector2() {
        return (Vector2) Marshal.PtrToStructure(valuePtr, typeof(Vector2));
      }

      public Vector3 ToVector3() {
        return (Vector3) Marshal.PtrToStructure(valuePtr, typeof(Vector3));
      }

      public Vector4 ToVector4() {
        return (Vector4) Marshal.PtrToStructure(valuePtr, typeof(Vector4));
      }

      public Quaternion ToQuaternion() {
        return (Quaternion) Marshal.PtrToStructure(valuePtr, typeof(Quaternion));
      }

      public gvr_rectf ToGvrRectf() {
        return (gvr_rectf) Marshal.PtrToStructure(valuePtr, typeof(gvr_rectf));
      }

      public gvr_recti ToGvrRecti() {
        return (gvr_recti) Marshal.PtrToStructure(valuePtr, typeof(gvr_recti));
      }

      public Matrix4x4 ToMatrix4x4() {
        Matrix4x4 mat4 = (Matrix4x4) Marshal.PtrToStructure(valuePtr, typeof(Matrix4x4));
        // Transpose the matrix from row-major (GVR) to column-major (Unity),
        // and change from LHS to RHS coordinates.
        return Pose3D.FlipHandedness(mat4.transpose);
      }
    }
#endregion  // GVR_TYPE_HELPERS

#region GVR_NATIVE_STRUCTS
    [StructLayout(LayoutKind.Sequential)]
    private class gvr_recenter_event_data {
      internal int recenter_event_type;  // gvr_recenter_event_type
      internal uint recenter_event_flags;  // gvr_flags

      [MarshalAs(UnmanagedType.ByValArray, SizeConst=16)]
      internal float[] pose_transform = new float[16];  // gvr_mat4f = float[4][4]
    }

    [StructLayout(LayoutKind.Explicit)]
    private class gvr_event_header {
      [FieldOffset(0)]
      internal long timestamp;

      [FieldOffset(8)]
      internal int type;  // gvr_event_type

      [FieldOffset(12)]
      internal int flags;  // gvr_flags

      // Event specific data starts at offset 16.
    }

    [StructLayout(LayoutKind.Explicit)]
    private class gvr_value_header {
      [FieldOffset(0)]
      internal gvr_value_type value_type;

      [FieldOffset(4)]
      internal int flags;  // gvr_flags

      // Value data starts at offset 8.
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct gvr_sizei {
      internal int width;
      internal int height;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct gvr_recti {
      internal int left;
      internal int right;
      internal int bottom;
      internal int top;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct gvr_rectf {
      internal float left;
      internal float right;
      internal float bottom;
      internal float top;
    }


#endregion  // GVR_NATIVE_STRUCTS

#region GVR_C_API
    private const string DLL_NAME = GvrActivityHelper.GVR_DLL_NAME;

    [DllImport(DLL_NAME)]
    private static extern bool gvr_is_feature_supported(IntPtr gvr_context, int feature);

    [DllImport(DLL_NAME)]
    private static extern int gvr_poll_event(IntPtr gvr_context, IntPtr event_out);

    [DllImport(DLL_NAME)]
    private static extern IntPtr gvr_get_current_properties(IntPtr gvr_context);

    [DllImport(DLL_NAME)]
    private static extern int gvr_properties_get(
        IntPtr gvr_properties, gvr_property_type property_type, IntPtr value_out);
#endregion  // GVR_C_API
  }
}
/// @endcond
#endif  // UNITY_ANDROID && !UNITY_EDITOR
