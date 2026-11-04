using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace HelicopterMod.Main.Controllers
{
    [RegisterTypeInIl2Cpp]
    public class HelicopterControls : MonoBehaviour
    {
        public Rigidbody helicopterRigidbody;
        public Transform rotorUp;
        public Rotor mainRotor;
        public Seat seat;
        
        public float climbRate = 7.5f;
        public float maximumAcceleration = 21.8f;
        public float optimalAccelerationAngle = 2f;
        public Vector3 angularVelocity;
        public Vector3 rotationSpeeds = new Vector3(180f, 230f, 210f);
        public Vector3 flightControl;
        public Vector3 currentFlightControl;
        public Vector3 horizontalVelocity;
        public Vector3 groundEffect;
        public float pitch;
        public float collective;

        public float effectiveLift;

        private float _deltaDebug;

        public override string ToString()
        {
            return $"HELICOPTER STATE:\n" +
                   $"RPM: {Mathf.RoundToInt(mainRotor.rpm * 400f)}\n" +
                   $"COL: {Mathf.CeilToInt(collective * 100f)}\n " +
                   $"SPD: {(helicopterRigidbody.velocity.magnitude * 3.6f).ToString("F1")}km/h\n" +
                   $"CLM: {helicopterRigidbody.velocity.y.ToString("F1")}m/s\n" +
                   $"X: {pitch.ToString("F1")}deg\n" +
                   $"optimalAccelerationAngle: {optimalAccelerationAngle}\n" +
                   $"climbRate: {climbRate}\n" +
                   $"maximumAcceleration: {maximumAcceleration}\n";
        }

        private void Start()
        {
            helicopterRigidbody = gameObject.AddComponent<Rigidbody>();
            helicopterRigidbody.useGravity = true;
            helicopterRigidbody.linearDamping = 0.7f;
            helicopterRigidbody.angularDamping = 1.2f;
            helicopterRigidbody.mass = 370f;

            PhysicsHelper.SetLayer(gameObject, 14);
            helicopterRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void FixedUpdate()
        {
            _deltaDebug += Time.deltaTime;
            
            _UpdateTrigonometry();
            _UpdateGroundEffect();
            _UpdateFlightControl();
            _UpdateAcceleration();
            _UpdateRotation();
            /*
            if (_deltaDebug > 1f)
            {
                Logger.Msg(ToString());
                _deltaDebug = 0f;
            }
            */
        }
        
        private void _UpdateTrigonometry()
        {
            //float fuselagePitch = Vector3.Angle(helicopterRigidbody.transform.forward, rotorUp.up) - 90f;
            pitch = (Vector3.Angle(helicopterRigidbody.transform.forward, Vector3.up) - 90f);
        }

        private void _UpdateGroundEffect()
        {
            // Terrain mask (without Player LayerMask)
            int layerMask = Utils.m_PhysicalCollisionLayerMask;
            layerMask &= ~14; // Remove Player

            float rpmPow2 = Mathf.Pow(mainRotor.rpm, 2f);

            if (rpmPow2 < 0.64f) return;
            
            Physics.Raycast(
                helicopterRigidbody.position, 
                -rotorUp.up, 
                out RaycastHit hit, 
                12.7f * rpmPow2,
                layerMask
            );
            
            float scaledCollective = Mathf.Pow(collective, 1.5f);

            float distanceRatio = Mathf.Pow(1f - Mathf.Clamp01(hit.distance / 12.7f), 1.2f);
            groundEffect = climbRate * scaledCollective * distanceRatio * hit.normal;
        }
        
        private void _UpdateFlightControl()
        {
            flightControl = Vector3.zero;

            if (!seat.occupied)
            {
                mainRotor.power = 0f;
                return;
            }
            mainRotor.power = 1f;
            
            if (!Input.GetKey(KeyCode.Mouse1))
            {
                float xAxis = Input.GetAxis("Mouse X");
                float yAxis = -Input.GetAxis("Mouse Y");

                flightControl.x = yAxis;
                flightControl.y = xAxis;
            }
            
            if (Input.GetKey(KeyCode.Q))
            {
                flightControl.y += -1;
            }
            if (Input.GetKey(KeyCode.E))
            {
                flightControl.y += 1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                flightControl.z += 1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                flightControl.z += -1;
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                flightControl.x += 1;
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                flightControl.x += -1;
            }

            // Collective
            if (Input.GetKey(KeyCode.W))
            {
                collective += 0.5f * Time.fixedDeltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                collective += -0.5f * Time.fixedDeltaTime;
            }
            collective = Mathf.Clamp01(collective);
            
            flightControl.Normalize();
        }
        
        private void _UpdateAcceleration()
        {
            float rpmPow2 = Mathf.Pow(mainRotor.rpm, 2f);
            float pitchRatio = Mathf.Clamp(pitch / optimalAccelerationAngle, -1f, 1f);
            pitchRatio *= rpmPow2;
            
            float scaledCollective = Mathf.Pow(collective, 1.5f);

            effectiveLift = scaledCollective * rpmPow2;
            
            float verticalRatio = Mathf.Clamp(1f - pitchRatio, .7f, 1f) * scaledCollective;
            float forwardRatio = pitchRatio * Mathf.Clamp(scaledCollective, .5f, 1f);
            
            
            Vector3 horizontal = forwardRatio * maximumAcceleration * helicopterRigidbody.transform.forward;
            Vector3 vertical = verticalRatio * (climbRate + -Physics.gravity.y) * rotorUp.up;
            vertical += groundEffect;

            // Flying backwards, reduce acceleration. More momentum. Better feel.
            bool backwards = Vector3.Dot(horizontal.normalized, helicopterRigidbody.velocity.normalized) < 0;
            if (backwards)
            {
                horizontal = forwardRatio * (maximumAcceleration * 0.4f) * helicopterRigidbody.transform.forward;
            }
            
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, horizontal, .2f * Time.fixedDeltaTime);
            /*
            if (_deltaDebug > 1f)
            {
                string fwdSuffix = backwards ? " (backwards)" : "";
                Logger.Msg(
                    $"\nFWD{fwdSuffix}: {horizontalVelocity} @ {(forwardRatio * 100f)}%\n" +
                    $"VER: {vertical} @ {(verticalRatio * 100f)}%\n" +
                    $"GND: {groundEffect}\n" +
                    $"P: {(pitchRatio * 100f)}%\n" +
                    $"SCL: {(scaledCollective * 100f)}%\n"
                );
            }
            */
            
            helicopterRigidbody.AddForce((vertical + horizontalVelocity) * helicopterRigidbody.mass, ForceMode.Force);
        }

        private void _UpdateRotation()
        {
            // Smooth inputs
            currentFlightControl = Vector3.Lerp(currentFlightControl, flightControl, 3.6f * Time.fixedDeltaTime);
            // Accumulate axis for angular velocity 
            Vector3 newAngularVelocity =
                rotationSpeeds.x * Mathf.Deg2Rad * currentFlightControl.x * transform.right +
                rotationSpeeds.y * Mathf.Deg2Rad * currentFlightControl.y * transform.up +
                rotationSpeeds.z * Mathf.Deg2Rad * currentFlightControl.z * transform.forward;
            
            angularVelocity = Vector3.Lerp(angularVelocity, newAngularVelocity, 20f * Time.fixedDeltaTime);
            angularVelocity *= 1f - .5f * Time.fixedDeltaTime;
            
            Vector3 direction = angularVelocity - helicopterRigidbody.angularVelocity;
            Vector3 localDirection = Quaternion.Inverse(transform.rotation) * direction;

            Vector3 torque = Vector3.Scale(helicopterRigidbody.inertiaTensor, localDirection);
            torque = (transform.rotation * torque) / Time.fixedDeltaTime;
            
            helicopterRigidbody.AddTorque(torque * Time.fixedDeltaTime, ForceMode.Force);
        }
    }
}
