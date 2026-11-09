using HelicopterMod.Main.Utilities;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.Serialization;

namespace HelicopterMod.Main.Controllers
{
    [Serializable]
    public class HelicopterControlsFlightModel
    {
        public float climbRate = 7.5f;
        public float maximumAcceleration = 21.8f;
        public float transverseFlowSpeed = 21f;
        public float optimalAccelerationAngle = 2f;
        public float retreatingBladeStallSpeed = 110f;
        public float dutchRollSpeed = 30f;
        public float groundEffectRpm = .64f;
        public float groundEffectDistance = 12.7f;
        public float rbsGraceFactor = .25f;
        public float collectiveExponent = 1.5f;
        public float oppositeDirectionAccelerationReduction = .4f;
        public float normalDrag = 0.7f;
        public float etlDrag = 1.4f;
        public float etlVerticalDrag = 2.1f;
        public float angularDrag = .4f;
        public float mass = 370f;
        public float dutchRollMultiplier = 2f;
        public float rollDueToYawMultiplier = 3f;
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
        public Vector3 sumOfAerodynamicFactors;
        public float collective;
    }
    
    [RegisterTypeInIl2Cpp]
    public class HelicopterControls : MonoBehaviour
    {
        public Rigidbody helicopterRigidbody;
        public Transform rotorUp;
        public Rotor mainRotor;
        public Seat seat;

        public HelicopterControlsFlightModel flightModel = new();
        public HelicopterControlsData data = new();

        public float pitch;
        public float effectiveLift;

        private float _deltaDebug;
        public bool debugEnabled;

        public override string ToString()
        {
            return $"HELICOPTER STATE:\n" +
                   $"RPM: {Mathf.RoundToInt(mainRotor.rpm * 400f)}\n" +
                   $"COL: {Mathf.CeilToInt(data.collective * 100f)}\n " +
                   $"SPD: {(helicopterRigidbody.velocity.magnitude * 3.6f).ToString("F1")}km/h\n" +
                   $"CLM: {helicopterRigidbody.velocity.y.ToString("F1")}m/s\n" +
                   $"X: {pitch.ToString("F1")}deg\n" +
                   $"RBS: {flightModel.retreatingBladeStallSpeed / 3.6f}m/s\n" +
                   $"optimalAccelerationAngle: {flightModel.optimalAccelerationAngle}\n" +
                   $"climbRate: {flightModel.climbRate}\n" +
                   $"maximumAcceleration: {flightModel.maximumAcceleration}\n";
        }

        private void Start()
        {
            helicopterRigidbody = gameObject.AddComponent<Rigidbody>();
            helicopterRigidbody.useGravity = true;
            helicopterRigidbody.linearDamping = flightModel.normalDrag;
            helicopterRigidbody.angularDamping = 1.2f;
            helicopterRigidbody.mass = flightModel.mass;

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
            pitch = Vector3.Angle(helicopterRigidbody.transform.forward, Vector3.up) - 90f;
        }

        private void _UpdateGroundEffect()
        {
            // Terrain mask (without Player LayerMask)
            int layerMask = Utils.m_PhysicalCollisionLayerMask;
            layerMask &= ~14; // Remove Player

            float rpmPow2 = Mathf.Pow(mainRotor.rpm, 2f);

            if (rpmPow2 < flightModel.groundEffectRpm) return;
            
            Physics.Raycast(
                helicopterRigidbody.position, 
                -rotorUp.up, 
                out RaycastHit hit, 
                12.7f * rpmPow2,
                layerMask
            );
            
            float scaledCollective = Mathf.Pow(data.collective, flightModel.collectiveExponent);

            float distanceRatio = Mathf.Pow(1f - Mathf.Clamp01(hit.distance / flightModel.groundEffectDistance), 1.2f);
            
            Vector3 normal = hit.normal;
            // Force the ground effect upwards relative to the rotor to force it to move away from a surface.
            normal.y = Mathf.Abs(normal.y) * Mathf.Sign(rotorUp.up.y);
            normal.Normalize();
            
            data.groundEffect = flightModel.climbRate * scaledCollective * distanceRatio * normal;
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

            float dutchRollWeight = Mathf.Clamp(Mathf.Pow(speed / flightModel.dutchRollSpeed, 2f), 0f, 2f) / 2f;
            float dutchRoll = flightModel.rotationSpeeds.y * flightModel.dutchRollMultiplier * yawToHeading * dutchRollWeight;
            newSumOfAeroDynamicFactors.y = dutchRoll;

            // Retreating blade stall. (Roll helicopter when over speeding.)
            float rbs = (speed / (flightModel.retreatingBladeStallSpeed / 3.6f)) - 1;
            
            rbs = Mathf.Clamp(rbs, 0f, 1f) / flightModel.rbsGraceFactor;
            rbs = Mathf.Clamp(rbs, 0f, 2f);
            
            float retreatingBladeStall = flightModel.rotationSpeeds.z * rbs;
            newSumOfAeroDynamicFactors.z = retreatingBladeStall;

            // Roll-due-to-yaw / sympathetic roll. Roll the helicopter to the opposite side of the yaw.
            float yawInDegrees = Vector3.Dot(helicopterRigidbody.angularVelocity, transform.up) * Mathf.Rad2Deg;

            float yawRatio = Mathf.Clamp(Mathf.Abs(yawInDegrees) / flightModel.rotationSpeeds.y, 0f, 1.5f);
            float rollDueToYaw = yawRatio * flightModel.rotationSpeeds.z * flightModel.rollDueToYawMultiplier * dutchRollWeight * Mathf.Sign(yawInDegrees);
            
            newSumOfAeroDynamicFactors.z += rollDueToYaw;

            data.sumOfAerodynamicFactors = Vector3.Lerp(
                data.sumOfAerodynamicFactors,
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
            data.flightControl = Vector3.zero;

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

                data.flightControl.x = yAxis;
                data.flightControl.y = xAxis;
            }
            
            if (Input.GetKey(KeyCode.Q))
            {
                data.flightControl.y += -1;
            }
            if (Input.GetKey(KeyCode.E))
            {
                data.flightControl.y += 1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                data.flightControl.z += 1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                data.flightControl.z += -1;
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                data.flightControl.x += 1;
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                data.flightControl.x += -1;
            }

            // Collective
            if (Input.GetKey(KeyCode.W))
            {
                data.collective += 0.5f * Time.fixedDeltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                data.collective += -0.5f * Time.fixedDeltaTime;
            }
            data.collective = Mathf.Clamp01(data.collective);
            
            data.flightControl.Normalize();
        }
        
