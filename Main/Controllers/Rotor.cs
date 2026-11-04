using MelonLoader;
using UnityEngine;

namespace HelicopterMod.Main.Controllers
{
    [RegisterTypeInIl2Cpp]
    public class Rotor : MonoBehaviour
    {
        public float load;
        public float power;
        public float maxAcceleration;
        public float friction;
        public float rpm;

        private float angle;

        private void Update()
        {
            rpm += maxAcceleration * power * Time.deltaTime;
            rpm *= 1f - (load + friction) * Time.deltaTime;
            
            rpm = Mathf.Clamp(rpm, 0f, 1f);

            _AnimateRotor();
        }

        private void _AnimateRotor()
        {
            float deltaRotation = rpm * 400f * 6f * Time.deltaTime;
            angle += deltaRotation;

            if (angle > 180f) angle -= 360f;
            if (angle < -180f) angle += 360f;
            
            transform.localRotation = Quaternion.Euler(0, angle, 0);
        }
    }
}

