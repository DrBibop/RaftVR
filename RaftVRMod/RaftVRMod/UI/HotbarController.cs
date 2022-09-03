using RaftVR.Configs;
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
        private float radialButtonTimer = 0f;

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

            if (CanvasHelper.ActiveMenu != MenuType.None)
            {
                if (RadialHotbar.instance.gameObject.activeSelf)
                    RadialHotbar.instance.gameObject.SetActive(false);
            }
            else
            {
                if (MyInput.GetButton("NextItem") || MyInput.GetButton("PrevItem"))
                    radialButtonTimer += Time.deltaTime;
                else
                    radialButtonTimer = 0;

                bool radialHotbarVisible = radialButtonTimer > (VRConfigs.UseRadialHotbar == VRConfigs.RadialHotbarMode.Always ? 0f : 0.5f) || MyInput.GetButton("RadialHotbar");

                if (RadialHotbar.instance.gameObject.activeSelf != radialHotbarVisible)
                    RadialHotbar.instance.gameObject.SetActive(radialHotbarVisible);

                if (radialHotbarVisible)
                    visibilityTimer = 0f;
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

                bool hotbarVisible = visibilityTimer > 0f  && radialButtonTimer < 0.5f;

                if (hotbarVisible)
                    visibilityTimer -= Time.unscaledDeltaTime;

                if (!handCanvas) return;

                if (handCanvas.enabled != hotbarVisible)
                    handCanvas.enabled = hotbarVisible;
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
