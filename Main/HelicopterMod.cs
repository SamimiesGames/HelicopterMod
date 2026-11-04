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
        public GameObject helicopter;
        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            
            Logger.Msg("HelicopterMod loading!"); 
            AssetBundleUtility.LoadBundle("helicopters", "Mods/helicoptermodassets");
            
            uConsole.RegisterCommand("spawn_helicopter", new Action(SpawnHelicopter));
            uConsole.RegisterCommand("unstuck_heli", new Action(UnstuckHelicopter));
        }

        public void UnstuckHelicopter()
        {
            if (!helicopter)
            {
                Logger.Warning($"No helicopter available!");
                return;
            }
            
            helicopter.transform.rotation = Quaternion.identity;
            helicopter.transform.position += Vector3.up * 4f;
        }

        public void SpawnHelicopter()
        {
            Logger.Msg("Load Helicopter");
            
            GameObject prefab = AssetBundleUtility.GetPrefab(
                "helicopters", 
                "VEHICLE_McLeodsUltimatePersonalTransportationMachineA"
            );
            Transform playerTransform = GameManager.GetPlayerTransform();
            
            helicopter = GameObject.Instantiate(prefab, playerTransform.position + Vector3.up * 2, playerTransform.rotation);
            Logger.Msg("Load Helicopter ... OK");
            
            Logger.Msg("Finding objects for rig"); 
            GameObject seat = helicopter.transform.Find("seat0").gameObject;
            Logger.Msg($"seat0={seat}");
            GameObject rotor = helicopter.transform.Find("rotor0").gameObject;
            Logger.Msg($"rotor0={rotor}");
            GameObject tachometer0 = helicopter.transform.Find("tachometer0").gameObject;
            Logger.Msg($"tachometer0={tachometer0}");
            GameObject tachometer1 = helicopter.transform.Find("tachometer1").gameObject;
            Logger.Msg($"tachometer1={tachometer1}");
            GameObject tachometer2 = helicopter.transform.Find("tachometer2").gameObject;
            Logger.Msg($"tachometer2={tachometer2}");
            Logger.Msg("Finding objects for rig ... OK"); 
            
            Logger.Msg($"Seat player");
            playerTransform.SetParent(seat.transform);
            playerTransform.localPosition = Vector3.zero;
            playerTransform.localRotation = Quaternion.identity;
            Logger.Msg($"Seat player ... OK");
            
            Logger.Msg($"Adding Seat");
            seat.AddComponent<Seat>();
            Logger.Msg($"Adding Seat ... OK");
            Logger.Msg($"Adding PilotFreeLook");
            PilotFreeLook freeLook = seat.AddComponent<PilotFreeLook>();
            freeLook.helicopter = helicopter;
            Logger.Msg($"Adding PilotFreeLook ... OK");
            
            Logger.Msg($"Adding HelicopterControls");
            HelicopterControls controls = helicopter.AddComponent<HelicopterControls>();
            Logger.Msg($"Adding HelicopterControls ... OK");
            controls.rotorUp = rotor.transform;
            
            Logger.Msg($"Adding Rotor");
            Rotor mainRotor = rotor.AddComponent<Rotor>();
            mainRotor.friction = .08f;
            mainRotor.power = 1f;
            mainRotor.maxAcceleration = 0.5f;
            
            controls.mainRotor = mainRotor;
            Logger.Msg($"Adding Rotor ... OK");
            
            Logger.Msg($"Adding Instruments");
            HelicopterInstruments instruments = helicopter.AddComponent<HelicopterInstruments>();
            instruments.helicopterControls = controls;
            instruments.liftTachometer = tachometer0.AddComponent<Tachometer>();
            instruments.climbRateTachometer = tachometer1.AddComponent<Tachometer>();
            instruments.airSpeedTachometer = tachometer2.AddComponent<Tachometer>();
            Logger.Msg($"Adding Instruments ... OK");
            
            Logger.Msg($"IgnoreCollisions");
            PhysicsHelper.IgnoreCollisions(helicopter, playerTransform.gameObject);
            Logger.Msg($"IgnoreCollisions ... OK");
            
            Logger.Msg("COMPLETE"); 
        }
        
    }   
}