using FMODUnity;
using RaftVR.Utils;
using UnityEngine;

namespace RaftVR.ItemComponents
{
    class HookVR : TriggerReceiverTool
    {
        internal static HookVR instance { get; private set; }

        private PickupItem currentPickup;

        private Hook hook;

        private Network_Player playerNetwork;

        private CanvasHelper canvas;

        private bool pickedLastFrame;

        protected override void Start()
        {
            base.Start();

            hook = GetComponent<Hook>();
            playerNetwork = GetComponentInParent<Network_Player>();
            canvas = ComponentManager<CanvasHelper>.Value;

            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, -0.29f, 0f);
            collider.size = new Vector3(0.05f, 0.05f, 0.15f);
        }

        private void OnEnable()
        {
            instance = this;
        }

        internal bool HandleGathering()
        {
            if (currentPickup && !currentPickup.isActiveAndEnabled)
                currentPickup = null;

            if (currentPickup)
            {
                playerNetwork.Animator.anim.SetBool("ItemHit", true);
                pickedLastFrame = true;
                float gatherTimer = (float)ReflectionInfos.hookGatherTimerField.GetValue(hook);
                gatherTimer += Time.deltaTime;
                ReflectionInfos.hookGatherTimerField.SetValue(hook, gatherTimer);
                float num = gatherTimer / hook.gatherTime;
                if (num < 1f)
                {
                    canvas.SetLoadCircle(num);
                    return false;
                }
                playerNetwork.PickupScript.PickupItem(currentPickup, true, true);
                ReflectionInfos.hookStopCollectingMethod.Invoke(hook, null);
                currentPickup = null;
                playerNetwork.Inventory.RemoveDurabillityFromHotSlot(1);

                StudioEventEmitter eventEmitter_gather = (StudioEventEmitter)ReflectionInfos.hookGatherEmitterMethod.GetValue(hook);
                if (eventEmitter_gather != null)
                {
                    eventEmitter_gather.TriggerQue();
                }

                return false;
            }
            else if (pickedLastFrame)
            {
                StudioEventEmitter eventEmitter_gather = (StudioEventEmitter)ReflectionInfos.hookGatherEmitterMethod.GetValue(hook);
                if (eventEmitter_gather != null)
                {
                    eventEmitter_gather.Stop();
                }
                ReflectionInfos.hookStopCollectingMethod.Invoke(hook, null);
                ReflectionInfos.hookFinishGatheringMethod.Invoke(hook, null);
                pickedLastFrame = false;
            }

            return true;
        }

        internal override void OnEnter(Collider collider)
        {
            if (collider.gameObject.tag != "PickupHook") return;

            currentPickup = collider.gameObject.GetComponentInChildren<PickupItem>();
            ReflectionInfos.hookStartCollectingMethod.Invoke(hook, new object[] { currentPickup });
        }

        internal override void OnExit(Collider collider)
        {
            if (collider.gameObject.tag == "PickupHook" && currentPickup == collider.gameObject.GetComponentInChildren<PickupItem>())
            {
                currentPickup = null;
                return;
            }
        }
    }
}
