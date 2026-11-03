using Il2Cpp;
using MelonLoader;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HelicopterMod.Main
{
    [RegisterTypeInIl2Cpp]
    public class Seat : MonoBehaviour
    {
        private void Update()
        { 
            Transform playerTransform = GameManager.GetPlayerTransform();
            
            PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
            playerManager.SetControlMode(PlayerControlMode.Locked);
            
            playerTransform.transform.position = transform.position;
        }
    }    
}
