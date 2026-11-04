using Il2Cpp;
using MelonLoader;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HelicopterMod.Main
{
    [RegisterTypeInIl2Cpp]
    public class Seat : MonoBehaviour
    {
        private void OnEnable()
        {
            PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
            playerManager.SetControlMode(PlayerControlMode.InVehicle);
            GameManager.m_vpFPSPlayer.Controller.enabled = false;
            
            Transform playerTransform = GameManager.GetPlayerTransform();
            playerTransform.transform.position = transform.position;
            playerTransform.transform.rotation = transform.rotation;
        }

        private void Update()
        { 
            Transform playerTransform = GameManager.GetPlayerTransform();
            
            playerTransform.transform.rotation = transform.rotation;
        }

        private void OnDisable()
        {
            PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
            playerManager.SetControlMode(PlayerControlMode.Normal);
            GameManager.m_vpFPSPlayer.Controller.enabled = true;
        }
    }    
}
