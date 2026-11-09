using HelicopterMod.Main.BoundarySystem;
using HelicopterMod.Main.Controllers;
using HelicopterMod.Main.Utilities;
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
        public static HelicopterMod Instance { get; private set; }
        public GameObject currentHelicopter;
        
        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            Instance = this;
            
            Logger.Msg("HelicopterMod loading!"); 
            AssetBundleUtility.LoadBundle("helicopters", "Mods/helicoptermodassets");
            
            uConsole.RegisterCommand("spawn_helicopter", new Action(SpawnHelicopter));
            uConsole.RegisterCommand("unstuck_heli", new Action(UnstuckHelicopter));
            uConsole.RegisterCommand("switch_scene", new Action(SwitchScene));
            
            uConsole.RegisterCommand("BE.Start", new Action(BoundaryEditor.Start));
            uConsole.RegisterCommand("BE.Export", new Action(BoundaryEditor.Export));
            uConsole.RegisterCommand("BE.SetExport", new Action(BoundaryEditor.SetExport));
            uConsole.RegisterCommand("BE.Create", new Action(BoundaryEditor.Create));
            uConsole.RegisterCommand("BE.Place", new Action(BoundaryEditor.Place));
            uConsole.RegisterCommand("BE.SetExit", new Action(BoundaryEditor.ExitPoint));
        }

        public void UnstuckHelicopter()
        {
            if (!currentHelicopter)
            {
                Logger.Warning($"No helicopter available!");
                return;
            }
            
            currentHelicopter.transform.rotation = Quaternion.identity;
            currentHelicopter.transform.position += Vector3.up * 4f;
        }

        public void SpawnHelicopter()
        {
            Logger.Msg("Load Helicopter");

            currentHelicopter = HelicopterUtility.LoadHelicopter("VEHICLE_McLeodsUltimatePersonalTransportationMachineA");
        }
        
        public void SwitchScene()
        {
            if (uConsole.GetNumParameters() <= 0)
            {
                Logger.Warning("switch_scene usage: switch_scene <string: sceneName>");
                return;
            }

            string sceneName = uConsole.GetString();
            
            RegionUtility.LoadScene(sceneName);
        }
    }

    public static class HelicopterModState
    {
        public static HelicopterModTransitionState TransitionState;
        public static string LastActiveScene;
    }

    public enum HelicopterModTransitionState
    {
        OnFoot,
        InHelicopter
    }
}