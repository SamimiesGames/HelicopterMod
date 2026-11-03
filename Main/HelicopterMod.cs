using HelicopterMod.Main.Controllers;
using MelonLoader;
using Il2Cpp;
using Il2CppTLD.Interactions;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Console = Il2CppSystem.Console;

namespace HelicopterMod.Main
{
    public class HelicopterMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            
            Logger.Msg("HelicopterMod loading!"); 
            AssetBundleUtility.LoadBundle("helicopters", "Mods/helicoptermodassets");
            
            uConsole.RegisterCommand("spawn_helicopter", new Action(SpawnHelicopter));
        }

        public void SpawnHelicopter()
        {
            Logger.Msg("Load helicopter");
            
            GameObject prefab = AssetBundleUtility.GetPrefab(
                "helicopters", 
                "VEHICLE_McLeodsUltimatePersonalTransportationMachineA"
            );
            Transform playerTransform = GameManager.GetPlayerTransform();
            
            GameObject helicopter = GameObject.Instantiate(prefab, playerTransform.position + Vector3.up * 2, playerTransform.rotation);
            
            Logger.Msg("Seat player in helicopter?"); 
            GameObject seat = helicopter.transform.Find("PlayerEnter").gameObject;
            
            playerTransform.SetParent(seat.transform);
            playerTransform.localPosition = Vector3.zero;
            playerTransform.localRotation = Quaternion.identity;
            
            //seat.AddComponent<Seat>();
            //seat.AddComponent<PilotFreeLook>();
            helicopter.AddComponent<HelicopterControls>();
            Logger.Msg("Seated player in helicopter?"); 
        }
        
    }   
}