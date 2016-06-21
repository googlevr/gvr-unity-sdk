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

namespace GVR.Gfx {
  [RequireComponent(typeof(Camera))]
  public class ScreenSpaceQuad : MonoBehaviour {
    [SerializeField]
    ImageFXBase imageFX;

    [Tooltip("The Material which the quad will be rendered with. (Pass 0 only)")]
    public Material material;

    [SerializeField]
    [Tooltip("Should the quad take the bounds of the target object? If False, the quad will fill the screen.")]
    bool sizeToBounds = true;

    [System.NonSerialized]
    public Bounds targetBounds = new Bounds();

    private Vector3 center, extents, lbf, rbf, ltf, rtf, lbb, rbb, ltb, rtb;
    private float left = 0f;
    private float right = 1f;
    private float top = 1f;
    private float bottom = 0f;
    private float depth = 0.1f;
    private Camera cam;

    void Awake() {
      Assert.NotNull<ImageFXBase>(this, imageFX, "imageFX");
      Assert.AttachedCamera(this);
      cam = GetComponent<Camera>();
    }

#if UNITY_EDITOR
    // This is just here so we get the checkbox in the Inspector to debug our Render messages
    void Update() { }
#endif


    void OnPreRender() {
      if (imageFX != null) {
        imageFX.Render(cam);
      }
    }

    void OnPostRender() {
      RenderQuad();
    }

    void RenderQuad() {
      if (sizeToBounds) {
        Profiler.BeginSample("ScreenSpaceQuad.Render.Calculate Screen Rect");
          // Refresh the world-space bounds verts of our target object
          UpdateBoundsVertices();
          Profiler.BeginSample("ScreenSpaceQuad.Render.Space Conversion");
            // convert the bounds information into viewport space
            lbf = cam.WorldToViewportPoint(lbf);
            rbf = cam.WorldToViewportPoint(rbf);
            ltf = cam.WorldToViewportPoint(ltf);
            rtf = cam.WorldToViewportPoint(rtf);

            lbb = cam.WorldToViewportPoint(lbb);
            rbb = cam.WorldToViewportPoint(rbb);
            ltb = cam.WorldToViewportPoint(ltb);
            rtb = cam.WorldToViewportPoint(rtb);
          Profiler.EndSample();
          Profiler.BeginSample("ScreenSpaceQuad.Render.Calculate Rect Extents");
            // resolve the maximum extents of the viewport Rect
            // The params overload allocates bytes to the heap, so we do this long hand.
            left = Mathf.Min(lbf.x, Mathf.Min(ltf.x, Mathf.Min(lbb.x, ltb.x))); 
            right = Mathf.Max(rbf.x, Mathf.Max(rtf.x, Mathf.Max(rbb.x, rtb.x)));
            top = Mathf.Max(ltf.y, Mathf.Max(rtf.y, Mathf.Max(ltb.y, rtb.y)));
            bottom = Mathf.Min(lbf.y, Mathf.Min(rbf.y, Mathf.Min(lbb.y, rbb.y)));
          Profiler.EndSample();
        Profiler.EndSample();
      }

      Profiler.BeginSample("ScreenSpaceQuad.Render.Draw Quad");
        GL.PushMatrix();
        GL.LoadOrtho();
        material.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.TexCoord2(0f, 0f);
        GL.Vertex3(left, bottom, depth); // BL
        GL.TexCoord2(0f, 1f);
        GL.Vertex3(left, top, depth); // TL
        GL.TexCoord2(1f, 1f);
        GL.Vertex3(right, top, depth); // TR
        GL.TexCoord2(1f, 0f);
        GL.Vertex3(right, bottom, depth); // BR
        GL.End();
        GL.PopMatrix();
      Profiler.EndSample();
    }

    void UpdateBoundsVertices() {
      Profiler.BeginSample("ScreenSpaceQuad.Render.Update Bounds Vertices");
        // world space bounds information
        center = targetBounds.center;
        extents = targetBounds.extents;
        // All our AABB verts, named as xyz ('lbf' == Left-Bottom-Front)
        lbf = center - extents;
        rbf = center + new Vector3(extents.x, -extents.y, -extents.z);
        ltf = center + new Vector3(-extents.x, extents.y, -extents.z);
        rtf = center + new Vector3(extents.x, extents.y, -extents.z);
        lbb = center + new Vector3(-extents.x, -extents.y, extents.z);
        rbb = center + new Vector3(extents.x, -extents.y, extents.z);
        ltb = center + new Vector3(-extents.x, extents.y, extents.z);
        rtb = center + extents;
      Profiler.EndSample();
    }

    void OnDrawGizmos() {
      UpdateBoundsVertices();
      Gizmos.color = Color.red;
      // Front
      Gizmos.DrawLine(lbf, rbf);
      Gizmos.DrawLine(rbf, rtf);
      Gizmos.DrawLine(rtf, ltf);
      Gizmos.DrawLine(ltf, lbf);
      // Back
      Gizmos.DrawLine(lbb, rbb);
      Gizmos.DrawLine(rbb, rtb);
      Gizmos.DrawLine(rtb, ltb);
      Gizmos.DrawLine(ltb, lbb);
      // Left
      Gizmos.DrawLine(lbf, lbb);
      Gizmos.DrawLine(ltf, ltb);
      // Right
      Gizmos.DrawLine(rbf, rbb);
      Gizmos.DrawLine(rtf, rtb);
    }
  }
}
