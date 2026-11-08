using HarmonyLib;
using Il2Cpp;

namespace HelicopterMod.Main.Patches
{
    [HarmonyPatch(typeof(PlayerManager), "TeleportPlayerAfterSceneLoad")]
    static class RegionArrival
    {
        static void Postfix(PlayerManager __instance)
        {
            
        } 
    }    
}
