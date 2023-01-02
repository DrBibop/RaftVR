using RaftVR.Configs;
using RaftVR.Inputs;
using RaftVR.Rig;
using RaftVR.Utils;
using UnityEngine;

namespace RaftVR.ItemComponents
{
    class BowVR : MonoBehaviour
    {
        private ThrowableComponent_Bow bow;
        private ChargeMeter bowCharge;
        private const float MIN_DISTANCE = 0.22f;
        private const float MAX_DISTANCE = 0.6f;
        private GameObject realBowTransform;
        private int holdFrames = 0;
        private Transform stringBone;
        private Transform bottomBowBone;
        private Transform topBowBone;
        private Transform bottomBowBone2;
        private Transform topBowBone2;
        private Transform bottomBowBone3;
        private Transform topBowBone3;
        private float ORIG_ROPE_Z = 0.002683922f;
        internal static bool canPull = true;

        private void Start()
        {
            bow = GetComponent<ThrowableComponent_Bow>();
            bowCharge = GetComponent<ChargeMeter>();

            Transform bowRoot = transform.Find("bow animator/Armature_Bow/Root");

            if (bowRoot)
            {
                stringBone = bowRoot.Find("StringMiddle");
                bottomBowBone = bowRoot.Find("BowBottom1");
                topBowBone = bowRoot.Find("BowTop1");
                bottomBowBone2 = bottomBowBone.Find("BowBottom2");
                topBowBone2 = topBowBone.Find("BowTop2");
                bottomBowBone3 = bottomBowBone2.Find("StringBottom");
                topBowBone3 = topBowBone2.Find("Stringtop");
            }
        }

        private void Update()
        {
            if (!VRConfigs.ImmersiveBow)
            {
                holdFrames = 0;
                canPull = true;
                return;
            }

            if (CanvasHelper.ActiveMenu != MenuType.None) return;

            if (!PlayerItemManager.IsBusy || bowCharge.ChargeNormal > 0 || holdFrames > 0)
            {
                Vector3 realArrowPosition = VRRig.instance.DominantController.transform.TransformPoint(new Vector3(0f, -0.02f, 0f));

                float handsDistance = Vector3.Distance(VRRig.instance.DominantController.transform.position, VRRig.instance.NonDominantController.transform.position);
                bool handIsClose = handsDistance > MIN_DISTANCE - 0.15f && handsDistance < MIN_DISTANCE + 0.15f;

                canPull = holdFrames == 2 || handIsClose;

                if (VRInput.TryGetButton("LMB", out bool holdingPrimary) && holdingPrimary && (bool)ReflectionInfos.throwableCanThrowField.GetValue(bow) && handIsClose && !realBowTransform && bow.HasAmmo())
                {
                    Vector3 realBowPosition = VRRig.instance.NonDominantHandIKTarget.TransformPoint(VRRig.instance.NonDominantHandPlayerBone.transform.InverseTransformPoint(transform.position + (transform.up * 0.03f)));
                    Vector3 realBowForward = VRRig.instance.NonDominantHandIKTarget.TransformDirection(VRRig.instance.NonDominantHandPlayerBone.transform.InverseTransformDirection(-transform.forward));
                    Vector3 realBowUp = VRRig.instance.NonDominantHandIKTarget.TransformDirection(VRRig.instance.NonDominantHandPlayerBone.transform.InverseTransformDirection(transform.up));

                    realBowTransform = new GameObject("Real Bow Transform");
                    realBowTransform.transform.position = realBowPosition;
                    realBowTransform.transform.rotation = Quaternion.LookRotation(realBowForward, realBowUp);
                    realBowTransform.transform.SetParent(VRRig.instance.NonDominantController.transform);
                }

                if (VRInput.TryGetButtonDown("RMB", out bool pressedSecondary) && pressedSecondary && realBowTransform != null)
                {
                    holdFrames = 0;
                }

                if (holdingPrimary && !pressedSecondary && realBowTransform != null)
                {
                    holdFrames = 2;
                }
                else if (holdFrames > 0)
                {
                    holdFrames--;
                }

                if (holdFrames > 0)
                {
                    VRRig.instance.UpdateHands();

                    Vector3 pullVector = realBowTransform.transform.position - realArrowPosition;

                    float nonDominantAngleDifference = Vector3.Angle(realBowTransform.transform.forward, pullVector);
                    Vector3 nonDominantRotationAxis = Vector3.Cross(realBowTransform.transform.forward, pullVector).normalized;
                    VRRig.instance.NonDominantController.transform.RotateAround(realBowTransform.transform.position, nonDominantRotationAxis, nonDominantAngleDifference);

                    Vector3 targetDominantUp = realBowTransform.transform.up * (Vector3.Angle(realBowTransform.transform.up, VRRig.instance.DominantController.transform.up) < 90 ? 1 : -1);

                    float dominantAngleDifference = Vector3.Angle(VRRig.instance.DominantController.transform.forward, pullVector);
                    Vector3 dominantRotationAxis = Vector3.Cross(VRRig.instance.DominantController.transform.forward, pullVector).normalized;
                    VRRig.instance.DominantController.transform.RotateAround(realArrowPosition, dominantRotationAxis, dominantAngleDifference);

                    float dominantUpDifference = Vector3.SignedAngle(VRRig.instance.DominantController.transform.up, targetDominantUp, VRRig.instance.DominantController.transform.forward);
                    VRRig.instance.DominantController.transform.RotateAround(realArrowPosition, VRRig.instance.DominantController.transform.forward, dominantUpDifference);

                    if (holdFrames == 2)
                    {
                        float clampedMagnitude = Mathf.Clamp(pullVector.magnitude, MIN_DISTANCE + 0.001f, MAX_DISTANCE);

                        if (clampedMagnitude != pullVector.magnitude)
                        {
                            VRRig.instance.DominantController.transform.position = realBowTransform.transform.position - (pullVector.normalized * clampedMagnitude) - (realArrowPosition - VRRig.instance.DominantController.transform.position);
                        }

                        float charge = (clampedMagnitude - MIN_DISTANCE) / (MAX_DISTANCE - MIN_DISTANCE);

                        float minCharge = (float)ReflectionInfos.chargeMeterMinChargeField.GetValue(bowCharge);
                        float maxCharge = (float)ReflectionInfos.chargeMeterMaxChargeField.GetValue(bowCharge);

                        ReflectionInfos.chargeMeterCurrentChargeField.SetValue(bowCharge, minCharge + (maxCharge - minCharge) * charge);
                    }
                }
                else if (realBowTransform)
                {
                    VRRig.instance.UpdateHands();

                    Vector3 handsVector = realBowTransform.transform.position - realArrowPosition;

                    float nonDominantAngleDifference = Vector3.Angle(realBowTransform.transform.forward, handsVector);
                    Vector3 nonDominantRotationAxis = Vector3.Cross(realBowTransform.transform.forward, handsVector).normalized;

                    VRRig.instance.NonDominantController.transform.RotateAround(realBowTransform.transform.position, nonDominantRotationAxis, nonDominantAngleDifference);

                    Destroy(realBowTransform);
                }
            }
        }

