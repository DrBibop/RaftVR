using RaftVR.Utils;
using UnityEngine;

namespace RaftVR.ItemComponents
{
    class ShovelVR : TriggerReceiverTool
    {
        private Shovel shovel;

        private Network_Player playerNetwork;

        private PickupItem currentPickup;

        private bool channelledLastFrame;

        protected override void Start()
        {
            base.Start();

            shovel = GetComponent<Shovel>();
            playerNetwork = GetComponentInParent<Network_Player>();

            ReflectionInfos.toolOnPressUseEventField.SetValue(shovel, null);
            ReflectionInfos.toolOnReleaseUseEventField.SetValue(shovel, null);
            ReflectionInfos.toolSetAnimationField.SetValue(shovel, false);

            Item_Base item = playerNetwork.PlayerItemManager.useItemController.GetCurrentItemInHand();

            ReflectionInfos.usableUseAnimationField.SetValue(item.settings_usable, PlayerAnimation.None);

            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.1f, 1.3f);
            collider.size = new Vector3(0.3f, 0.2f, 0.2f);
        }

        private void Update()
        {
            if (currentPickup && !currentPickup.isActiveAndEnabled)
                currentPickup = null;

            if (currentPickup)
            {
                channelledLastFrame = true;
                ReflectionInfos.shovelCurrentTargetField.SetValue(shovel, currentPickup);
                shovel.ChannelItem();
            }
            else if (channelledLastFrame)
            {
                channelledLastFrame = false;
                shovel.ChannelItem();
                shovel.DigThrow();
                ReflectionInfos.shovelResetMethod.Invoke(shovel, null);
            }
        }

        internal override void OnEnter(Collider collider)
        {
            if (collider.gameObject.tag == "Pickup_Shovel")
            {
                currentPickup = collider.gameObject.GetComponent<PickupItem>();
                shovel.ChannelItem();
                ReflectionInfos.shovelCurrentTargetField.SetValue(shovel, currentPickup);
                shovel.DigDown();
                return;
            }
        }

        internal override void OnExit(Collider collider)
        {
            if (collider.gameObject.tag == "Pickup_Shovel" && currentPickup == collider.gameObject.GetComponent<PickupItem>())
            {
                currentPickup = null;
                return;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Obstruction"))
            {
                shovel.DigDown();
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Obstruction"))
            {
                shovel.DigThrow();
            }
        }
    }
}
