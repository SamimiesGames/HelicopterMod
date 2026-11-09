using System.Diagnostics.CodeAnalysis;
using HelicopterMod.Main.Utilities;
using Il2Cpp;
using UnityEngine;

namespace HelicopterMod.Main.BoundarySystem
{
    public struct BoundaryCache
    {
        [AllowNull]
        public static Boundary ActiveBoundary => _boundary;
        
        [AllowNull] 
        public static GameObject Boundaries => _boundariesGameObject;
        
        [AllowNull]
        private static GameObject _boundariesGameObject;
        [AllowNull]
        private static Boundary _boundary;
        
        public static void SetActiveBoundaries(string sceneName)
        {
            Logger.Msg($"[BoundaryCache] SetActiveBoundaries for \"{sceneName}\"");
            
            GameObject prefab = AssetBundleUtility.GetPrefab("helicopters", $"Boundaries_{sceneName}");
            if (!prefab)
            {
                _boundary = null;
                _boundariesGameObject = null;
                return;
            }
            
            _boundariesGameObject = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            _boundariesGameObject.SetActive(false);
            
            _boundary = BoundaryUtility.Unpack(_boundariesGameObject);
            
            BoundaryUtility.Load(_boundary);
        }

        public static void SetLayerActive(bool enabled)
        {
            Logger.Msg($"[BoundaryCache] SetLayerActive, enabled={enabled}");
            if (enabled && !_boundariesGameObject)
            {
                Logger.Warning($"[BoundaryCache] SetLayerActive will fail; enabled={enabled}, but there are no boundaries.");
            }
            _boundariesGameObject?.SetActive(enabled);
        }
    }    
}
