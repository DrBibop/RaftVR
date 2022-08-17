using RaftVR.Utils;
using UnityEngine;

namespace RaftVR.ItemComponents
{
    class SweepNetVR : TriggerReceiverTool
    {
        private SweepNet sweepNet;

        private Network_Player playerNetwork;

        protected override void Start()
        {
            base.Start();

            playerNetwork = GetComponentInParent<Network_Player>();

            sweepNet = GetComponent<SweepNet>();
            ReflectionInfos.toolOnPressUseEventField.SetValue(sweepNet, null);
            ReflectionInfos.toolSetAnimationField.SetValue(sweepNet, false);

            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 0.18f;
            collider.center = new Vector3(0f, 1.53f, -0.1f);

            Item_Base item = playerNetwork.PlayerItemManager.useItemController.GetCurrentItemInHand();
            ReflectionInfos.usableUseAnimationField.SetValue(item.settings_usable, PlayerAnimation.None);
        }

        // Raft doesn't have a separate method for this, so I had to borrow from them :P
        internal override void OnEnter(Collider collider)
        {
            if (collider.gameObject.tag != "Net") return;

            FMODUnity.RuntimeManager.PlayOneShotAttached((string)ReflectionInfos.netSwingEventField.GetValue(sweepNet), gameObject);

            PickupItem component = collider.gameObject.GetComponent<PickupItem>();
            ReflectionInfos.netPickupTargetField.SetValue(sweepNet, component);
            if ((bool)ReflectionInfos.netAttemptCaptureMethod.Invoke(sweepNet, new object[] { collider }))
            {
                playerNetwork.PickupScript.AddItemToInventory(component);
                ReflectionInfos.netPlayCatureSoundMethod.Invoke(sweepNet, null);
                playerNetwork.Inventory.RemoveDurabillityFromHotSlot(1);
            }
        }
    }
}
