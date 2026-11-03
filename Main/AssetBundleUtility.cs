using MelonLoader.Utils;
using UnityEngine;

namespace HelicopterMod.Main;

public class AssetBundleUtility
{
    public static Dictionary<string, AssetBundle> LoadedBundles = new();

    public static void LoadBundle(string refenceName, string filePath)
    {
        if (LoadedBundles.ContainsKey(refenceName))
        {
            Logger.Warning($"[AssetBundleUtility] AssetBundle={refenceName} already loaded!");
            return;
        }
        if (!File.Exists(filePath))
        {
            Logger.Error($"[AssetBundleUtility] Failed to load bundle, filePath={filePath} does not exists!");
            return;
        }
        
        string bundlePath = Path.Combine(MelonEnvironment.GameRootDirectory, filePath);
        
        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
        Logger.Msg($"[AssetBundleUtility] Loaded bundle={refenceName}");
        LoadedBundles.Add(refenceName, bundle);
    }

    public static GameObject GetPrefab(string refenceName, string prefabName)
    {
        if (!LoadedBundles.ContainsKey(refenceName))
        {
            Logger.Error($"[AssetBundleUtility] GetPrefab, failed to load prefab, AssetBundle={refenceName} is not loaded!");
            return null;
        }

        Logger.Msg($"[AssetBundleUtility] GetPrefab prefabName={prefabName}");
        return LoadedBundles[refenceName]
            .LoadAsset<GameObject>(prefabName);
    }
}