        private void LateUpdate()
        {
            if (holdFrames == 2)
            {
                stringBone.localPosition = Vector3.Lerp(new Vector3(0f, 0.00195027f, ORIG_ROPE_Z), new Vector3(0f, 0.00195027f, ORIG_ROPE_Z + (MAX_DISTANCE - MIN_DISTANCE) / stringBone.lossyScale.x), bowCharge.ChargeNormal);

                bottomBowBone.localRotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(156.213f, -0.3089905f, -0.3739929f)), Quaternion.Euler(new Vector3(147.933f, -0.3729858f, -0.4029846f)), bowCharge.ChargeNormal);
                bottomBowBone2.localRotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(-17.869f, -0.363f, 0.262f)), Quaternion.Euler(new Vector3(-28.127f, -0.416f, 0.282f)), bowCharge.ChargeNormal);
                Vector3 stringVector1 = (stringBone.position - bottomBowBone3.position).normalized;
                bottomBowBone3.rotation = Quaternion.LookRotation(Vector3.Cross(bottomBowBone3.right, stringVector1), stringVector1);

                topBowBone.localRotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(23.9f, -0.273f, -0.323f)), Quaternion.Euler(new Vector3(31.173f, -0.321f, -0.345f)), bowCharge.ChargeNormal);
                topBowBone2.localRotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(18.198f, 0.32f, 0.224f)), Quaternion.Euler(new Vector3(27.379f, 0.36f, 0.239f)), bowCharge.ChargeNormal);
                Vector3 stringVector2 = (stringBone.position - topBowBone3.position).normalized;
                topBowBone3.rotation = Quaternion.LookRotation(Vector3.Cross(topBowBone3.right, stringVector2), stringVector2);
            }
        }
    }
}
