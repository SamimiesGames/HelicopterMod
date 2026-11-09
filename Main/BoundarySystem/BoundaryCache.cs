using System.Diagnostics.CodeAnalysis;
using HelicopterMod.Main.Utilities;
using Il2Cpp;
using UnityEngine;

namespace HelicopterMod.Main.BoundarySystem
{
    public struct BoundaryCache
    {
        [AllowNull]
        public static GameObject ActiveBoundariesGameObject => _boundariesGameObject;
        [AllowNull]
        public static Boundary ActiveBoundary => _boundary;

        [AllowNull]
        private static GameObject _boundariesGameObject;
        [AllowNull]
        private static Boundary _boundary;
        
        public static void SetActiveBoundaries(string sceneName, bool setLayerActive)
        {
            Logger.Msg($"[BoundaryCache] SetActiveBoundaries for \"{sceneName}\"");
            
            GameObject prefab = AssetBundleUtility.GetPrefab("helicopters", $"Boundaries_{sceneName}");
            if (!prefab)
            {
                _boundary = null;
                _boundariesGameObject = null;
                return;
            }
            
            _boundariesGameObject = GameObject.Instantiate(prefab);
            _boundariesGameObject.SetActive(setLayerActive);
            
            _boundary = BoundaryUtility.Unpack(_boundariesGameObject);
            
            BoundaryUtility.Load(_boundary);
        }
    }    
}
