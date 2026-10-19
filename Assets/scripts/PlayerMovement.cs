using UnityEngine;
using UnityEngine.InputSystem; 
public class PlayerMovement : MonoBehaviour
{
    public float kecepatan = 5f;
    private Vector2 arahGerak; 
    private int skor = 0;     
    void OnMove(InputValue value)
    {
        
        arahGerak = value.Get<Vector2>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        bool isCoin = other.CompareTag("Coin");
        Item item = other.GetComponent<Item>();
        if (!isCoin && item == null)
            return;

        InventoryManager inventory = FindFirstObjectByType<InventoryManager>();
        if (inventory == null)
        {
            Debug.LogError("InventoryManager tidak ditemukan. Item tidak diambil.", this);
            return;
        }

        ItemDetails details = item != null ? item.Details : inventory.DefaultCoinItem;
        if (!inventory.TryAddItem(details))
            return;

        if (isCoin)
        {
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
                gameManager.AmbilKoin();
        }

        Destroy(other.gameObject);
    }
    void Update()
    {

        Vector3 arah = new Vector3(arahGerak.x, arahGerak.y, 0);
        transform.position += arah * kecepatan * Time.deltaTime;
    }
    
}
