using RaftVR.Configs;
using RaftVR.Inputs;
using RaftVR.Rig;
using UnityEngine;
using UnityEngine.XR;

namespace RaftVR.UI
{
    class CalibrationController : MonoBehaviour
    {
        private const float CHARACTER_ARM_LENGTH = 0.433f;
        private const float CHARACTER_SHOULDERS_WIDTH = 0.5f;

        private void Update()
        {
            if (VRInput.TryGetButtonDown("Calibrate", out bool calibrateDown) && calibrateDown)
            {
                float playerWingspan = Vector3.Distance(VRRig.instance.RightHandIKTarget.position, VRRig.instance.LeftHandIKTarget.position);
                float playerArmLength = (playerWingspan - CHARACTER_SHOULDERS_WIDTH) / 2;
                VRConfigs.ArmScale = playerArmLength / CHARACTER_ARM_LENGTH;
                VRConfigs.RefreshHiddenSettings();

                Recenter();

                gameObject.SetActive(false);
            }
        }

        private void Recenter()
        {
            if (VRConfigs.Runtime == VRConfigs.VRRuntime.SteamVR)
            {
                Valve.VR.OpenVR.Chaperone.ResetZeroPose(Valve.VR.ETrackingUniverseOrigin.TrackingUniverseStanding);
            }
            else
            {
                // Ignore this warning. We're not using the new XR system.
                InputTracking.Recenter();
            }
        }
    }
}
