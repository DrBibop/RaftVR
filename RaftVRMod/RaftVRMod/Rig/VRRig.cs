using RaftVR.Configs;
using RaftVR.UI;
using RaftVR.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;
using UnityEngine.XR;

namespace RaftVR.Rig
{
    public class VRRig : MonoBehaviour
    {
        public static VRRig instance { get; private set; }

        private Camera _camera;

        private bool canSnapTurn = true;

        public Camera camera {
            get => _camera;
            set
            {
                _camera = value;

                if (!cameraHolder)
                    cameraHolder = new GameObject("VR Camera Holder").transform;

                _camera.transform.SetParent(cameraHolder);
                XRDevice.DisableAutoXRCameraTracking(_camera, true);

                //Don't ask me why. It just works.
                StartCoroutine(FixCameraStuffOneFrameLater());

                TrackedPoseDriver headPoseDriver = _camera.gameObject.AddComponent<TrackedPoseDriver>();
                headPoseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.Head);
                headPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
                headPoseDriver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;

                if (_camera.cullingMask == (_camera.cullingMask | (1 << LayerMask.NameToLayer("UI"))))
                    _camera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));

                if (_camera.cullingMask != (_camera.cullingMask | (1 << LayerMask.NameToLayer("HandCamera"))))
                    _camera.cullingMask |= (1 << LayerMask.NameToLayer("HandCamera"));

