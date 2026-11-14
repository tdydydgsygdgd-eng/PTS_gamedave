using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TMP_Text quantityTextTMP;
    public Text quantityTextLegacy;
    public Button slotButton;

    private ItemData currentItem;
    private Action<ItemData> onSlotClicked;

    void Awake()
    {
        if (slotButton == null)
        {
            slotButton = GetComponent<Button>();
        }

        if (slotButton != null)
        {
            slotButton.onClick.AddListener(HandleClick);
        }
    }

    public void Setup(ItemData item, int quantity, Action<ItemData> onClick)
    {
        currentItem = item;
        onSlotClicked = onClick;

        if (currentItem != null)
        {
            if (iconImage != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = currentItem.icon;
            }

            string qText = quantity > 1 ? quantity.ToString() : "";
            if (quantityTextTMP != null) quantityTextTMP.text = qText;
            if (quantityTextLegacy != null) quantityTextLegacy.text = qText;

            if (slotButton != null) slotButton.interactable = true;
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        currentItem = null;
        onSlotClicked = null;

        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
            iconImage.sprite = null;
        }

        if (quantityTextTMP != null) quantityTextTMP.text = "";
        if (quantityTextLegacy != null) quantityTextLegacy.text = "";

        if (slotButton != null) slotButton.interactable = false;
    }

    private void HandleClick()
    {
        if (currentItem != null)
        {
            onSlotClicked?.Invoke(currentItem);
        }
    }
}
