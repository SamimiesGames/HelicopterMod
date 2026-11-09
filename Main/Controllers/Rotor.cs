using MelonLoader;
using UnityEngine;

namespace HelicopterMod.Main.Controllers
{
    [Serializable]
    public class RotorData
    {
        public float rpm;
    }
    
    [RegisterTypeInIl2Cpp]
    public class Rotor : MonoBehaviour
    {
        public float load;
        public float power;
        public float maxAcceleration;
        public float friction;
        public RotorData data = new();

        private float angle;

        private void Update()
        {
            data.rpm += maxAcceleration * power * Time.deltaTime;
            data.rpm *= 1f - (load + friction) * Time.deltaTime;
            
            data.rpm = Mathf.Clamp(data.rpm, 0f, 1f);

            _AnimateRotor();
        }

        private void _AnimateRotor()
        {
            float deltaRotation = data.rpm * 400f * 6f * Time.deltaTime;
            angle += deltaRotation;

            if (angle > 180f) angle -= 360f;
            if (angle < -180f) angle += 360f;
            
            transform.localRotation = Quaternion.Euler(0, angle, 0);
        }
    }
}

