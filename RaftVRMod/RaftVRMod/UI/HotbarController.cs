using UnityEngine;

namespace RaftVR.UI
{
    class HotbarController : MonoBehaviour
    {
        public static HotbarController instance { get; private set; }

        private const float AMT_TIME_VISIBLE = 1f;

        private float visibilityTimer = 0f;
        private Canvas handCanvas;
        private Transform originalParent;
        private Transform hotbar;

        private CanvasHelper canvasHelper;

        private void Awake()
        {
            instance = this;
            handCanvas = GetComponent<Canvas>();
        }

        private void Update()
        {
            if (!canvasHelper)
            {
                canvasHelper = ComponentManager<CanvasHelper>.Value;

                if (!canvasHelper) return;
            }

            bool inventoryOpen = canvasHelper.GetMenu(MenuType.Inventory).IsOpen;

            if (inventoryOpen)
            {
                visibilityTimer = 0f;

                if (hotbar.parent != originalParent)
                    MoveToOriginalParent();
            }
            else
            {
                if (hotbar.parent != transform)
                    MoveToHand();

                bool visible = visibilityTimer > 0f;

                if (visible)
                    visibilityTimer -= Time.unscaledDeltaTime;

                if (!handCanvas) return;

                if (handCanvas.enabled != visible)
                    handCanvas.enabled = visible;
            }
        }

        public void Init(Transform hotbar, Camera uiCamera)
        {
            originalParent = hotbar.transform.parent;
            this.hotbar = hotbar;
            handCanvas.worldCamera = uiCamera;
        }

        public void RefreshVisibility()
        {
            visibilityTimer = AMT_TIME_VISIBLE;
        }

        public void MoveToOriginalParent()
        {
            if (!hotbar) return;

            hotbar.SetParent(originalParent);
            hotbar.localPosition = new Vector3(0, -540, 0);
            hotbar.localRotation = Quaternion.identity;
            hotbar.localScale = Vector3.one;
        }

        public void MoveToHand()
        {
            if (!hotbar) return;

            hotbar.SetParent(transform);
            hotbar.localPosition = Vector3.zero;
            hotbar.localRotation = Quaternion.identity;
            hotbar.localScale = Vector3.one;
        }
    }
}
