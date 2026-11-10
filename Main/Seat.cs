using HelicopterMod.Main.BoundarySystem;
using HelicopterMod.Main.Utilities;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace HelicopterMod.Main
{
    [RegisterTypeInIl2Cpp]
    public class Seat : MonoBehaviour
    {
        public bool occupied;
        public List<SeatingAction> SeatingComponents;

        public void Enter()
        {
            if (occupied)
            {
                Exit();
                return;
            }
            
            if (ManagerCache.PlayerManager.GetControlMode() != PlayerControlMode.Normal)
            {
                Logger.Msg($"[Seat] Cannot enter while performing an action!");
                return;
            }
            
            Logger.Msg($"[Seat] Entered!");
            occupied = true;
            HelicopterModState.TransitionState = HelicopterModTransitionState.InHelicopter;
            BoundaryCache.SetLayerActive(true);
            
            ManagerCache.PlayerManager.SetControlMode(PlayerControlMode.InVehicle);
            GameManager.m_vpFPSPlayer.Controller.m_Controller.enabled = false;
            
            ManagerCache.PlayerTransform.position = transform.position;
            ManagerCache.PlayerTransform.rotation = transform.rotation;
            
            foreach (var seatingAction in SeatingComponents)
            {
                Logger.Msg($"[Seat] seatingAction={seatingAction.name}.OnOccupied");
                seatingAction.OnOccupied();
            }
        }

        private void Update()
        {
            if (!occupied) return;

            PlayerStruggle struggle = GameManager.GetPlayerStruggleComponent();
            if (struggle && struggle.InStruggle())
            {
                Exit();
                return;
            }

            if (!ManagerCache.PlayerTransform) return;
            
            ManagerCache.PlayerTransform.position = transform.position;
            ManagerCache.PlayerTransform.rotation = transform.rotation;
        }

        public void Exit()
        {
            Logger.Msg($"[Seat] Exited!");
            occupied = false;
            HelicopterModState.TransitionState = HelicopterModTransitionState.OnFoot;
            BoundaryCache.SetLayerActive(false);


            ManagerCache.PlayerManager.SetControlMode(PlayerControlMode.Normal);
            GameManager.m_vpFPSPlayer.Controller.m_Controller.enabled = true;
            
            foreach (var seatingAction in SeatingComponents)
            {
                seatingAction.OnUnOccupied();
            }
        }

        public void ForceExitDueToTransition()
        {
            Logger.Msg($"[Seat] ForceExitDueToTransition!");
            occupied = false;
            BoundaryCache.SetLayerActive(false);
            
            GameManager.m_vpFPSPlayer.Controller.m_Controller.enabled = true;
            
            foreach (var seatingAction in SeatingComponents)
            {
                seatingAction.OnUnOccupied();
            }
        }
        
        private void OnDisable()
        {
            Logger.Msg($"[Seat] OnDisable, attempting cleanup");
            if (!ManagerCache.PlayerManager) return;
            Logger.Msg($"[Seat] OnDisable, PlayerManager exists; continuing with cleanup.");

            if (HelicopterModState.TransitionState == HelicopterModTransitionState.InHelicopter)
            {
                Logger.Msg($"[Seat] OnDisable, player in helicopter. Skipping.");
                return;
            }
            
            ManagerCache.PlayerManager.SetControlMode(PlayerControlMode.Normal);
            GameManager.m_vpFPSPlayer.FPSCamera.enabled = true;
        }
    }    
}
