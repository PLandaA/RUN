using UnityEngine;
using UnityEditor;
using System.Linq;

public static class WebGLTextureShrinker
{
    const int MaxSize = 1024;

    [MenuItem("Build Tools/Shrink Textures For WebGL")]
    public static void Shrink()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");
        int changed = 0, scanned = 0;

        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                if (!path.StartsWith("Assets/")) continue;
                scanned++;

                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null) continue;

                var web = imp.GetPlatformTextureSettings("WebGL");
                web.overridden = true;
                web.maxTextureSize = MaxSize;
                web.format = TextureImporterFormat.Automatic;
                web.textureCompression = TextureImporterCompression.Compressed;
                imp.SetPlatformTextureSettings(web);
                imp.SaveAndReimport();
                changed++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        Debug.LogError("[Shrink] DONE. scanned=" + scanned + " overridden=" + changed + " maxSize=" + MaxSize);
    }
}