        private void _UpdateAcceleration()
        {
            float rpmPow2 = Mathf.Pow(mainRotor.rpm, 2f);
            float scaledCollective = Mathf.Pow(data.collective, flightModel.collectiveExponent);
            
            effectiveLift = scaledCollective * rpmPow2;
            
            float pitchRatio = Mathf.Clamp(pitch / flightModel.optimalAccelerationAngle, -1f, 1f);
            
            float verticalRatio = Mathf.Clamp(1f - pitchRatio, .7f, 1f) * effectiveLift;
            float forwardRatio = pitchRatio * Mathf.Clamp(effectiveLift, .5f, 1f);
            
            Vector3 horizontal = forwardRatio * flightModel.maximumAcceleration * helicopterRigidbody.transform.forward;
            Vector3 vertical = verticalRatio * (flightModel.climbRate + -Physics.gravity.y) * rotorUp.up;
            vertical += data.groundEffect;

            // Flying in a different direction to velocity, reduce the acceleration.
            // This creates more momentum resulting in a smoother flying experience (very important).
            bool differentDirection = Vector3.Dot(horizontal.normalized, helicopterRigidbody.velocity.normalized) < 0;
            if (differentDirection)
            {
                horizontal = forwardRatio * (
                    flightModel.maximumAcceleration * flightModel.oppositeDirectionAccelerationReduction
                ) * helicopterRigidbody.transform.forward;
            }
            
            data.horizontalVelocity = Vector3.Lerp(data.horizontalVelocity, horizontal, .2f * Time.fixedDeltaTime);
            
            if (_deltaDebug > 1f && debugEnabled)
            {
                string fwdSuffix = differentDirection ? " (differentDirection)" : "";
                Logger.Msg(
                    $"\nFWD{fwdSuffix}: {data.horizontalVelocity} @ {(forwardRatio * 100f)}%\n" +
                    $"VER: {vertical} @ {(verticalRatio * 100f)}%\n" +
                    $"GND: {data.groundEffect}\n" +
                    $"LFT: {effectiveLift * 100f}%\n" +
                    $"AIR: {data.sumOfAerodynamicFactors}\n"
                );
            }

            bool inTransverseFlow = helicopterRigidbody.velocity.magnitude * 3.6f < flightModel.transverseFlowSpeed;
            helicopterRigidbody.linearDamping = flightModel.normalDrag;

            if (inTransverseFlow)
            {
                helicopterRigidbody.linearDamping = flightModel.etlDrag;
                vertical *= 1f - flightModel.etlVerticalDrag * Time.fixedDeltaTime;
            }
            
            Vector3 velocity = vertical + data.horizontalVelocity;
            Vector3 force = velocity * helicopterRigidbody.mass;
            
            helicopterRigidbody.AddForce(force, ForceMode.Force);
        }

        private void _UpdateRotation()
        {
            // Smooth inputs
            data.currentFlightControl = Vector3.Lerp(data.currentFlightControl, data.flightControl, 3.6f * Time.fixedDeltaTime);
            
            // Accumulate axis for angular velocity 
            Vector3 newAngularVelocity =
                (flightModel.rotationSpeeds.x * data.currentFlightControl.x + data.sumOfAerodynamicFactors.x) * transform.right +
                (flightModel.rotationSpeeds.y * data.currentFlightControl.y + data.sumOfAerodynamicFactors.y) * transform.up +
                (flightModel.rotationSpeeds.z * data.currentFlightControl.z + data.sumOfAerodynamicFactors.z) * transform.forward;
            newAngularVelocity *= Mathf.Deg2Rad;
            
            data.angularVelocity = Vector3.Lerp(data.angularVelocity, newAngularVelocity, 20f * Time.fixedDeltaTime);
            data.angularVelocity *= 1f - flightModel.angularDrag * Time.fixedDeltaTime;
            
            Vector3 direction = data.angularVelocity - helicopterRigidbody.angularVelocity;
            Vector3 localDirection = Quaternion.Inverse(transform.rotation) * direction;

            Vector3 torque = Vector3.Scale(helicopterRigidbody.inertiaTensor, localDirection);
            torque = (transform.rotation * torque) / Time.fixedDeltaTime;
            
            helicopterRigidbody.AddTorque(torque * Time.fixedDeltaTime, ForceMode.Force);
        }
    }
}
