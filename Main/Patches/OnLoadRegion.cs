using HarmonyLib;
using HelicopterMod.Main.BoundarySystem;
using Il2Cpp;
using UnityEngine;

namespace HelicopterMod.Main.Patches
{
    [HarmonyPatch(typeof(PlayerManager), "TeleportPlayerAfterSceneLoad")]
    static class OnLoadRegion
    {
        static void Postfix(PlayerManager __instance)
        {
            bool inHelicopter = HelicopterModState.TransitionState == HelicopterModTransitionState.InHelicopter;
            BoundaryCache.SetActiveBoundaries(GameManager.m_ActiveScene, inHelicopter);

            bool hasBoundary = BoundaryCache.ActiveBoundary != null;
            bool hasLastActiveScene = !string.IsNullOrEmpty(HelicopterModState.LastActiveScene);
            Logger.Msg($"[HarmonyPatch::PlayerManager.TeleportPlayerAfterSceneLoad] inHelicopter={inHelicopter}, hasBoundary={hasBoundary}, hasLastActiveScene={hasLastActiveScene}");
            if (inHelicopter && hasBoundary && hasLastActiveScene)
            {
                if (!BoundaryCache.ActiveBoundary.ExitPoints.ContainsKey(HelicopterModState.LastActiveScene))
                {
                    Logger.Warning($"[HarmonyPatch::PlayerManager.TeleportPlayerAfterSceneLoad] No exit points!");
                    return;
                }

                if (!HelicopterMod.Instance.currentHelicopter)
                {
                    Logger.Msg($"[HarmonyPatch::PlayerManager.TeleportPlayerAfterSceneLoad] Stale helicopter reference! Loading a new one.");
                    HelicopterMod.Instance.currentHelicopter = HelicopterUtility.LoadHelicopter(HelicopterUtility.LastPrefabName);
                    HelicopterUtility.EnterHelicopter();
                }
                
                Transform exitPoint = BoundaryCache.ActiveBoundary.ExitPoints[HelicopterModState.LastActiveScene];
                HelicopterMod.Instance.currentHelicopter.transform.position = exitPoint.position;
                HelicopterMod.Instance.currentHelicopter.transform.rotation = exitPoint.rotation;
                Logger.Msg($"[HarmonyPatch::PlayerManager.TeleportPlayerAfterSceneLoad] Teleported helicopter to {exitPoint.name}");
            }

            HelicopterModState.LastActiveScene = GameManager.m_ActiveScene;
        } 
    }    
}