                typeof(Helper).GetField("localFovCamera", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(null, _camera);
            }
        }

        private Camera _uiCamera;

        public Camera uiCamera {
            get => _uiCamera;
            set
            {
                _uiCamera = value;

                _uiCamera.transform.SetParent(null);
                XRDevice.DisableAutoXRCameraTracking(_uiCamera, true);

                TrackedPoseDriver headPoseDriver = _uiCamera.gameObject.AddComponent<TrackedPoseDriver>();
                headPoseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.Head);
                headPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
                headPoseDriver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;

                _uiCamera.cullingMask = LayerMask.GetMask("UI");

                if (UI.HotbarController.instance)
                    UI.HotbarController.instance.GetComponent<Canvas>().worldCamera = _uiCamera;

                UI.UIHelper.RefreshPermanentCanvases(value);
            }
        }

        public Transform player { get; set; }

        private Transform _positionTarget;
        public Transform positionTarget {
            get => _positionTarget;
            set
            {
                _positionTarget = value;
                origTargetPos = _positionTarget.localPosition;
            }
        }

        public Hand LeftController { get; private set; }
        public Hand RightController { get; private set; }

        public Hand DominantController => VRConfigs.IsLeftHanded ? LeftController : RightController;
        public Hand NonDominantController => VRConfigs.IsLeftHanded ? RightController : LeftController;

        public Transform Head { get; private set; }
        public Transform Mouth { get; private set; }

        public Transform HeadIKTarget { get; private set; }
        public Transform LeftHandIKTarget { get; private set; }
        public Transform RightHandIKTarget { get; private set; }

        private GameObject playspaceCenterIndicator;

        private Material centerIndicatorMat;

        private Transform canvasHolder;

        private Quaternion canvasHolderRotation = Quaternion.identity;

        private bool canTurn = true;

        private Transform cameraHolder;

        private Vector3 origTargetPos;

        private bool updatedHands = false;

        internal List<Canvas> worldCanvases = new List<Canvas>();

        private GameObject interactionRay;

        private bool interactionRayShown;

        private IEnumerator FixCameraStuffOneFrameLater()
        {
            yield return null;

            foreach (Camera childCam in camera.gameObject.GetComponentsInChildren<Camera>(true))
            {
                if (childCam == camera || childCam == uiCamera) continue;

                childCam.gameObject.AddComponent<HeadTrackingRemover>();
            }
            Animator anim = camera.GetComponent<Animator>();

            if (anim)
                anim.enabled = false;
        }

        private void Awake()
        {
            instance = this;

            //To be honest, I should just make a prefab for all this...
            LeftController = new GameObject("Left Controller").AddComponent<Hand>();
            LeftController.transform.SetParent(transform);
            LeftController.Init(XRNode.LeftHand);

            LeftHandIKTarget = new GameObject("Left Hand IK Target").transform;
            LeftHandIKTarget.SetParent(LeftController.transform);
            LeftHandIKTarget.localPosition = new Vector3(-0.02218593f, -0.0299f, -0.1200768f);
            LeftHandIKTarget.localRotation = Quaternion.Euler(-1.69f, 87.37801f, 7.029f);

            RightController = new GameObject("Right Controller").AddComponent<Hand>();
            RightController.transform.SetParent(transform);
            RightController.Init(XRNode.RightHand);

            RightHandIKTarget = new GameObject("Right Hand IK Target").transform;
            RightHandIKTarget.SetParent(RightController.transform);
            RightHandIKTarget.localPosition = new Vector3(0.02218593f, -0.0299f, -0.1200768f);
            RightHandIKTarget.localRotation = Quaternion.Euler(-1.69f, -87.37801f, 172.971f);

            Head = new GameObject("Head").transform;
            Head.SetParent(transform);

            Mouth = new GameObject("Mouth").transform;
            Mouth.SetParent(Head);
            Mouth.localPosition = new Vector3(0f, -0.08f, 0.05f);
            Mouth.localRotation = Quaternion.identity;

            TrackedPoseDriver headPoseDriver = Head.gameObject.AddComponent<TrackedPoseDriver>();
            headPoseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.Head);
            headPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            headPoseDriver.updateType = TrackedPoseDriver.UpdateType.Update;

            HeadIKTarget = new GameObject("Head IK Target").transform;
            HeadIKTarget.SetParent(Head);
            HeadIKTarget.localPosition = new Vector3(0, -0.07801461f, -0.1143799f);
            HeadIKTarget.localRotation = Quaternion.Euler(1.451f, 0, -90);

            playspaceCenterIndicator = Instantiate(VRAssetsManager.playspaceCenterIndicatorPrefab);
            DontDestroyOnLoad(playspaceCenterIndicator);

            centerIndicatorMat = playspaceCenterIndicator.GetComponentInChildren<MeshRenderer>().material;

            interactionRay = Instantiate(VRAssetsManager.interactionRayPrefab, RightController.transform);
            interactionRay.SetActive(false);
        }

        public void AddCameraCanvas(Canvas canvas)
        {
            MoveCanvasToWorld(canvas, false);

            if (!canvasHolder)
            {
                canvasHolder = new GameObject("Camera Canvas Holder").transform;
                canvasHolder.SetParent(uiCamera.transform);
                canvasHolder.ResetTransform();
            }

            canvas.transform.SetParent(canvasHolder);
            canvas.transform.localPosition = new Vector3(0, 0, 1);
            canvas.transform.localRotation = Quaternion.identity;
            canvas.transform.localScale = Vector3.one * 0.0006f;
        }

        public void AddWorldCanvas(Canvas canvas, Quaternion fixedRotation)
        {
            if (canvas.renderMode != RenderMode.WorldSpace)
                MoveCanvasToWorld(canvas, true);

            worldCanvases.RemoveAll(x => x == null);
            if (!worldCanvases.Contains(canvas)) worldCanvases.Add(canvas);

            canvas.transform.SetParent(null);
            canvas.transform.localPosition = fixedRotation * new Vector3(0, VRConfigs.SeatedMode ? 1.2f : 1.6f, 1);
            canvas.transform.localRotation = fixedRotation;
            canvas.transform.localScale = Vector3.one * 0.0006f;
        }

        private void MoveCanvasToWorld(Canvas canvas, bool addCollider)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = uiCamera;
            (canvas.transform as RectTransform).sizeDelta = new Vector2(1920, 1080);
            (canvas.transform as RectTransform).pivot = new Vector2(0.5f, 0.5f);
            canvas.gameObject.layer = LayerMask.NameToLayer("UI");

            if (addCollider)
            {
                BoxCollider uiCollider = canvas.gameObject.AddComponent<BoxCollider>();
                uiCollider.size = new Vector3(1920, 1080, 1);

                canvas.gameObject.AddComponent<UI.UICollider>().Init(canvas);
            }
        }

        internal void UpdateWorldCanvasesPosition()
        {
            worldCanvases.RemoveAll(x => x == null);

            foreach (Canvas canvas in worldCanvases)
            {
                Vector3 pos = canvas.transform.localPosition;
                pos.y = VRConfigs.SeatedMode ? 1.2f : 1.6f;
                canvas.transform.localPosition = pos;
            }
        }

        internal void ShowInteractionRay()
        {
            if (interactionRayShown || !VRConfigs.ShowInteractionRay || CanvasHelper.ActiveMenu != MenuType.None) return;

            interactionRayShown = true;

            if (!interactionRay.activeSelf)
                interactionRay.SetActive(true);
        }

        internal void InitHUD(Network_Player player)
        {
            GameObject canvasesObject = GameObject.Find("Canvases");

            if (canvasesObject)
            {
                GameObject rightHandCanvas = Instantiate(VRAssetsManager.handCanvasPrefab, DominantController.UIWorldHand);
                rightHandCanvas.transform.localPosition = new Vector3(0f, 0.05f, 0f);
                rightHandCanvas.transform.localRotation = Quaternion.Euler(15, 20, 0);
                rightHandCanvas.transform.localScale = Vector3.one * 0.0003f;
                rightHandCanvas.name = "Right Hand Canvas";

                Transform hotbar = canvasesObject.transform.Find("_CanvasGame_New/InventoryParent/Hotbar");

                if (hotbar)
                {
                    GameObject hotbarCanvas = Instantiate(VRAssetsManager.handCanvasPrefab, DominantController.UIWorldHand);
                    hotbarCanvas.transform.localPosition = new Vector3(0f, 0.05f, 0f);
                    hotbarCanvas.transform.localRotation = Quaternion.Euler(15, 20, 0);
                    hotbarCanvas.transform.localScale = Vector3.one * 0.0003f;
                    hotbarCanvas.AddComponent<HotbarController>();
                    hotbarCanvas.name = "Hotbar Canvas";

                    HotbarController.instance.Init(hotbar, uiCamera);

                    Transform hotslot = hotbar.Find("Hotslot parent/Slot_Hotbar");

                    if (hotslot)
                    {
                        RadialHotbar radialHotbar = Instantiate(VRAssetsManager.radialHotbarCanvasPrefab).AddComponent<RadialHotbar>();
                        radialHotbar.GetComponent<Canvas>().worldCamera = uiCamera;

                        for(int i = 0; i < 10; i++)
                        {
                            GameObject hotslotInstance = Instantiate(hotslot.gameObject, radialHotbar.transform);
                            hotslotInstance.transform.ResetTransform();
                            (hotslotInstance.transform as RectTransform).pivot = new Vector2(0.5f, 0.5f);
                            Destroy(hotslotInstance.GetComponent<Slot>());

                            RadialSlot slot = hotslotInstance.AddComponent<RadialSlot>();
                            slot.slot = player.Inventory.GetSlot(i);
                            radialHotbar.AddSlot(slot);
                        }

                        Transform outline = Instantiate(hotbar.Find("Hotslot parent/Hotbar selection").gameObject, radialHotbar.transform).transform;
                        outline.ResetTransform();
                        (outline as RectTransform).pivot = new Vector2(0.5f, 0.5f);

                        radialHotbar.Init(outline, hotbar.GetComponent<Hotbar>());

                        radialHotbar.gameObject.SetActive(false);
                    }
                }

                Transform crosshair = canvasesObject.transform.Find("_CanvasGame_New/Aim");

                if (crosshair)
                {
                    crosshair.gameObject.SetActive(false);
                }

                Transform statSliders = canvasesObject.transform.Find("_CanvasGame_New/Stat sliders");

                if (statSliders)
                {
                    GameObject statusBarsCanvas = Instantiate(VRAssetsManager.handCanvasPrefab, NonDominantController.UIWorldHand);
                    statusBarsCanvas.transform.localPosition = new Vector3(-0.07f, -0.05f, -0.05f);
                    statusBarsCanvas.transform.localRotation = Quaternion.Euler(180, -90, 0);
                    statusBarsCanvas.transform.localScale = Vector3.one * 0.0003f;
                    statusBarsCanvas.AddComponent<UI.StatusBarsController>();
                    statusBarsCanvas.name = "Status Bars";

                    statSliders.SetParent(statusBarsCanvas.transform);
                    (statSliders as RectTransform).pivot = new Vector2(0.5f, 0.4f);
                    statSliders.ResetTransform();
                }

                Transform bindText1 = canvasesObject.transform.Find("_CanvasGame_New/DisplayTextManager");
                Transform bindText2 = canvasesObject.transform.Find("_CanvasGame_New/DisplayTextBottom");

                if (bindText1 && bindText2)
                {
                    bindText1.SetParent(rightHandCanvas.transform);
                    bindText1.localPosition = new Vector3(0, 150, 0);
                    bindText1.localRotation = Quaternion.identity;
                    bindText1.localScale = Vector3.one;

                    bindText2.SetParent(rightHandCanvas.transform);
                    bindText2.localPosition = new Vector3(0, 150, 0);
                    bindText2.localRotation = Quaternion.identity;
                    bindText2.localScale = Vector3.one;
                }

                Transform pickupNotif = canvasesObject.transform.Find("_CanvasGame_New/InventoryPickup");

                if (pickupNotif)
                {
                    pickupNotif.SetParent(rightHandCanvas.transform);
                    pickupNotif.localPosition = new Vector3(0, -150, 0);
                    pickupNotif.localRotation = Quaternion.identity;
                    pickupNotif.localScale = Vector3.one;
                }

                Transform screenEffects = canvasesObject.transform.Find("_CanvasGame_New/ScreenEffects");

                if (screenEffects)
                {
                    Canvas screenEffectsCanvas = Instantiate(VRAssetsManager.handCanvasPrefab).GetComponent<Canvas>();

                    screenEffectsCanvas.transform.localScale = Vector3.one;
                    (screenEffectsCanvas.transform as RectTransform).pivot = new Vector2(0.5f, 0.5f);
                    screenEffectsCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                    screenEffectsCanvas.planeDistance = 0.3f;
                    screenEffectsCanvas.worldCamera = uiCamera;

                    screenEffects.transform.SetParent(screenEffectsCanvas.transform);
                    screenEffects.transform.ResetTransform();
                }

                Transform oxygenMeter = canvasesObject.transform.Find("_CanvasGame_New/OxygenMeter");
                Transform notificationManager = canvasesObject.transform.Find("_CanvasGame_New/NotificationManager");
                Transform saveIcon = canvasesObject.transform.Find("_CanvasGame_New/SaveGameIcon");

                if (oxygenMeter && notificationManager && saveIcon)
                {
                    Canvas cameraCanvas = Instantiate(VRAssetsManager.handCanvasPrefab).GetComponent<Canvas>();
                    cameraCanvas.gameObject.name = "Camera Canvas";

                    (cameraCanvas.transform as RectTransform).pivot = new Vector2(0.5f, 0.5f);
                    cameraCanvas.transform.ResetTransform();
                    oxygenMeter.SetParent(cameraCanvas.transform);
                    oxygenMeter.ResetTransform();
                    (oxygenMeter as RectTransform).anchoredPosition = Vector2.zero;

                    notificationManager.SetParent(cameraCanvas.transform);
                    notificationManager.ResetTransform();

                    foreach (Transform child in notificationManager)
                    {
                        child.transform.localPosition = new Vector3(390, -700, 0);
                    }

                    saveIcon.SetParent(cameraCanvas.transform);
                    saveIcon.ResetTransform();
                    saveIcon.localPosition = new Vector3(900, -500, 0);

                    AddCameraCanvas(cameraCanvas);
                }

                Transform cutsceneCanvasObject = canvasesObject.transform.Find("_CanvasCutscene");

                if (cutsceneCanvasObject)
                {
                    cutsceneCanvasObject.gameObject.AddComponent<RectMask2D>();

                    Transform dialogBox = cutsceneCanvasObject.Find("Dialogue_UI");
                    dialogBox.localScale = Vector3.one * 1.5f;

                    AddCameraCanvas(cutsceneCanvasObject.GetComponent<Canvas>());
                }

                Transform loadCircle = canvasesObject.transform.Find("_CanvasGame_New/LoadCircle");

                if (loadCircle)
                {
                    loadCircle.SetParent(rightHandCanvas.transform);
                    loadCircle.ResetTransform();
                    loadCircle.localPosition = new Vector3(0, 50, 0);
                }

                Transform costCursor = canvasesObject.transform.Find("_CanvasGame_New/CostCollectionCursor");

                if (costCursor)
                {
                    costCursor.SetParent(rightHandCanvas.transform);

                    costCursor.localPosition = new Vector3(0, 400, 0);
                    costCursor.localRotation = Quaternion.identity;
                    costCursor.localScale = Vector3.one;
                }

                Transform craftMenu = canvasesObject.transform.Find("_CanvasGame_New/Crafting");

                if (craftMenu)
                {
                    RectTransform craftMenuRect = craftMenu as RectTransform;

                    craftMenuRect.anchorMin = craftMenuRect.anchorMax = new Vector2(0.5f, 1f);
                    craftMenuRect.anchoredPosition = new Vector3(218, -179, 0);
                    craftMenuRect.offsetMin = new Vector2(193, -204);
                    craftMenuRect.offsetMax = new Vector2(243, -154);
                }

                Transform researchMenu = canvasesObject.transform.Find("_CanvasGame_New/InventoryParent/Inventory_ResearchTable");

                if (researchMenu)
                {
                    RectTransform researchMenuRect = researchMenu as RectTransform;

                    researchMenuRect.pivot = Vector2.one;
                    researchMenuRect.localPosition = new Vector3(-192, 344, 0);
                }

                canvasesObject.layer = LayerMask.NameToLayer("UI");
                BoxCollider uiCollider = canvasesObject.AddComponent<BoxCollider>();
                uiCollider.size = new Vector3(1920, 1080, 1);
                uiCollider.center = Vector3.zero;

                Canvas[] canvases = canvasesObject.GetComponentsInChildren<Canvas>();

                for (int i = 0; i < canvases.Length; i++)
                {
                    Canvas canvas = canvases[i];

                    canvas.renderMode = RenderMode.WorldSpace;
                    canvas.worldCamera = uiCamera;
                    (canvas.transform as RectTransform).sizeDelta = new Vector2(1920, 1080);
                    canvas.transform.ResetTransform();
                }

                canvasesObject.transform.SetParent(NonDominantController.UIWorldHand);
                canvasesObject.transform.localPosition = new Vector3(0, 0.2f, 0.1f);
                canvasesObject.transform.localRotation = Quaternion.Euler(15, -20, 0);
                canvasesObject.transform.localScale = Vector3.one * 0.0003f;
            }
        }

        public void SetCanTurn(bool canTurn)
        {
            this.canTurn = canTurn;
        }

        public void SetVerticalOffset(float offset)
        {
            if (!positionTarget) return;
            if (VRConfigs.SeatedMode) offset += 0.5f;
            positionTarget.localPosition = origTargetPos + (Vector3.up * offset);
        }

        private void Update()
        {
            UpdateHands();

            switch (VRConfigs.ShowPlayspaceCenter)
            {
                case VRConfigs.PlayspaceCenterDisplay.Always:
                    if (!playspaceCenterIndicator.activeSelf) playspaceCenterIndicator.SetActive(true);
                    break;
                case VRConfigs.PlayspaceCenterDisplay.Never:
                    if (playspaceCenterIndicator.activeSelf) playspaceCenterIndicator.SetActive(false);
                    break;
                default:
                    if (!playspaceCenterIndicator.activeSelf) playspaceCenterIndicator.SetActive(true);
                    float alpha = (new Vector2(uiCamera.transform.position.x, uiCamera.transform.position.z).magnitude - 0.3f) * 3;
                    centerIndicatorMat.color = new Color(1, 1, 1, Mathf.Clamp(alpha, 0, 1));
                    break;
            }
        }

        internal void UpdateHands()
        {
            if (updatedHands) return;

            DominantController.UpdateHand();
            NonDominantController.UpdateHand();

            updatedHands = true;
        }

        private void LateUpdate()
        {
            updatedHands = false;

            if (positionTarget)
            {
                transform.position = positionTarget.position;
            }
            if (player)
            {
                if (canTurn)
                {
                    if (Inputs.VRInput.TryGetAxis("Turn", out float turnAxis) && Mathf.Abs(turnAxis) > 0.8f)
                    {
                        Vector3 rigAngles = transform.eulerAngles;

                        if (VRConfigs.SnapTurn)
                        {
                            if (canSnapTurn)
                            {
                                rigAngles.y += Mathf.Sign(turnAxis) * VRConfigs.SnapTurnAngle;
                                canSnapTurn = false;
                            }
                        }
                        else
                        {
                            rigAngles.y += turnAxis * VRConfigs.SmoothTurnSpeed * Time.deltaTime;
                        }

                        transform.eulerAngles = rigAngles;
                    }
                    else if (VRConfigs.SnapTurn)
                    {
                        canSnapTurn = true;
                    }
                }

                Vector3 playerAngles = player.eulerAngles;
                playerAngles.y = (VRConfigs.MoveDirectionOrigin == VRConfigs.DirectionOriginType.Head ? Head : LeftController.transform).eulerAngles.y;
                player.eulerAngles = playerAngles;
            }

            if (cameraHolder)
            {
                cameraHolder.position = transform.position;
                cameraHolder.rotation = transform.rotation;
            }

            if (canvasHolder && uiCamera)
            {
                Quaternion deriv = new Quaternion();
                canvasHolderRotation = canvasHolderRotation.SmoothDamp(uiCamera.transform.rotation, ref deriv, 0.07f);
                canvasHolder.rotation = Quaternion.LookRotation(canvasHolderRotation * Vector3.forward, uiCamera.transform.up);
            }

            if (!interactionRayShown)
            {
                if (interactionRay.activeSelf)
                    interactionRay.SetActive(false);
            }

            interactionRayShown = false;
        }
    }
}
