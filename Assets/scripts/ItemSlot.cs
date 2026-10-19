using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public const int MaxStackSize = 99;

    public string itemName;
    public int itemQuantity;
    public Sprite itemIcon;
    public bool isFull;

    private ItemDetails itemDetails;
    private Image itemImage;
    private Text quantityText;

    public void Initialize()
    {
        if (itemImage == null)
        {
            GameObject iconObject = new GameObject("Item Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(transform, false);

            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.12f, 0.12f);
            iconRect.anchorMax = new Vector2(0.88f, 0.88f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            itemImage = iconObject.GetComponent<Image>();
            itemImage.preserveAspect = true;
            itemImage.raycastTarget = false;
        }

        if (quantityText == null)
        {
            GameObject quantityObject = new GameObject("Quantity", typeof(RectTransform), typeof(Text));
            quantityObject.transform.SetParent(transform, false);

            RectTransform textRect = quantityObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0.02f);
            textRect.anchorMax = new Vector2(0.95f, 0.35f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            quantityText = quantityObject.GetComponent<Text>();
            quantityText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            quantityText.fontSize = 18;
            quantityText.fontStyle = FontStyle.Bold;
            quantityText.alignment = TextAnchor.LowerRight;
            quantityText.color = Color.white;
            quantityText.raycastTarget = false;
            quantityText.enabled = false;
        }

        if (!isFull)
            Clear();
    }

    public bool CanStack(ItemDetails item)
    {
        return isFull && itemDetails == item && itemQuantity < MaxStackSize;
    }

    public int AddToStack(ItemDetails item, int amount)
    {
        if (!CanStack(item) || amount <= 0)
            return 0;

        int added = Mathf.Min(amount, MaxStackSize - itemQuantity);
        itemQuantity += added;
        UpdateVisuals();
        return added;
    }

    public void SetItem(ItemDetails item, int quantity)
    {
        if (item == null || quantity <= 0)
            return;

        itemDetails = item;
        itemName = item.ItemName;
        itemQuantity = Mathf.Clamp(quantity, 1, MaxStackSize);
        itemIcon = item.ItemIcon;
        isFull = true;
        UpdateVisuals();
    }

    public void AddItem(ItemDetails item)
    {
        SetItem(item, item != null ? item.Quantity : 0);
    }

    private void UpdateVisuals()
    {
        if (itemImage == null || quantityText == null)
            Initialize();

        itemImage.sprite = itemIcon;
        itemImage.enabled = itemIcon != null;
        quantityText.text = itemQuantity > 1 ? itemQuantity.ToString() : string.Empty;
        quantityText.enabled = itemQuantity > 1;
    }

    private void Clear()
    {
        itemDetails = null;
        itemName = string.Empty;
        itemQuantity = 0;
        itemIcon = null;
        isFull = false;

        if (itemImage != null)
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
        }

        if (quantityText != null)
        {
            quantityText.text = string.Empty;
            quantityText.enabled = false;
        }
    }
}
