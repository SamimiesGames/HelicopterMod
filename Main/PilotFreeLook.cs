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
        public Vector2 yawLimits = new Vector2(-1080f, 1080f);

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
            if (!GameManager.m_vpFPSPlayer || !GameManager.m_vpFPSPlayer.FPSCamera) return;
            GameManager.m_vpFPSPlayer.FPSCamera.DisableFreeCameraLook();
        }
    }
}
