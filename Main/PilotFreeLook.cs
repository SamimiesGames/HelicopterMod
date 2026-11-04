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
        public Vector2 yawLimits = new Vector2(-190f, 190f);

        public float yaw;
        public float pitch;
        
        private void Update()
        {
            GameManager.m_vpFPSPlayer.FPSCamera.enabled = false;

            float newYaw = 0f;
            float newPitch = 8f;
            
            if (Input.GetKey(KeyCode.Mouse1))
            {
                newYaw = yaw + Input.GetAxis("Mouse X") * 270f * Time.deltaTime;
                newPitch = pitch + -Input.GetAxis("Mouse Y") * 180f * Time.deltaTime;
            }

            newYaw = Mathf.Clamp(newYaw, yawLimits.x, yawLimits.y);
            newPitch = Mathf.Clamp(newPitch, pitchLimits.x, pitchLimits.y);

            float rateOfChange = newPitch == 0f ? 65f : 180f;

            yaw = Mathf.MoveTowardsAngle(yaw, newYaw, rateOfChange * Time.deltaTime);
            pitch = Mathf.MoveTowardsAngle(pitch, newPitch, rateOfChange * Time.deltaTime);
            
            GameManager.m_vpFPSPlayer.FPSCamera.m_Camera.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void OnDisable()
        {
            if (!Application.isPlaying || !GameManager.m_vpFPSPlayer || !GameManager.m_vpFPSPlayer.FPSCamera) return;
            GameManager.m_vpFPSPlayer.FPSCamera.m_Camera.transform.localRotation = Quaternion.Euler(0, 0, 0);
            GameManager.m_vpFPSPlayer.FPSCamera.enabled = true;
        }
    }
}
