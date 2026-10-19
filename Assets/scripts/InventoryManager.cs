using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour, GrabableItem
{
    [SerializeField] private GameObject InventoryCanvas;
    [SerializeField] private ItemDetails defaultCoinItem;
    private bool InventoryActive;
    private float previousTimeScale = 1f;
    private InputAction inventoryAction;
    private InputAction fallbackInventoryAction;

    void Start()
    {
        if (InventoryCanvas == null)
        {
            GameObject canvasObject = GameObject.Find("IventoryCanvas");
            if (canvasObject == null)
                canvasObject = GameObject.Find("InventoryCanvas");

            InventoryCanvas = canvasObject;
        }

        CreateSlots();

        if (InventoryCanvas != null)
            InventoryCanvas.SetActive(false);

        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null && playerInput.actions != null)
            inventoryAction = playerInput.actions.FindAction("Inventory", false);

        if (inventoryAction == null && InputSystem.actions != null)
            inventoryAction = InputSystem.actions.FindAction("Inventory", false);

        // Tetap bisa membuka inventory jika action belum ditambahkan ke Input Actions.
        if (inventoryAction == null)
        {
            fallbackInventoryAction = new InputAction(
                "Inventory",
                InputActionType.Button,
                "<Keyboard>/i");
            fallbackInventoryAction.Enable();
            inventoryAction = fallbackInventoryAction;
            Debug.LogWarning("Action Inventory belum ada. Tombol I dipakai sebagai cadangan.", this);
        }

        InventoryActive = false;
    }

    void Update()
    {
        if (inventoryAction != null && inventoryAction.WasPressedThisFrame())
            ToggleInventory();
    }

    private void OnDisable()
    {
        if (InventoryActive)
        {
            Time.timeScale = previousTimeScale;
            InventoryActive = false;
        }

        if (fallbackInventoryAction != null)
        {
            fallbackInventoryAction.Disable();
            fallbackInventoryAction.Dispose();
            fallbackInventoryAction = null;
        }
    }

    private void ToggleInventory()
    {
        if (InventoryCanvas == null)
        {
            Debug.LogError("Inventory Canvas belum dipasang pada InventoryManager.", this);
            return;
        }

        InventoryActive = !InventoryActive;
        InventoryCanvas.SetActive(InventoryActive);

        if (InventoryActive)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = previousTimeScale;
        }
    }

    private void CreateSlots()
    {
        if (InventoryCanvas == null)
        {
            Debug.LogError("Inventory Canvas belum dipasang pada InventoryManager.", this);
            itemSlot = new ItemSlot[0];
            return;
        }

        GridLayoutGroup grid = InventoryCanvas.GetComponentInChildren<GridLayoutGroup>(true);
        if (grid == null)
        {
            Debug.LogError("Tambahkan GridLayoutGroup di dalam Inventory Canvas.", this);
            itemSlot = new ItemSlot[0];
            return;
        }

        System.Collections.Generic.List<ItemSlot> slots =
            new System.Collections.Generic.List<ItemSlot>();

        for (int i = 0; i < grid.transform.childCount; i++)
        {
            Transform slotTransform = grid.transform.GetChild(i);
            if (slotTransform.GetComponent<Image>() == null)
                continue;

            ItemSlot slot = slotTransform.GetComponent<ItemSlot>();
            if (slot == null)
                slot = slotTransform.gameObject.AddComponent<ItemSlot>();

            slot.Initialize();
            slots.Add(slot);
        }

        itemSlot = slots.ToArray();
        if (itemSlot.Length == 0)
            Debug.LogError("Belum ada slot kosong di dalam GridLayoutGroup inventory.", this);
    }

    public ItemSlot[] itemSlot;

    public ItemDetails DefaultCoinItem
    {
        get
        {
            if (defaultCoinItem == null)
                defaultCoinItem = Resources.Load<ItemDetails>("ItemCoin");

            if (defaultCoinItem == null)
            {
                defaultCoinItem = ScriptableObject.CreateInstance<ItemDetails>();
                defaultCoinItem.ItemName = "Koin";
                defaultCoinItem.Quantity = 1;

                GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
                if (coins.Length > 0)
                {
                    SpriteRenderer coinRenderer = coins[0].GetComponent<SpriteRenderer>();
                    if (coinRenderer != null)
                        defaultCoinItem.ItemIcon = coinRenderer.sprite;
                }
            }

            return defaultCoinItem;
        }
    }

    public void AddItem(ItemDetails item)
    {
        TryAddItem(item);
    }

    public bool TryAddItem(ItemDetails item)
    {
        if (item == null)
        {
            Debug.LogWarning("Item tidak memiliki ItemDetails dan tidak masuk inventory.", this);
            return false;
        }

        if (itemSlot == null || itemSlot.Length == 0)
        {
            Debug.LogWarning("Inventory belum memiliki slot. Item tidak diambil.", this);
            return false;
        }

        int remaining = Mathf.Max(1, item.Quantity);

        // Pastikan seluruh jumlah item muat sebelum mengubah slot mana pun.
        foreach (ItemSlot slot in itemSlot)
        {
            if (slot.CanStack(item))
                remaining -= Mathf.Min(remaining, ItemSlot.MaxStackSize - slot.itemQuantity);
        }

        foreach (ItemSlot slot in itemSlot)
        {
            if (!slot.isFull)
                remaining -= Mathf.Min(remaining, ItemSlot.MaxStackSize);
        }

        if (remaining > 0)
        {
            Debug.LogWarning("Inventory penuh. Item '" + item.ItemName + "' tetap di scene.", this);
            return false;
        }

        remaining = Mathf.Max(1, item.Quantity);
        foreach (ItemSlot slot in itemSlot)
        {
            if (slot.CanStack(item))
                remaining -= slot.AddToStack(item, remaining);

            if (remaining <= 0)
                break;
        }

        foreach (ItemSlot slot in itemSlot)
        {
            if (!slot.isFull)
            {
                int amount = Mathf.Min(remaining, ItemSlot.MaxStackSize);
                slot.SetItem(item, amount);
                remaining -= amount;
            }

            if (remaining <= 0)
                break;
        }

        Debug.Log("Item masuk inventory: " + item.ItemName + " x" + item.Quantity, this);
        return true;
    }
}
