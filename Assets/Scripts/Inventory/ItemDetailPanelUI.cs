using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDetailPanelUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelRoot;
    public Image iconImage;
    public TMP_Text nameTextTMP;
    public Text nameTextLegacy;
    public TMP_Text typeTextTMP;
    public Text typeTextLegacy;
    public TMP_Text descriptionTextTMP;
    public Text descriptionTextLegacy;
    public TMP_Text valueTextTMP;
    public Text valueTextLegacy;
    public Button useButton;
    public TMP_Text useButtonTextTMP;
    public Text useButtonTextLegacy;
    public Button closeButton;

    private ItemData currentItem;

    void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject;

        if (useButton != null)
        {
            useButton.onClick.AddListener(OnUseButtonClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideDetail);
        }
    }

    public void ShowDetail(ItemData item)
    {
        currentItem = item;
        if (currentItem == null)
        {
            HideDetail();
            return;
        }

        if (panelRoot != null) panelRoot.SetActive(true);

        if (iconImage != null)
        {
            iconImage.sprite = currentItem.icon;
            iconImage.gameObject.SetActive(currentItem.icon != null);
        }

        SetText(nameTextTMP, nameTextLegacy, currentItem.itemName);
        SetText(typeTextTMP, typeTextLegacy, $"Tipe: {GetItemTypeString(currentItem.itemType)}");
        SetText(descriptionTextTMP, descriptionTextLegacy, currentItem.description);

        string valueInfo = currentItem.itemType == ItemType.Consumable 
            ? $"Efek: +{currentItem.value} HP" 
            : $"Nilai: {currentItem.value} Gold";
        SetText(valueTextTMP, valueTextLegacy, valueInfo);

        // Konfigurasi tombol Gunakan
        if (useButton != null)
        {
            bool isConsumable = currentItem.itemType == ItemType.Consumable;
            useButton.interactable = isConsumable;
            string btnText = isConsumable ? "Gunakan (Pakai)" : "Tidak Dapat Dipakai";
            SetText(useButtonTextTMP, useButtonTextLegacy, btnText);
        }
    }

    public void HideDetail()
    {
        currentItem = null;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void OnUseButtonClicked()
    {
        if (currentItem != null && InventoryManager.Instance != null)
        {
            ItemData toUse = currentItem;
            InventoryManager.Instance.UseItem(toUse);

            // Periksa apakah item masih ada di inventory setelah dipakai
            InventorySlot slot = InventoryManager.Instance.slots.Find(s => s.item == toUse);
            if (slot != null && slot.quantity > 0)
            {
                ShowDetail(toUse);
            }
            else
            {
                HideDetail();
            }
        }
    }

    private void SetText(TMP_Text tmp, Text legacy, string content)
    {
        if (tmp != null) tmp.text = content;
        if (legacy != null) legacy.text = content;
    }

    private string GetItemTypeString(ItemType type)
    {
        switch (type)
        {
            case ItemType.Consumable: return "Ramuan / Konsumsi";
            case ItemType.Valuable: return "Barang Berharga";
            case ItemType.Material: return "Material / Kristal";
            case ItemType.Weapon: return "Senjata / Amunisi";
            default: return "Lainnya";
        }
    }
}
