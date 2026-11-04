using MelonLoader;
using UnityEngine;

namespace HelicopterMod.Main.Controllers
{
    [RegisterTypeInIl2Cpp]
    public class Tachometer : MonoBehaviour
    {
        public Vector2 angles = new Vector2(-160, 160);
        public float value;

        private Vector3 homeEuler;

        private void Awake()
        {
            homeEuler = transform.eulerAngles;
        }

        private void Update()
        {
            _AnimateNeedle();
        }

        private void _AnimateNeedle()
        {
            float angle = Mathf.LerpUnclamped(angles.x, angles.y, value);

            if (angle > 180f) angle -= 360f;
            if (angle < -180f) angle += 360f;
            
            transform.localRotation = Quaternion.Euler(homeEuler + Vector3.forward * angle);
        }
    }
}
