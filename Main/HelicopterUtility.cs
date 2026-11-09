using HelicopterMod.Main.Controllers;
using HelicopterMod.Main.Utilities;
using Il2CppTLD.Interactions;
using UnityEngine;
using UnityEngine.Events;

namespace HelicopterMod.Main
{
    public struct HelicopterUtility
    {
        private static Seat _cachedSeat;
        private static HelicopterControls _cachedControls;
        private static HelicopterState _cachedState;
        private static string _lastPrefabName;

        public static string LastPrefabName => _lastPrefabName;
        
        public static GameObject LoadHelicopter(string prefabName)
        {
            _lastPrefabName = prefabName;
            GameObject prefab = AssetBundleUtility.GetPrefab(
                "helicopters", 
                prefabName
            );
            
            GameObject helicopter = GameObject.Instantiate(
                prefab, 
                ManagerCache.PlayerTransform.position + Vector3.up * 2,
                ManagerCache.PlayerTransform.rotation
            );
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
            GameObject enterInteraction = helicopter.transform.Find("interaction_enter").gameObject;
            Logger.Msg($"enterInteraction={enterInteraction}");
            Logger.Msg("Finding objects for rig ... OK"); 
            
            Logger.Msg($"Adding Seat");
            Seat seat = seat0.AddComponent<Seat>();
            _cachedSeat = seat;
            seat.SeatingComponents = new List<SeatingAction>();
            Logger.Msg($"Adding Seat ... OK");
            Logger.Msg($"Adding PilotFreeLook");
            PilotFreeLook freeLook = seat0.AddComponent<PilotFreeLook>();
            freeLook.enabled = false;
            seat.SeatingComponents.Add(freeLook);
            Logger.Msg($"Adding PilotFreeLook ... OK");
            
            Logger.Msg($"Adding HelicopterControls");
            HelicopterControls controls = helicopter.AddComponent<HelicopterControls>();
            _cachedControls = controls;
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
            
            Logger.Msg($"Adding BaseInteraction(enter)");
            
            SimpleInteraction interaction = enterInteraction.AddComponent<SimpleInteraction>();
            interaction.HoverText = "McLeod's Ultimate Personal Transportation Machine";
            interaction.AddEventCallback(InteractionEventType.PerformInteraction, (UnityAction<BaseInteraction>)new Action<BaseInteraction>(_ => seat.Enter()));
            
            enterInteraction.layer = LayerMask.NameToLayer("InteractivePropNoCollidePlayer");
            
            Logger.Msg($"Adding BaseInteraction(enter) ... OK");
            
            Logger.Msg($"IgnoreCollisions");
            PhysicsHelper.IgnoreCollisions(helicopter, ManagerCache.PlayerTransform.gameObject);
            Logger.Msg($"IgnoreCollisions ... OK");
            
            Logger.Msg("COMPLETE");

            return helicopter;
        }

        public static void EnterHelicopter()
        {
            if (!_cachedSeat)
            {
                Logger.Warning($"[HelicopterUtility] EnterHelicopter, no cached seat!");
                return;
            }
            
            _cachedSeat.Enter();
        }
        
        public static void StoreState()
        {
            if (!_cachedControls)
            {
                Logger.Warning($"[HelicopterUtility] StoreState failed; no cached controls!");
                return;
            }
            
            Logger.Msg($"[HelicopterUtility] Save state");
            _cachedState = GetState();
            Logger.Msg($"[HelicopterUtility] Save state ... OK");
        }

        public static void RestoreState()
        {
            if (_cachedState == null)
            {
                Logger.Warning($"[HelicopterUtility] Restore state failed; no cached state!");
                return;
            }
            Logger.Msg($"[HelicopterUtility] Restore state");
            SetState(_cachedState);
            Logger.Msg($"[HelicopterUtility] Restore state ... OK");
        }
        
        public static HelicopterState GetState()
        {
            return new HelicopterState
            {
                controlData = _cachedControls.data,
                RotorData = _cachedControls.mainRotor.data
            };
        }

        public static void SetState(HelicopterState state)
        {
            _cachedControls.data = state.controlData;
            _cachedControls.mainRotor.data = state.RotorData;
        }
    }
}
