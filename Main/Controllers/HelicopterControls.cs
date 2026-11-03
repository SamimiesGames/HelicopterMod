using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace HelicopterMod.Main.Controllers
{
    [RegisterTypeInIl2Cpp]
    public class HelicopterControls : MonoBehaviour
    {
        public Rigidbody HelicopterRigidbody;
        
        private void Start()
        {
            HelicopterRigidbody = gameObject.AddComponent<Rigidbody>();
            HelicopterRigidbody.useGravity = true;
            HelicopterRigidbody.linearDamping = 0.2f;
            HelicopterRigidbody.angularDamping = 0.8f;
            HelicopterRigidbody.mass = 370f;

            PhysicsHelper.SetLayer(gameObject, 14);
            HelicopterRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.W))
            {
                HelicopterRigidbody.AddForce(HelicopterRigidbody.mass * 100f * Time.fixedDeltaTime * transform.up);
            }
            if (Input.GetKey(KeyCode.A))
            {
                HelicopterRigidbody.AddTorque(HelicopterRigidbody.mass * 100f * Time.fixedDeltaTime * Vector3.up);
            }
            if (Input.GetKey(KeyCode.D))
            {
                HelicopterRigidbody.AddTorque(HelicopterRigidbody.mass * 100f * Time.fixedDeltaTime * -Vector3.up);
            }
        }
    }
}
