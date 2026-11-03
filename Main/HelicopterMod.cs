using MelonLoader;
using Il2Cpp;
using Il2CppTLD.Interactions;
using UnityEngine;
using Console = Il2CppSystem.Console;

namespace HelicopterMod.Main
{
    public class HelicopterMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            
            Logger.Msg("HelicopterMod loading!"); 
            
            uConsole.RegisterCommand("seat_player", new Action(SeatPlayer));
        }

        public void SeatPlayer()
        {
            Logger.Msg("Load helicopter");
            string bundlePath = "Mods/helicoptermodassets";
            if (File.Exists(bundlePath))
            {
                Logger.Msg($"Asset bundle={bundlePath} exists!");
            }
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            GameObject prefab = bundle.LoadAsset<GameObject>("VEHICLE_McLeodsUltimatePersonalTransportationMachineA");
            Transform playerTransform = GameManager.GetPlayerTransform();
            
            GameObject helicopter = GameObject.Instantiate(prefab, playerTransform.position, playerTransform.rotation);
            
            Logger.Msg("Seating player..."); 
            PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
            playerManager.SetControlMode(PlayerControlMode.InSeat);
            Logger.Msg("Seated player?");
            
            Logger.Msg("Seat player in helicopter?"); 
            GameObject seat = helicopter.transform.Find("PlayerEnter").gameObject;
            playerTransform.SetParent(seat.transform);
            playerTransform.localPosition = Vector3.zero;
            playerTransform.localRotation = Quaternion.identity;
            Logger.Msg("Seated player in helicopter?"); 
        }
        
    }   
}