using UnityEngine;

public enum ItemType
{
    Consumable,
    Valuable,
    Material,
    Weapon,
    Other
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "TinyRPG/Item Data")]
public class ItemData : ScriptableObject
{
    public string id;
    public string itemName = "Nama Item";
    public ItemType itemType = ItemType.Consumable;
    public Sprite icon;
    [TextArea(3, 6)]
    public string description = "Deskripsi item ini.";
    public int value = 25; // Misal: HP jika consumable, atau Gold jika valuable
}
