using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace HelicopterMod.Main
{
    [RegisterTypeInIl2Cpp]
    public class PilotFreeLook : MonoBehaviour
    {
        public GameObject helicopter;
        public Vector2 pitchLimits = new Vector2(-87, 87);
        public Vector2 yawLimits = new Vector2(-180f, 180f);

        private void LateUpdate()
        {
            /*
            if (!GameManager.m_vpFPSPlayer.FPSCamera.m_FreeCameraLookEnabled)
            {
                GameManager.m_vpFPSPlayer.FPSCamera.EnableFreeCameraLook(
                    pitchLimits,
                    yawLimits
                );
            }
            
            GameManager.GetPlayerTransform().rotation = helicopter.transform.rotation;
            */
        }

        private void OnDisable()
        {
            if (!GameManager.m_vpFPSPlayer || !GameManager.m_vpFPSPlayer.FPSCamera) return;
            GameManager.m_vpFPSPlayer.FPSCamera.DisableFreeCameraLook();
        }
    }
}
