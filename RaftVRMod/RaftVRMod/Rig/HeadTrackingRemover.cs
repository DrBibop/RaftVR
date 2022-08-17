using UnityEngine;
using UnityEngine.XR;

namespace RaftVR.Rig
{
    class HeadTrackingRemover : MonoBehaviour
    {
        public void Start()
        {
            //This little method isn't enough to completely stop tracking as there's still rotational tracking.
            XRDevice.DisableAutoXRCameraTracking(GetComponent<Camera>(), true);
        }

        private void LateUpdate()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
}
