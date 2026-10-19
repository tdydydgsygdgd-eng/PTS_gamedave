using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Details")]
public class ItemDetails : ScriptableObject
{
    public string ItemName = "Item";
    public Sprite ItemIcon;
    [Min(1)] public int Quantity = 1;
}
