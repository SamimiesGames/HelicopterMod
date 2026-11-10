using HarmonyLib;
using HelicopterMod.Main.BoundarySystem;
using HelicopterMod.Main.Utilities;
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
            
            BoundaryCache.SetActiveBoundaries(GameManager.m_ActiveScene);

            bool mainMenu = GameManager.m_ActiveScene == "MainMenu";
            bool hasBoundary = BoundaryCache.ActiveBoundary != null;
            bool hasLastActiveScene = !string.IsNullOrEmpty(HelicopterModState.LastActiveScene);
            
            if (inHelicopter && hasBoundary && hasLastActiveScene)
            {
                if (!BoundaryCache.ActiveBoundary.ExitPoints.ContainsKey(HelicopterModState.LastActiveScene))
                {
                    Logger.Warning($"[TeleportPlayerAfterSceneLoad] No exit points!");
                    return;
                }
                
                if (!HelicopterMod.Instance.currentHelicopter)
                {
                    if (ManagerCache.PlayerManager)
                    {
                        Logger.Msg($"[TeleportPlayerAfterSceneLoad] PlayerManager.ControlMode={ManagerCache.PlayerManager.GetControlMode()}");
                        Logger.Msg($"[TeleportPlayerAfterSceneLoad] __instance.ControlMode={__instance.GetControlMode()}");
                    }
                    else
                    {
                        Logger.Error($"[TeleportPlayerAfterSceneLoad] No PlayerManager!");
                    }
                    
                    Logger.Msg($"[TeleportPlayerAfterSceneLoad] Stale helicopter reference! Loading a new one.");
                    HelicopterMod.Instance.currentHelicopter = HelicopterUtility.LoadHelicopter(HelicopterUtility.LastPrefabName);
                    
                    HelicopterUtility.EnterHelicopter();
                    HelicopterUtility.RestoreState();
                }
                
                Transform exitPoint = BoundaryCache.ActiveBoundary.ExitPoints[HelicopterModState.LastActiveScene];
                
                HelicopterMod.Instance.currentHelicopter.transform.position = exitPoint.position;
                HelicopterMod.Instance.currentHelicopter.transform.rotation = exitPoint.rotation;
                
                Logger.Msg($"[TeleportPlayerAfterSceneLoad] Teleported helicopter to {exitPoint.name}");
            }

            HelicopterModState.LastActiveScene = GameManager.m_ActiveScene;

            if (mainMenu)
            {
                Logger.Msg($"[TeleportPlayerAfterSceneLoad] In main menu.");
                HelicopterModState.LastActiveScene = string.Empty;
                HelicopterModState.TransitionState = HelicopterModTransitionState.OnFoot;
            }
        } 
    }    
}
