using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RaftVR.UI
{
    class RadialSlot : MonoBehaviour
    {
        internal bool IsEmpty => slot == null || slot.IsEmpty;

        internal Slot slot;

        private Image itemImage;

        private Text amountText;

        private Slider durabilitySlider;

        private void Awake()
        {
            itemImage = transform.Find("ItemImage").GetComponent<Image>();
            amountText = GetComponentInChildren<Text>();
            durabilitySlider = GetComponentInChildren<Slider>(true);
            itemImage.enabled = true;

            RectTransform bgTransform = transform as RectTransform;

            bgTransform.offsetMin = Vector2.zero;
            bgTransform.offsetMax = Vector2.zero;
            bgTransform.sizeDelta = new Vector2(70, 70);
        }

        private void OnEnable()
        {
            if (!slot) return;

            itemImage.sprite = slot.imageComponent.sprite;
            amountText.text = slot.textComponent.text;
            durabilitySlider.value = slot.sliderComponent.value;

            if (durabilitySlider.gameObject.activeSelf != slot.sliderComponent.gameObject.activeSelf)
                durabilitySlider.gameObject.SetActive(slot.sliderComponent.gameObject.activeSelf);
        }
    }
}
