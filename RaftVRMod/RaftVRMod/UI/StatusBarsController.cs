using RaftVR.Rig;
using UnityEngine;

namespace RaftVR.UI
{
    class StatusBarsController : MonoBehaviour
    {
        private float scaleLerp = 1f;
        private Vector3 shownScale = Vector3.one * 0.0004f;

        private void LateUpdate()
        {
            bool show = Vector3.Angle(transform.forward, transform.position - VRRig.instance.uiCamera.transform.position) <= 70;

            if (scaleLerp != (show ? 1f : 0f))
            {
                scaleLerp = Mathf.Clamp(scaleLerp + (show ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime) * 6, 0f, 1f);

                transform.localScale = Vector3.Lerp(Vector3.zero, shownScale, scaleLerp);
            }
        }
    }
}
