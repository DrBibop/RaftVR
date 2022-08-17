using RaftVR.Inputs;
using RaftVR.Rig;
using UnityEngine;

namespace RaftVR.ItemComponents
{
    class TwoHandedToolVR : MonoBehaviour
    {
        private Vector3 relativeDominantDirection = Vector3.down;
        private Quaternion nonDominantPivotRotation = Quaternion.Euler(2.979f, 0.253f, 5.106f);

        private Transform dominantTargetPivot;
        private Transform nonDominantTargetPivot;

        private bool isHoldingTwoHanded = false;
        private bool queuePivotDirection = false;
        private bool isTextShown = false;

        private DisplayTextManager textManager;

        protected void Start()
        {
            textManager = ComponentManager<DisplayTextManager>.Value;
        }

        private void OnDisable()
        {
            SetHoldTwoHanded(false);

            if (textManager && isTextShown)
            {
                textManager.HideDisplayTexts(1);
                isTextShown = false;
            }
        }

        private void Update()
        {
            if (CanvasHelper.ActiveMenu != MenuType.None) return;

            if (VRInput.TryGetButtonDown("RMB", out bool pressed) && pressed)
            {
                SetHoldTwoHanded(!isHoldingTwoHanded);
            }

            if (isHoldingTwoHanded)
            {
                if (textManager && isTextShown)
                {
                    textManager.HideDisplayTexts(1);
                    isTextShown = false;
                }

                VRRig.instance.UpdateHands();

                Vector3 desiredDominantDirection = (dominantTargetPivot.position - nonDominantTargetPivot.position).normalized;
                float dominantAngleDifference = Vector3.Angle(dominantTargetPivot.up, desiredDominantDirection);
                Vector3 dominantRotationAxis = Vector3.Cross(dominantTargetPivot.up, desiredDominantDirection).normalized;
                VRRig.instance.DominantController.transform.RotateAround(dominantTargetPivot.position, dominantRotationAxis, dominantAngleDifference);

                float nonDominantAngleDifference = Vector3.Angle(nonDominantTargetPivot.up, desiredDominantDirection);
                Vector3 nonDominantRotationAxis = Vector3.Cross(nonDominantTargetPivot.up, desiredDominantDirection).normalized;
                VRRig.instance.NonDominantController.transform.RotateAround(nonDominantTargetPivot.position, nonDominantRotationAxis, nonDominantAngleDifference);
            }
            else if (textManager && CanvasHelper.ActiveMenu == MenuType.None)
            {
                textManager.ShowText("Hold with two hands", MyInput.Keybinds["RMB"].MainKey, 1, 0, false);
                isTextShown = true;
            }
        }

        // A weird little hack that works
        private void LateUpdate()
        {
            if (queuePivotDirection && dominantTargetPivot)
            {
                dominantTargetPivot.up = transform.TransformDirection(relativeDominantDirection);
                queuePivotDirection = false;
            }
        }

        private void SetHoldTwoHanded(bool enabled)
        {
            if (isHoldingTwoHanded == enabled) return;

            isHoldingTwoHanded = enabled;

            if (isHoldingTwoHanded)
            {
                dominantTargetPivot = new GameObject("Dominant Target Pivot").transform;
                dominantTargetPivot.SetParent(VRRig.instance.DominantController.transform);
                dominantTargetPivot.localPosition = new Vector3(0.005f, -0.0359f, -0.0377f);
                dominantTargetPivot.up = transform.TransformDirection(relativeDominantDirection);
                queuePivotDirection = true;

                nonDominantTargetPivot = new GameObject("Non-Dominant Target Pivot").transform;
                nonDominantTargetPivot.SetParent(VRRig.instance.NonDominantController.transform); 
                nonDominantTargetPivot.localPosition = new Vector3(-0.008771034f, 0.00003706f, -0.02509881f);
                nonDominantTargetPivot.localRotation = nonDominantPivotRotation;
            }
            else
            {
                Destroy(dominantTargetPivot.gameObject);
                Destroy(nonDominantTargetPivot.gameObject);
            }
        }

        public void SetHoldDirection(Vector3 relativeDominantDirection, Quaternion nonDominantPivotRotation)
        {
            this.relativeDominantDirection = relativeDominantDirection;
            this.nonDominantPivotRotation = nonDominantPivotRotation;
        }
    }
}
