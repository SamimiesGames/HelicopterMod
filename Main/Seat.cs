using Il2Cpp;
using Il2CppTLD.Interactions;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;

namespace HelicopterMod.Main
{
    [RegisterTypeInIl2Cpp]
    public class Seat : MonoBehaviour
    {
        public bool occupied;
        public List<SeatingAction> SeatingComponents;
        
        private PlayerManager PlayerManager
        {
            get
            {
                if (!_playerManager) _playerManager = GameManager.GetPlayerManagerComponent();
                return _playerManager;
            }
        }

        protected PlayerManager _playerManager;

        public void Enter()
        {
            if (occupied)
            {
                Exit();
                return;
            }
            
            if (PlayerManager.GetControlMode() != PlayerControlMode.Normal)
            {
                Logger.Msg($"[Seat] Cannot enter while performing an action!");
                return;
            }
            
            Logger.Msg($"[Seat] Entered!");
            occupied = true;
            HelicopterModState.TransitionState = HelicopterModTransitionState.InHelicopter;
            
            PlayerManager.SetControlMode(PlayerControlMode.InVehicle);
            GameManager.m_vpFPSPlayer.Controller.m_Controller.enabled = false;
            
            Transform playerTransform = GameManager.GetPlayerTransform();
            playerTransform.transform.position = transform.position;
            playerTransform.transform.rotation = transform.rotation;
            
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
            
            Transform playerTransform = GameManager.GetPlayerTransform();
            
            playerTransform.transform.position = transform.position;
            playerTransform.transform.rotation = transform.rotation;
        }

        public void Exit()
        {
            Logger.Msg($"[Seat] Exited!");
            occupied = false;
            HelicopterModState.TransitionState = HelicopterModTransitionState.OnFoot;

            PlayerManager.SetControlMode(PlayerControlMode.Normal);
            GameManager.m_vpFPSPlayer.Controller.m_Controller.enabled = true;
            
            foreach (var seatingAction in SeatingComponents)
            {
                Logger.Msg($"[Seat] seatingAction={seatingAction.name}.OnUnOccupied");
                seatingAction.OnUnOccupied();
            }
        }
        
        private void OnDisable()
        {
            Logger.Msg($"[Seat] OnDisable, cleanup");
            PlayerManager.SetControlMode(PlayerControlMode.Normal);
            GameManager.m_vpFPSPlayer.FPSCamera.enabled = true;
        }
    }    
}
