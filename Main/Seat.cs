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
            playerManager.SetControlMode(PlayerControlMode.Locked);
            GameManager.m_vpFPSPlayer.Controller.enabled = false;
            GameManager.m_vpFPSPlayer.Controller.m_Controller.enabled = false;
            
            Transform playerTransform = GameManager.GetPlayerTransform();
            playerTransform.transform.position = transform.position;
            playerTransform.transform.rotation = transform.rotation;
        }

        private void OnDisable()
        {
            PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
            playerManager.SetControlMode(PlayerControlMode.Normal);
            GameManager.m_vpFPSPlayer.Controller.enabled = true;
            GameManager.m_vpFPSPlayer.Controller.m_Controller.enabled = true;
        }
    }    
}
