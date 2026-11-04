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
        
        public void Enter(VehicleDoor door)
        {
            if (occupied)
            {
                Exit(door);
                return;
            }
            
            occupied = true;
            door.AddEventCallback(InteractionEventType.PerformInteraction, (UnityAction<BaseInteraction>)new Action<BaseInteraction>(_ => Exit(door)));

            
            Logger.Msg($"[Seat] Entered!");
            
            PlayerManager.SetControlMode(PlayerControlMode.InVehicle);
            GameManager.m_vpFPSPlayer.Controller.enabled = false;
            GameManager.m_vpFPSPlayer.Controller.m_Controller.enabled = false;
            
            Transform playerTransform = GameManager.GetPlayerTransform();
            
            playerTransform.SetParent(transform);
            playerTransform.localPosition = Vector3.zero;
            playerTransform.localRotation = Quaternion.identity;
            
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
            
            // Hopefully, override VehicleDoor's behaviour.
            PlayerManager.SetControlMode(PlayerControlMode.InVehicle);
            GameManager.m_vpFPSPlayer.Controller.enabled = false;
            GameManager.m_vpFPSPlayer.Controller.m_Controller.enabled = false;
        }

        public void Exit(VehicleDoor door)
        {
            occupied = false;
            door.AddEventCallback(InteractionEventType.PerformInteraction, (UnityAction<BaseInteraction>)new Action<BaseInteraction>(_ => Enter(door)));

            Logger.Msg($"[Seat] Exited!");
            Transform playerTransform = GameManager.GetPlayerTransform();
            playerTransform.SetParent(null);
            
            PlayerManager.SetControlMode(PlayerControlMode.Normal);
            GameManager.m_vpFPSPlayer.Controller.enabled = true;
            GameManager.m_vpFPSPlayer.Controller.m_Controller.enabled = true;

            foreach (var seatingAction in SeatingComponents)
            {
                Logger.Msg($"[Seat] seatingAction={seatingAction.name}.OnUnOccupied");
                seatingAction.OnUnOccupied();
            }
        }
    }    
}
