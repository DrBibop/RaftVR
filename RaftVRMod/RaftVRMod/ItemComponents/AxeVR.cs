using RaftVR.Utils;
using Steamworks;
using UnityEngine;

namespace RaftVR.ItemComponents
{
    class AxeVR : MonoBehaviour
    {
        private Axe axe;
        private Network_Player playerNetwork;
        private float cooldown;
        private float cooldownTimer;
        private CanvasHelper canvas;

        private void Start()
        {
            axe = GetComponent<Axe>();

            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;

            playerNetwork = GetComponentInParent<Network_Player>();

            Item_Base item = playerNetwork.PlayerItemManager.useItemController.GetCurrentItemInHand();

            cooldown = item.settings_usable.UseButtonCooldown;

            canvas = ComponentManager<CanvasHelper>.Value;

            // Required for collision detection to work
            gameObject.layer = LayerMask.NameToLayer("Projectiles");
        }

        private void OnEnable()
        {
            cooldownTimer = 0f;
        }

        private void Update()
        {
            if (cooldownTimer > 0)
            {
                cooldownTimer = Mathf.Max(0f, cooldownTimer - Time.deltaTime);
                canvas.SetLoadCircle(cooldownTimer / cooldown);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (cooldownTimer > 0 || PlayerItemManager.IsBusy) return;

            if (collision.gameObject.tag == "Tree")
            {
                OnTreeHit(collision);
            }
        }

        // Raft doesn't have a separate method for this, so I had to borrow from them :P
        private void OnTreeHit(Collision collision)
        {
            HarvestableTree tree = collision.transform.GetComponentInParent<HarvestableTree>();
            if (tree == null || tree.Depleted) return;

            cooldownTimer = cooldown;

            Message_AxeHit message_AxeHit = new Message_AxeHit(Messages.AxeHit, this.playerNetwork, this.playerNetwork.steamID);
            message_AxeHit.treeObjectIndex = (int)tree.PickupNetwork.ObjectIndex;

            if (Raft_Network.IsHost)
            {
                tree.Harvest(playerNetwork.Inventory);
            }

            message_AxeHit.HitPoint = collision.GetContact(0).point;
            message_AxeHit.HitNormal = collision.GetContact(0).normal;

            playerNetwork.Inventory.RemoveDurabillityFromHotSlot(1);

            if (Raft_Network.IsHost)
            {
                playerNetwork.Network.RPC(message_AxeHit, Target.Other, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
                axe.PlayEffect(playerNetwork.transform.parent, message_AxeHit.HitPoint, message_AxeHit.HitNormal);
            }
            else
            {
                playerNetwork.SendP2P(message_AxeHit, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
            }

            if (axe.mode == AxeMode.Chopping)
            {
                axe.mode = AxeMode.None;
            }
        }
    }
}
