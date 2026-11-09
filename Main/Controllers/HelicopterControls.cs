using HelicopterMod.Main.Utilities;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace HelicopterMod.Main.Controllers
{
    [Serializable]
    public class HelicopterControlsParameters
    {
        public float climbRate = 7.5f;
        public float maximumAcceleration = 21.8f;
        public float optimalAccelerationAngle = 2f;
        public Vector3 rotationSpeeds = new Vector3(180f, 230f, 210f);
    }
    
    [Serializable]
    public class HelicopterControlsData
    {
        public Vector3 angularVelocity;
        public Vector3 flightControl;
        public Vector3 currentFlightControl;
        public Vector3 horizontalVelocity;
        public Vector3 groundEffect;
        public float pitch;
        public float collective;
    }
    
    [RegisterTypeInIl2Cpp]
    public class HelicopterControls : MonoBehaviour
    {
        public Rigidbody helicopterRigidbody;
        public Transform rotorUp;
        public Rotor mainRotor;
        public Seat seat;

        public HelicopterControlsParameters parameters;
        public HelicopterControlsData data;
        public float climbRate = 7.5f;
        public float maximumAcceleration = 21.8f;
        public float transverseFlow = 21f;
        public float optimalAccelerationAngle = 2f;
        public float retreatingBladeStallSpeed = 110f;
        public float dutchSpeed = 30f;
        public Vector3 rotationSpeeds = new Vector3(180f, 230f, 210f);
        public Vector3 angularVelocity;
        public Vector3 flightControl;
        public Vector3 currentFlightControl;
        public Vector3 horizontalVelocity;
        public Vector3 groundEffect;
        public Vector3 sumOfAerodynamicFactors;
        public float pitch;
        public float collective;

        public float effectiveLift;

        private float _deltaDebug;
        public bool debugEnabled;

        public override string ToString()
        {
            return $"HELICOPTER STATE:\n" +
                   $"RPM: {Mathf.RoundToInt(mainRotor.rpm * 400f)}\n" +
                   $"COL: {Mathf.CeilToInt(collective * 100f)}\n " +
                   $"SPD: {(helicopterRigidbody.velocity.magnitude * 3.6f).ToString("F1")}km/h\n" +
                   $"CLM: {helicopterRigidbody.velocity.y.ToString("F1")}m/s\n" +
                   $"X: {pitch.ToString("F1")}deg\n" +
                   $"RBS: {retreatingBladeStallSpeed / 3.6f}m/s\n" +
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
            _UpdateAeroDynamics();
            _UpdateFlightControl();
            _UpdateAcceleration();
            _UpdateRotation();
            
            if (_deltaDebug > 1f && debugEnabled)
            {
                Logger.Msg(ToString());
                _deltaDebug = 0f;
            }
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
            
            Vector3 normal = hit.normal;
            // Force the ground effect upwards relative to the rotor to force it to move away from a surface.
            normal.y = Mathf.Abs(normal.y) * Mathf.Sign(rotorUp.up.y);
            normal.Normalize();
            
            groundEffect = climbRate * scaledCollective * distanceRatio * normal;
        }

        private void _UpdateAeroDynamics()
        {
            Vector3 newSumOfAeroDynamicFactors = Vector3.zero;
            
            float speed = helicopterRigidbody.linearVelocity.magnitude;
            Vector3 heading = helicopterRigidbody.linearVelocity.normalized;
            Vector3 forward = helicopterRigidbody.transform.forward;

            bool sameDirection = Vector3.Dot(heading, forward) >= 0;
            if (!sameDirection) forward = -forward;
            
            // Face fuselage towards velocity. (Dutch roll)
            float yawToHeading = Mathf.Clamp(Vector3.SignedAngle(forward, heading, transform.up) / 90f, -1f, 1f);

            float dutchRollWeight = Mathf.Clamp(Mathf.Pow(speed / dutchSpeed, 2f), 0f, 2f) / 2f;
            float dutchRoll = rotationSpeeds.y * 2f * yawToHeading * dutchRollWeight;
            newSumOfAeroDynamicFactors.y = dutchRoll;

            // Retreating blade stall. (Roll helicopter when over speeding.)
            float rbs = (speed / (retreatingBladeStallSpeed / 3.6f)) - 1;
            
            rbs = Mathf.Clamp(rbs, 0f, 1f) / .25f;
            rbs = Mathf.Clamp(rbs, 0f, 2f);
            
            float retreatingBladeStall = rotationSpeeds.z * rbs;
            newSumOfAeroDynamicFactors.z = retreatingBladeStall;

            // Roll-due-to-yaw / sympathetic roll. Roll the helicopter to the opposite side of the yaw.
            float yawInDegrees = Vector3.Dot(helicopterRigidbody.angularVelocity, transform.up) * Mathf.Rad2Deg;

            float yawRatio = Mathf.Clamp(Mathf.Abs(yawInDegrees) / rotationSpeeds.y, 0f, 1.5f);
            float rollDueToYaw = yawRatio * rotationSpeeds.z * 3f * dutchRollWeight * Mathf.Sign(yawInDegrees);
            
            newSumOfAeroDynamicFactors.z += rollDueToYaw;

            sumOfAerodynamicFactors = Vector3.Lerp(
                sumOfAerodynamicFactors,
                newSumOfAeroDynamicFactors, 
                4f * Time.fixedDeltaTime
            );
            
            if (_deltaDebug > 1f && debugEnabled)
            {
                Logger.Msg(
                    "\nAERODYNAMICS:\n" +
                    $"dutchRoll: {dutchRoll}\n" +
                    $"retreatingBladeStall: {retreatingBladeStall}\n" +
                    $"rollDueToYaw: {rollDueToYaw} @ yawInDegrees: {yawInDegrees}\n" +
                    $"A: {helicopterRigidbody.angularVelocity}\n"
                );
            }
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
            float scaledCollective = Mathf.Pow(collective, 1.5f);
            
            effectiveLift = scaledCollective * rpmPow2;
            
            float pitchRatio = Mathf.Clamp(pitch / optimalAccelerationAngle, -1f, 1f);
            
            float verticalRatio = Mathf.Clamp(1f - pitchRatio, .7f, 1f) * effectiveLift;
            float forwardRatio = pitchRatio * Mathf.Clamp(effectiveLift, .5f, 1f);
            
            Vector3 horizontal = forwardRatio * maximumAcceleration * helicopterRigidbody.transform.forward;
            Vector3 vertical = verticalRatio * (climbRate + -Physics.gravity.y) * rotorUp.up;
            vertical += groundEffect;

            // Flying in a different direction to velocity, reduce the acceleration.
            // This creates more momentum resulting in a smoother flying experience (very important).
            bool differentDirection = Vector3.Dot(horizontal.normalized, helicopterRigidbody.velocity.normalized) < 0;
            if (differentDirection)
            {
                horizontal = forwardRatio * (maximumAcceleration * 0.4f) * helicopterRigidbody.transform.forward;
            }
            
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, horizontal, .2f * Time.fixedDeltaTime);
            
            if (_deltaDebug > 1f && debugEnabled)
            {
                string fwdSuffix = differentDirection ? " (differentDirection)" : "";
                Logger.Msg(
                    $"\nFWD{fwdSuffix}: {horizontalVelocity} @ {(forwardRatio * 100f)}%\n" +
                    $"VER: {vertical} @ {(verticalRatio * 100f)}%\n" +
                    $"GND: {groundEffect}\n" +
                    $"LFT: {effectiveLift * 100f}%\n" +
                    $"AIR: {sumOfAerodynamicFactors}\n"
                );
            }

            bool inTransverseFlow = helicopterRigidbody.velocity.magnitude * 3.6f < transverseFlow;
            helicopterRigidbody.linearDamping = 0.7f;

            if (inTransverseFlow)
            {
                helicopterRigidbody.linearDamping = 1.4f;
                vertical *= 1f - 2.1f * Time.fixedDeltaTime;
            }
            
            Vector3 velocity = vertical + horizontalVelocity;
            Vector3 force = velocity * helicopterRigidbody.mass;
            
            helicopterRigidbody.AddForce(force, ForceMode.Force);
        }

        private void _UpdateRotation()
        {
            // Smooth inputs
            currentFlightControl = Vector3.Lerp(currentFlightControl, flightControl, 3.6f * Time.fixedDeltaTime);
            
            // Accumulate axis for angular velocity 
            Vector3 newAngularVelocity =
                (rotationSpeeds.x * currentFlightControl.x + sumOfAerodynamicFactors.x) * transform.right +
                (rotationSpeeds.y * currentFlightControl.y + sumOfAerodynamicFactors.y) * transform.up +
                (rotationSpeeds.z * currentFlightControl.z + sumOfAerodynamicFactors.z) * transform.forward;
            newAngularVelocity *= Mathf.Deg2Rad;
            
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
