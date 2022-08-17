// Based on https://github.com/Raicuparta/two-forks-vr/blob/main/TwoForksVr/src/LaserPointer/LaserInputModule.cs, an adaptation of https://github.com/googlearchive/tango-examples-unity/blob/master/TangoWithCardboardExperiments/Assets/Cardboard/Scripts/GazeInputModule.cs

using RaftVR.Inputs;
using RaftVR.Rig;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RaftVR.UI
{
    public class VRPointerInputModule : BaseInputModule
    {
        private const float rayDistance = 30f;
        private Vector3 lastHeadPose;
        private PointerEventData pointerData;
        private static LineRenderer pointerLine;

        protected override void Awake()
        {
            base.Awake();
            if (pointerLine) return;
            pointerLine = new GameObject("UI Pointer").AddComponent<LineRenderer>();
            DontDestroyOnLoad(pointerLine.gameObject);
            pointerLine.gameObject.layer = LayerMask.NameToLayer("UI");
            pointerLine.material = VRAssetsManager.lineMaterial;
            pointerLine.widthMultiplier = 0.007f;
            pointerLine.sortingOrder = 30000;
        }

        public override void DeactivateModule()
        {
            base.DeactivateModule();
            if (pointerData != null)
            {
                HandlePendingClick();
                HandlePointerExitAndEnter(pointerData, null);
                pointerData = null;
            }

            eventSystem.SetSelectedGameObject(null, GetBaseEventData());
        }

        public override bool IsPointerOverGameObject(int pointerId)
        {
            return pointerData != null && pointerData.pointerEnter != null;
        }

        public override void Process()
        {
            pointerLine.enabled = Cursor.visible;
            if (!Cursor.visible) return;
            pointerLine.SetPosition(0, VRRig.instance.DominantController.UIHand.position);
            pointerLine.SetPosition(1, VRRig.instance.DominantController.UIHand.rotation * Vector3.forward * 100);

            CastRay();
            UpdateCurrentObject();

            var pressed = false;
            var released = false;
            var pressing = false;

            VRInput.TryGetButtonDown("Click", out pressed);
            VRInput.TryGetButtonUp("Click", out released);
            VRInput.TryGetButton("Click", out pressing);

            if (!pressed && pressing)
                HandleDrag();
            else if (!pointerData.eligibleForClick && pressed)
                HandleTrigger();
            else if (released)
                HandlePendingClick();
        }

        private void CastRay()
        {
            var isHit = Physics.Raycast(
                VRRig.instance.DominantController.UIHand.position,
                VRRig.instance.DominantController.UIHand.forward,
                out var hit,
                rayDistance,
                LayerMask.GetMask("UI"));

            if (isHit)
                pointerLine.SetPosition(1, hit.point);

            Camera camera = VRRig.instance.uiCamera ? VRRig.instance.uiCamera : Camera.main;

            var pointerPosition = camera.WorldToScreenPoint(hit.point);

            if (pointerData == null)
            {
                pointerData = new PointerEventData(eventSystem);
                lastHeadPose = pointerPosition;
            }

            // Cast a ray into the scene
            pointerData.Reset();
            pointerData.position = pointerPosition;
            eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
            pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            m_RaycastResultCache.Clear();
            pointerData.delta = pointerPosition - lastHeadPose;
            lastHeadPose = hit.point;
        }

        private void UpdateCurrentObject()
        {
            // Send enter events and update the highlight.
            var go = pointerData.pointerCurrentRaycast.gameObject;
            HandlePointerExitAndEnter(pointerData, go);
        }

        private void HandleDrag()
        {
            var moving = pointerData.IsPointerMoving();

            if (moving && pointerData.pointerDrag != null && !pointerData.dragging)
            {
                ExecuteEvents.Execute(pointerData.pointerDrag, pointerData,
                    ExecuteEvents.beginDragHandler);
                pointerData.dragging = true;
            }

            if (!pointerData.dragging || !moving || pointerData.pointerDrag == null) return;

            ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler);
        }

        private void HandlePendingClick()
        {
            if (!pointerData.eligibleForClick) return;

            var go = pointerData.pointerCurrentRaycast.gameObject;

            // Send pointer up and click events.
            ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);

            if (pointerData.pointerDrag != null)
                ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.dropHandler);

            if (pointerData.pointerDrag != null && pointerData.dragging)
                ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.endDragHandler);

            // Clear the click state.
            pointerData.pointerPress = null;
            pointerData.rawPointerPress = null;
            pointerData.eligibleForClick = false;
            pointerData.clickCount = 0;
            pointerData.pointerDrag = null;
            pointerData.dragging = false;
        }

        private void HandleTrigger()
        {
            var go = pointerData.pointerCurrentRaycast.gameObject;

            // Send pointer down event.
            pointerData.pressPosition = pointerData.position;
            pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
            pointerData.pointerPress =
                ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.pointerDownHandler)
                ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);

            // Save the drag handler as well
            pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(go);
            if (pointerData.pointerDrag != null)
                ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.initializePotentialDrag);

            // Save the pending click state.
            pointerData.rawPointerPress = go;
            pointerData.eligibleForClick = true;
            pointerData.delta = Vector2.zero;
            pointerData.dragging = false;
            pointerData.useDragThreshold = true;
            pointerData.clickCount = 1;
            pointerData.clickTime = Time.unscaledTime;
        }
    }
}