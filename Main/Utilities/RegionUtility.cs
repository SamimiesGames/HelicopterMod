using Il2Cpp;
using UnityEngine;

namespace HelicopterMod.Main.Utilities
{
    public struct RegionUtility
    {
        public static void LoadScene(string sceneName)
        {
            // Loading scenes work by creating a transition collider on the player.
            GameObject loadSceneGo = new GameObject("LoadScene");
            loadSceneGo.layer = LayerMask.NameToLayer("Trigger");

            // The collider needs to be added first, otherwise LoadScene cannot find it.
            BoxCollider collider = loadSceneGo.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = Vector3.one * 3f;
            
            // Setup LoadScene.
            LoadScene loadScene = loadSceneGo.AddComponent<LoadScene>();
            loadScene.m_SceneToLoad = sceneName; // Scene to travel to.
            loadScene.m_ExitPointName = "DefaultSpawnPoint"; // Fallback spawn point that is in all regions.
            loadScene.m_Active = true;
            loadScene.m_TransitionOnContact = true;
            loadScene.CanInteract = true;
            
            // Stick the collider to the player so that it will always trigger.
            loadScene.transform.SetParent(ManagerCache.PlayerTransform);
            loadScene.transform.localPosition = Vector3.zero;
            loadScene.transform.localRotation = Quaternion.identity;
        }

        public static void AttachLoadScene(GameObject to, string sceneName)
        {
            // The collider needs to be added first, otherwise LoadScene cannot find it.
            BoxCollider collider = to.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            
            // Setup LoadScene.
            LoadScene loadScene = to.AddComponent<LoadScene>();
            loadScene.m_SceneToLoad = sceneName; // Scene to travel to.
            loadScene.m_ExitPointName = "DefaultSpawnPoint"; // Fallback spawn point that is in all regions.
            loadScene.m_Active = true;
            loadScene.m_TransitionOnContact = true;
            loadScene.CanInteract = true;
        }
    }    
}
