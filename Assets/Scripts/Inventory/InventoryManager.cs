using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;

    public InventorySlot(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Config")]
    public int maxSlots = 12;
    public List<InventorySlot> slots = new List<InventorySlot>();

    public event Action OnInventoryChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    public bool AddItem(ItemData item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        // Cari apakah item sudah ada di dalam tas (stackable)
        InventorySlot existingSlot = slots.Find(s => s.item == item);
        if (existingSlot != null)
        {
            existingSlot.quantity += quantity;
            OnInventoryChanged?.Invoke();
            Debug.Log($"[Inventory] Berhasil menambah {quantity}x {item.itemName}. Total: {existingSlot.quantity}");
            return true;
        }

        // Cek kapasitas tas
        if (slots.Count >= maxSlots)
        {
            Debug.LogWarning("[Inventory] Tas penuh! Tidak dapat mengambil item lagi.");
            return false;
        }

        slots.Add(new InventorySlot(item, quantity));
        OnInventoryChanged?.Invoke();
        Debug.Log($"[Inventory] Berhasil mendapatkan item baru: {item.itemName} x{quantity}");
        return true;
    }

    public bool RemoveItem(ItemData item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        InventorySlot existingSlot = slots.Find(s => s.item == item);
        if (existingSlot == null) return false;

        existingSlot.quantity -= quantity;
        if (existingSlot.quantity <= 0)
        {
            slots.Remove(existingSlot);
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public void UseItem(ItemData item)
    {
        if (item == null) return;

        if (item.itemType == ItemType.Consumable)
        {
            PlayerController player = PlayerController.Instance;
            if (player != null)
            {
                player.Heal(item.value);
                Debug.Log($"[Inventory] Menggunakan {item.itemName}! Memulihkan {item.value} HP.");
                RemoveItem(item, 1);
            }
        }
        else
        {
            Debug.Log($"[Inventory] {item.itemName} bukan item konsumsi.");
        }
    }
}
