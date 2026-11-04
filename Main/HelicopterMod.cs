using HelicopterMod.Main.Controllers;
using MelonLoader;
using Il2Cpp;
using Il2CppTLD.Interactions;
using UnityEngine;
using UnityEngine.Events;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace HelicopterMod.Main
{
    public class HelicopterMod : MelonMod
    {
        public GameObject helicopter;
        public GameObject enterInteraction;
        
        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            
            Logger.Msg("HelicopterMod loading!"); 
            AssetBundleUtility.LoadBundle("helicopters", "Mods/helicoptermodassets");
            
            uConsole.RegisterCommand("spawn_helicopter", new Action(SpawnHelicopter));
            uConsole.RegisterCommand("unstuck_heli", new Action(UnstuckHelicopter));
            uConsole.RegisterCommand("layer_set", new Action(SetLayerOfInteraction));
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
            GameObject seat0 = helicopter.transform.Find("seat0").gameObject;
            Logger.Msg($"seat0={seat0}");
            GameObject rotor = helicopter.transform.Find("rotor0").gameObject;
            Logger.Msg($"rotor0={rotor}");
            GameObject tachometer0 = helicopter.transform.Find("tachometer0").gameObject;
            Logger.Msg($"tachometer0={tachometer0}");
            GameObject tachometer1 = helicopter.transform.Find("tachometer1").gameObject;
            Logger.Msg($"tachometer1={tachometer1}");
            GameObject tachometer2 = helicopter.transform.Find("tachometer2").gameObject;
            Logger.Msg($"tachometer2={tachometer2}");
            enterInteraction = helicopter.transform.Find("interaction_enter").gameObject;
            Logger.Msg($"tachometer2={tachometer2}");
            Logger.Msg("Finding objects for rig ... OK"); 
            
            Logger.Msg($"Adding Seat");
            Seat seat = seat0.AddComponent<Seat>();
            seat.SeatingComponents = new List<SeatingAction>();
            Logger.Msg($"Adding Seat ... OK");
            Logger.Msg($"Adding PilotFreeLook");
            PilotFreeLook freeLook = seat0.AddComponent<PilotFreeLook>();
            freeLook.enabled = false;
            seat.SeatingComponents.Add(freeLook);
            Logger.Msg($"Adding PilotFreeLook ... OK");
            
            Logger.Msg($"Adding HelicopterControls");
            HelicopterControls controls = helicopter.AddComponent<HelicopterControls>();
            controls.seat = seat;
            Logger.Msg($"Adding HelicopterControls ... OK");
            controls.rotorUp = rotor.transform;
            
            Logger.Msg($"Adding Rotor");
            Rotor mainRotor = rotor.AddComponent<Rotor>();
            mainRotor.friction = .08f;
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
            
            Logger.Msg($"Adding VehicleDoor(enter)");
            
            VehicleDoor interaction = enterInteraction.AddComponent<VehicleDoor>();
            interaction.HoldTime = 1.5f;
            interaction.HoverText = "McLeod's Ultimate Personal Transportation Machine";
            interaction.HoldText = "Enter";
            interaction.AddEventCallback(InteractionEventType.HoldEnd, (UnityAction<BaseInteraction>)new Action<BaseInteraction>(_ => seat.Enter()));
            enterInteraction.layer = LayerMask.NameToLayer("InteractivePropNoCollidePlayer");
            
            Logger.Msg($"Adding VehicleDoor(enter) ... OK");
            
            Logger.Msg($"IgnoreCollisions");
            PhysicsHelper.IgnoreCollisions(helicopter, playerTransform.gameObject);
            Logger.Msg($"IgnoreCollisions ... OK");
            
            Logger.Msg("COMPLETE"); 
        }

        public void SetLayerOfInteraction()
        {
            if (!helicopter)
            {
                Logger.Warning("No helicopter!");
                return;
            }

            if (uConsole.GetNumParameters() <= 0)
            {
                Logger.Warning("set_layer usage: set_layer <string: layerName>");
                return;
            }
            
            string layerName = uConsole.GetString();
            LayerMask layerMask = LayerMask.NameToLayer(layerName);
            enterInteraction.layer = layerMask;
            Logger.MsgConsole($"[SetLayerOfInteraction] to {layerName} -> {(int)layerMask}");
        }
    }   
}