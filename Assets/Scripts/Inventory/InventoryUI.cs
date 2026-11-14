using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("Window Panels")]
    public GameObject inventoryWindow;
    public Button openButton;
    public Button closeButton;

    [Header("Slots Configuration")]
    public Transform slotsContainer;
    public GameObject slotPrefab;
    public List<InventorySlotUI> slotUIList = new List<InventorySlotUI>();

    [Header("Detail Panel")]
    public ItemDetailPanelUI detailPanel;

    private bool isOpen = false;

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

    void Start()
    {
        if (openButton != null)
        {
            openButton.onClick.AddListener(OpenInventory);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseInventory);
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
        }

        // Cari slot UI yang sudah ada di container jika belum di-assign
        if (slotsContainer != null && slotUIList.Count == 0)
        {
            slotUIList.AddRange(slotsContainer.GetComponentsInChildren<InventorySlotUI>(true));
        }

        // Sembunyikan window saat awal permainan
        if (inventoryWindow != null)
        {
            inventoryWindow.SetActive(false);
        }

        if (detailPanel != null)
        {
            detailPanel.HideDetail();
        }
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
        }
    }

    void Update()
    {
        // Tombol pintasan 'I' atau 'Tab' untuk membuka/menutup tas
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (isOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    public void OpenInventory()
    {
        isOpen = true;
        if (inventoryWindow != null) inventoryWindow.SetActive(true);
        RefreshUI();
    }

    public void CloseInventory()
    {
        isOpen = false;
        if (inventoryWindow != null) inventoryWindow.SetActive(false);
        if (detailPanel != null) detailPanel.HideDetail();
    }

    public void RefreshUI()
    {
        if (InventoryManager.Instance == null) return;

        List<InventorySlot> slots = InventoryManager.Instance.slots;

        // Jika slot UI di hierarki lebih sedikit dari maxSlots dan ada prefab, buat slot baru
        if (slotsContainer != null && slotPrefab != null && slotUIList.Count < InventoryManager.Instance.maxSlots)
        {
            int toCreate = InventoryManager.Instance.maxSlots - slotUIList.Count;
            for (int i = 0; i < toCreate; i++)
            {
                GameObject newSlotObj = Instantiate(slotPrefab, slotsContainer);
                InventorySlotUI slotUI = newSlotObj.GetComponent<InventorySlotUI>();
                if (slotUI != null) slotUIList.Add(slotUI);
            }
        }

        // Tampilkan item pada setiap slot
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i < slots.Count && slots[i].item != null)
            {
                slotUIList[i].Setup(slots[i].item, slots[i].quantity, OnSlotClicked);
            }
            else
            {
                slotUIList[i].Clear();
            }
        }
    }

    private void OnSlotClicked(ItemData item)
    {
        if (detailPanel != null && item != null)
        {
            detailPanel.ShowDetail(item);
        }
    }
}
