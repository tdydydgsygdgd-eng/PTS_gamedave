using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollectibleItem : MonoBehaviour
{
    [Header("Item Configuration")]
    public ItemData itemData;
    public int quantity = 1;

    [Header("Floating Animation")]
    public float bobSpeed = 3f;
    public float bobHeight = 0.12f;

    private Vector3 initialPosition;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        initialPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (itemData != null && spriteRenderer != null && itemData.icon != null)
        {
            spriteRenderer.sprite = itemData.icon;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Update()
    {
        float newY = initialPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            Collect();
        }
    }

    public void Collect()
    {
        if (itemData == null) return;

        if (InventoryManager.Instance != null)
        {
            bool added = InventoryManager.Instance.AddItem(itemData, quantity);
            if (added)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogWarning("[CollectibleItem] InventoryManager tidak ditemukan di scene!");
            Destroy(gameObject);
        }
    }
}
