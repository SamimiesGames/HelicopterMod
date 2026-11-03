using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace HelicopterMod.Main
{
    [RegisterTypeInIl2Cpp]
    public class PilotFreeLook : MonoBehaviour
    {
        public Vector2 pitchLimits = new Vector2(-87, 87);
        public Vector2 yawLimits = new Vector2(-180f, 180f);

        private void Update()
        {
            if (!GameManager.m_vpFPSPlayer.FPSCamera.m_FreeCameraLookEnabled)
            {
                GameManager.m_vpFPSPlayer.FPSCamera.EnableFreeCameraLook(
                    pitchLimits,
                    yawLimits
                );
            }
        }

        private void OnDisable()
        {
            GameManager.m_vpFPSPlayer.FPSCamera.DisableFreeCameraLook();
        }
    }
}
