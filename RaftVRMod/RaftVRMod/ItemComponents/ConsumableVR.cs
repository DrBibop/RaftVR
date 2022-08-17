using RaftVR.Rig;
using RaftVR.Utils;
using UnityEngine;

namespace RaftVR.ItemComponents
{
    class ConsumableVR : MonoBehaviour
    {
        private Network_Player playerNetwork;

        private float consumeDelay;

        private void Start()
        {
            playerNetwork = GetComponentInParent<Network_Player>();
        }

        private void Update()
        {
            if (PlayerItemManager.IsBusy || CanvasHelper.ActiveMenu != MenuType.None) return;

            if (Vector3.Distance(VRRig.instance.DominantController.transform.position, VRRig.instance.Mouth.position) < 0.08f)
            {
                consumeDelay += Time.deltaTime;
            }
            else
            {
                consumeDelay = 0;
            }

            if (consumeDelay > 0.25f)
                ReflectionInfos.usableItemUse.Invoke(playerNetwork.PlayerItemManager.useItemController, null);
        }
    }
}
