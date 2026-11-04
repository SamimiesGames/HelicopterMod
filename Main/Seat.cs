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
            occupied = true;
            
            PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
            playerManager.SetControlMode(PlayerControlMode.Locked);
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
                seatingAction.OnOccupied();
            }
        }

        public void Exit()
        {
            occupied = false;
            PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
            
            Transform playerTransform = GameManager.GetPlayerTransform();
            playerTransform.SetParent(null);
            
            playerManager.SetControlMode(PlayerControlMode.Normal);
            GameManager.m_vpFPSPlayer.Controller.enabled = true;
            GameManager.m_vpFPSPlayer.Controller.m_Controller.enabled = true;

            foreach (var seatingAction in SeatingComponents)
            {
                seatingAction.OnUnOccupied();
            }
        }
    }    
}
