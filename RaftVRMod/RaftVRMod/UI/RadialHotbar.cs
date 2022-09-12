using RaftVR.Rig;
using System.Collections.Generic;
using UnityEngine;

namespace RaftVR.UI
{
    class RadialHotbar : MonoBehaviour
    {
        internal static RadialHotbar instance;

        private List<RadialSlot> allSlots = new List<RadialSlot>();
        private List<RadialSlot> availableSlots = new List<RadialSlot>();
        private Transform selectionOutline;
        private Transform cursor;
        private Hotbar hotbar;
        private RadialSlot selectedSlot;
        private int slotCount;

        private void Awake()
        {
            instance = this;
            cursor = transform.Find("Cursor");
        }

        internal void Init(Transform outline, Hotbar hotbar)
        {
            selectionOutline = outline;
            this.hotbar = hotbar;
        }

        internal void AddSlot(RadialSlot slot)
        {
            allSlots.Add(slot);
        }

        private void OnEnable()
        {
            if (!hotbar || !VRRig.instance.uiCamera) return;

            availableSlots.Clear();
            slotCount = 0;

            foreach (RadialSlot slot in allSlots)
            {
                if (slot.IsEmpty)
                {
                    slot.gameObject.SetActive(false);
                    slot.transform.localPosition = Vector3.zero;
                    continue;
                }

                availableSlots.Add(slot);
                slotCount++;
            }

            int currentSlot = 0;
            foreach (RadialSlot slot in availableSlots)
            {
                slot.gameObject.SetActive(true);

                float slotAngle = (float)currentSlot / (float)slotCount * 2f * Mathf.PI;

                slot.transform.localPosition = new Vector3(Mathf.Sin(slotAngle), Mathf.Cos(slotAngle), 0f) * 250;

                currentSlot++;
            }

            transform.position = VRRig.instance.RightController.UIHand.transform.position;
            transform.rotation = Quaternion.LookRotation(VRRig.instance.RightController.UIHand.position - VRRig.instance.uiCamera.transform.position);

            selectionOutline.localPosition = selectedSlot ? selectedSlot.transform.localPosition : Vector3.zero;
        }

        private void Update()
        {
            Plane plane = new Plane(transform.forward, transform.position);
            Vector3 selectionPos = plane.ClosestPointOnPlane(VRRig.instance.RightController.UIHand.position);

            cursor.position = selectionPos;

            if ((selectionPos - transform.position).magnitude < 0.03f) return;

            float shortestDistance = float.MaxValue;
            RadialSlot newSlot = null;

            foreach (RadialSlot slot in availableSlots)
            {
                float distance = Vector3.Distance(selectionPos, slot.transform.position);

                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    newSlot = slot;
                }
            }

            if (selectedSlot != newSlot)
            {
                selectedSlot = newSlot;
                selectionOutline.localPosition = selectedSlot.transform.localPosition;
                hotbar.SetSelectedSlotIndex(allSlots.IndexOf(selectedSlot));
                hotbar.SelectHotslot(selectedSlot.slot);
            }
        }

        internal void SetSelectedIndex(int index)
        {
            selectedSlot = allSlots[index];
        }
    }
}
