using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class AssetBundler
    {
        [MenuItem("HelicopterMod/Build AssetBundle")]
        public static void Build()
        {
            string directory = "Assets/AssetBundles";
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            Debug.Log($"[AssetBundler] Building Assets");
            BuildPipeline.BuildAssetBundles(
                directory, 
                BuildAssetBundleOptions.ChunkBasedCompression, 
                BuildTarget.StandaloneWindows64
            );
            Debug.Log($"[AssetBundler] Built assets");
        }
    }   
}
