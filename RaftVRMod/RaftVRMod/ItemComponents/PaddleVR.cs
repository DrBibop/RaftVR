using RaftVR.Utils;
using UnityEngine;

namespace RaftVR.ItemComponents
{
    class PaddleVR : MonoBehaviour
    {
        Network_Player playerNetwork;
        Transform paddlePoint;
        WaterPointGetter waterPointGetter;

        private void Start()
        {
            playerNetwork = GetComponentInParent<Network_Player>();

            paddlePoint = new GameObject("Paddle Point").transform;
            paddlePoint.SetParent(transform);
            paddlePoint.localPosition = new Vector3(0f, 0.8f, 0f);

            waterPointGetter = GetComponent<WaterPointGetter>();
        }

        private void Update()
        {
            if (PlayerItemManager.IsBusy || CanvasHelper.ActiveMenu != MenuType.None) return;

            if (paddlePoint.position.y < waterPointGetter.GetWaterPoint(paddlePoint.position))
                ReflectionInfos.usableItemUse.Invoke(playerNetwork.PlayerItemManager.useItemController, null);
        }
    }
}
