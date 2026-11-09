using MelonLoader;
using UnityEngine;

namespace HelicopterMod.Main.Controllers
{
    [RegisterTypeInIl2Cpp]
    public class HelicopterInstruments : MonoBehaviour
    {
        public HelicopterControls helicopterControls;
        public Tachometer liftTachometer;
        public Tachometer climbRateTachometer;
        public Tachometer airSpeedTachometer;


        private void Update()
        {
            liftTachometer.value = helicopterControls.effectiveLift;
            climbRateTachometer.value = Mathf.InverseLerp(
                    -helicopterControls.flightModel.climbRate,
                    helicopterControls.flightModel.climbRate,
                    helicopterControls.helicopterRigidbody.velocity.y
            );
            airSpeedTachometer.value = helicopterControls.helicopterRigidbody.velocity.magnitude * 3.6f / 109f;
        }
    }
}