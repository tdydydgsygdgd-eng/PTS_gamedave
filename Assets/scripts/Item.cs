using UnityEngine;

// Pasang komponen ini pada item selain koin, lalu pilih ItemDetails di Inspector.
public class Item : MonoBehaviour
{
    [SerializeField] private ItemDetails itemDetails;

    public ItemDetails Details => itemDetails;
}
