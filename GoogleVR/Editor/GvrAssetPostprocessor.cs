using UnityEngine;
using UnityEditor;
using System.IO;

class GvrAssetPostprocessor : AssetPostprocessor {
  void OnPreprocessTexture() {
    // Reconfigure all images in Plugins/iOS as so they don't get compressed,
    // resized, or mipmapped.  Saves a bunch of import time.
    if (assetPath.Contains("Plugins/iOS")) {
      TextureImporter ti = assetImporter as TextureImporter;
      // Don't compress at all.
      ti.textureFormat = TextureImporterFormat.AutomaticTruecolor;
      // Don't rescale at all.
      ti.npotScale = TextureImporterNPOTScale.None;
      // Don't generate mipmaps.
      ti.mipmapEnabled = false;
    }
  }
}
