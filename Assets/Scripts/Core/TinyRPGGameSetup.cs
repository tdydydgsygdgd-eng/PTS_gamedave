using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(-100)]
public class TinyRPGGameSetup : MonoBehaviour
{
    public static TinyRPGGameSetup Instance { get; private set; }

    [Header("Auto Setup Config")]
    public bool autoSetupPlayer = true;
    public bool autoSetupHUD = true;
    public bool autoSetupInventory = true;
    public bool autoSetupSpawner = true;
    public bool spawnInitialItems = true;

    private Canvas targetCanvas;
    private TMP_FontAsset defaultTMPFont;
    private Font defaultLegacyFont;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Cari font
        defaultTMPFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        defaultLegacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultLegacyFont == null)
        {
            defaultLegacyFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
        }

        FindOrCreateCanvas();
        SetupInventoryManager();

        if (autoSetupPlayer) SetupPlayer();
        if (autoSetupHUD) SetupHealthBarUI();
        if (autoSetupInventory) SetupInventoryUI();
        if (autoSetupSpawner) SetupEnemySpawner();
    }

    void Start()
    {
        if (spawnInitialItems)
        {
            SpawnWorldCollectibles();
        }
    }

    private void FindOrCreateCanvas()
    {
        targetCanvas = FindAnyObjectByType<Canvas>();
        if (targetCanvas == null)
        {
            GameObject canvasObj = new GameObject("GameUI");
            targetCanvas = canvasObj.AddComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    private void SetupInventoryManager()
    {
        if (InventoryManager.Instance == null)
        {
            InventoryManager mgr = GetComponent<InventoryManager>();
            if (mgr == null) mgr = gameObject.AddComponent<InventoryManager>();
        }
    }

    private void SetupPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null && PlayerController.Instance != null)
        {
            playerObj = PlayerController.Instance.gameObject;
        }

        if (playerObj == null)
        {
            Debug.LogWarning("[TinyRPGGameSetup] Player GameObject tidak ditemukan di scene!");
            return;
        }

        // Pastikan Rigidbody2D memiliki gravitasi 0 dan freeze rotation untuk game 2D top-down
        Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        // Pastikan PlayerController aktif
        PlayerController pc = playerObj.GetComponent<PlayerController>();
        if (pc == null)
        {
            pc = playerObj.AddComponent<PlayerController>();
        }

        // Setup Animasi Sprite Soldier
        SimpleSpriteAnimator anim = playerObj.GetComponent<SimpleSpriteAnimator>();
        if (anim == null)
        {
            anim = playerObj.AddComponent<SimpleSpriteAnimator>();
        }

        // Muat sprite soldier jika belum diisi
        if (anim.idleFrames == null || anim.idleFrames.Length == 0)
        {
            anim.idleFrames = LoadSortedSprites("TinyRPG/Characters/Soldier/Idle");
        }
        if (anim.walkFrames == null || anim.walkFrames.Length == 0)
        {
            anim.walkFrames = LoadSortedSprites("TinyRPG/Characters/Soldier/Walk");
        }
        if (anim.attackFrames == null || anim.attackFrames.Length == 0)
        {
            anim.attackFrames = LoadSortedSprites("TinyRPG/Characters/Soldier/Attack");
        }

        // Set sprite awal
        SpriteRenderer sr = playerObj.GetComponent<SpriteRenderer>();
        if (sr != null && anim.idleFrames != null && anim.idleFrames.Length > 0)
        {
            sr.sprite = anim.idleFrames[0];
            sr.sortingOrder = 2;
        }
    }

    private void SetupHealthBarUI()
    {
        if (HealthBarUI.Instance != null) return;
        if (targetCanvas == null) return;

        // Buat HUD Container di pojok kiri atas
        GameObject hudObj = new GameObject("PlayerHUD_HealthBar");
        hudObj.transform.SetParent(targetCanvas.transform, false);
        RectTransform hudRect = hudObj.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0, 1);
        hudRect.anchorMax = new Vector2(0, 1);
        hudRect.pivot = new Vector2(0, 1);
        hudRect.anchoredPosition = new Vector2(25, -25);
        hudRect.sizeDelta = new Vector2(260, 48);

        // Background Bar Panel
        GameObject bgObj = new GameObject("HUD_Background");
        bgObj.transform.SetParent(hudObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.12f, 0.12f, 0.15f, 0.85f);

        // Heart Icon
        GameObject iconObj = new GameObject("HeartIcon");
        iconObj.transform.SetParent(hudObj.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(6, 0);
        iconRect.sizeDelta = new Vector2(36, 36);
        Image iconImg = iconObj.AddComponent<Image>();
        Sprite heartSprite = Resources.Load<Sprite>("TinyRPG/Sprites/Heart_Icon");
        if (heartSprite != null) iconImg.sprite = heartSprite;
        iconImg.color = Color.white;

        // Health Slider Container
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(hudObj.transform, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0.5f);
        sliderRect.anchorMax = new Vector2(1, 0.5f);
        sliderRect.pivot = new Vector2(0, 0.5f);
        sliderRect.anchoredPosition = new Vector2(48, 0);
        sliderRect.sizeDelta = new Vector2(-56, 24);

        // Slider Background (dark red)
        GameObject barBg = new GameObject("BarBackground");
        barBg.transform.SetParent(sliderObj.transform, false);
        RectTransform barBgRect = barBg.AddComponent<RectTransform>();
        barBgRect.anchorMin = Vector2.zero;
        barBgRect.anchorMax = Vector2.one;
        barBgRect.sizeDelta = Vector2.zero;
        Image barBgImg = barBg.AddComponent<Image>();
        barBgImg.color = new Color(0.35f, 0.08f, 0.08f, 1f);

        // Slider Fill Area
        GameObject fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        // Slider Fill (bright red gradient)
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(0.92f, 0.2f, 0.2f, 1f);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        // Health Text
        GameObject textObj = new GameObject("HPText");
        textObj.transform.SetParent(sliderObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        HealthBarUI healthUI = hudObj.AddComponent<HealthBarUI>();
        healthUI.healthSlider = slider;
        healthUI.fillImage = fillImg;

        if (defaultTMPFont != null)
        {
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.font = defaultTMPFont;
            tmpText.fontSize = 14;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;
            tmpText.text = "100 / 100";
            healthUI.healthTextTMP = tmpText;
        }
        else
        {
            Text legText = textObj.AddComponent<Text>();
            legText.font = defaultLegacyFont;
            legText.fontSize = 13;
            legText.fontStyle = FontStyle.Bold;
            legText.alignment = TextAnchor.MiddleCenter;
            legText.color = Color.white;
            legText.text = "100 / 100";
            healthUI.healthTextLegacy = legText;
        }

        if (PlayerController.Instance != null)
        {
            healthUI.UpdateHealth(PlayerController.Instance.currentHP, PlayerController.Instance.maxHP);
        }
    }

    private void SetupInventoryUI()
    {
        if (InventoryUI.Instance != null) return;
        if (targetCanvas == null) return;

        Sprite slotBgSprite = Resources.Load<Sprite>("TinyRPG/Sprites/Slot_Bg");

        // 1. Tombol Tas di Layar HUD (Pojok Kanan Bawah / Atas)
        GameObject toggleBtnObj = new GameObject("HUD_InventoryButton");
        toggleBtnObj.transform.SetParent(targetCanvas.transform, false);
        RectTransform btnRect = toggleBtnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 0);
        btnRect.anchorMax = new Vector2(1, 0);
        btnRect.pivot = new Vector2(1, 0);
        btnRect.anchoredPosition = new Vector2(-25, 25);
        btnRect.sizeDelta = new Vector2(150, 44);
        Image btnImg = toggleBtnObj.AddComponent<Image>();
        btnImg.color = new Color(0.18f, 0.18f, 0.24f, 0.95f);
        Button toggleBtn = toggleBtnObj.AddComponent<Button>();

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(toggleBtnObj.transform, false);
        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;
        CreateTextComponent(btnTextObj, "ðŸŽ’ Buka Tas [I]", 14, Color.yellow, TextAnchor.MiddleCenter);

        // 2. Window Inventory Utama (Tengah Layar)
        GameObject windowObj = new GameObject("InventoryWindow");
        windowObj.transform.SetParent(targetCanvas.transform, false);
        RectTransform winRect = windowObj.AddComponent<RectTransform>();
        winRect.anchorMin = new Vector2(0.5f, 0.5f);
        winRect.anchorMax = new Vector2(0.5f, 0.5f);
        winRect.pivot = new Vector2(0.5f, 0.5f);
        winRect.anchoredPosition = Vector2.zero;
        winRect.sizeDelta = new Vector2(560, 360);
        Image winImg = windowObj.AddComponent<Image>();
        winImg.color = new Color(0.11f, 0.12f, 0.16f, 0.98f);

        // Window Border / Header
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(windowObj.transform, false);
        RectTransform headerRect = headerObj.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.anchoredPosition = Vector2.zero;
        headerRect.sizeDelta = new Vector2(0, 46);
        Image headerImg = headerObj.AddComponent<Image>();
        headerImg.color = new Color(0.16f, 0.18f, 0.24f, 1f);

        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(headerObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = Vector2.one;
        titleRect.sizeDelta = new Vector2(-60, 0);
        titleRect.anchoredPosition = new Vector2(15, 0);
        CreateTextComponent(titleObj, "âš”ï¸ TAS & INVENTORY PEMAIN âš”ï¸", 16, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleLeft);

        // Tombol Tutup Window ("X")
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(headerObj.transform, false);
        RectTransform closeBtnRect = closeBtnObj.AddComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(1, 0.5f);
        closeBtnRect.anchorMax = new Vector2(1, 0.5f);
        closeBtnRect.pivot = new Vector2(1, 0.5f);
        closeBtnRect.anchoredPosition = new Vector2(-10, 0);
        closeBtnRect.sizeDelta = new Vector2(32, 32);
        Image closeImg = closeBtnObj.AddComponent<Image>();
        closeImg.color = new Color(0.7f, 0.2f, 0.2f, 1f);
        Button closeBtn = closeBtnObj.AddComponent<Button>();

        GameObject closeTxtObj = new GameObject("Text");
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform closeTxtRect = closeTxtObj.AddComponent<RectTransform>();
        closeTxtRect.anchorMin = Vector2.zero;
        closeTxtRect.anchorMax = Vector2.one;
        closeTxtRect.sizeDelta = Vector2.zero;
        CreateTextComponent(closeTxtObj, "âœ–", 14, Color.white, TextAnchor.MiddleCenter);

        // 3. Area Kiri: Grid Slot Inventory
        GameObject slotsArea = new GameObject("SlotsArea");
        slotsArea.transform.SetParent(windowObj.transform, false);
        RectTransform slotsAreaRect = slotsArea.AddComponent<RectTransform>();
        slotsAreaRect.anchorMin = new Vector2(0, 0);
        slotsAreaRect.anchorMax = new Vector2(0.58f, 1);
        slotsAreaRect.pivot = new Vector2(0, 0.5f);
        slotsAreaRect.anchoredPosition = new Vector2(20, -25);
        slotsAreaRect.sizeDelta = new Vector2(-30, -65);

        GridLayoutGroup grid = slotsArea.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(62, 62);
        grid.spacing = new Vector2(8, 8);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;

        // 4. Area Kanan: Panel Detail Item
        GameObject detailObj = new GameObject("ItemDetailPanel");
        detailObj.transform.SetParent(windowObj.transform, false);
        RectTransform detailRect = detailObj.AddComponent<RectTransform>();
        detailRect.anchorMin = new Vector2(0.58f, 0);
        detailRect.anchorMax = new Vector2(1, 1);
        detailRect.pivot = new Vector2(1, 0.5f);
        detailRect.anchoredPosition = new Vector2(-20, -25);
        detailRect.sizeDelta = new Vector2(-30, -65);
        Image detailBg = detailObj.AddComponent<Image>();
        detailBg.color = new Color(0.08f, 0.09f, 0.12f, 0.95f);

        // Icon Preview di Detail
        GameObject dIconObj = new GameObject("DetailIcon");
        dIconObj.transform.SetParent(detailObj.transform, false);
        RectTransform dIconRect = dIconObj.AddComponent<RectTransform>();
        dIconRect.anchorMin = new Vector2(0.5f, 1);
        dIconRect.anchorMax = new Vector2(0.5f, 1);
        dIconRect.pivot = new Vector2(0.5f, 1);
        dIconRect.anchoredPosition = new Vector2(0, -15);
        dIconRect.sizeDelta = new Vector2(58, 58);
        Image dIconImg = dIconObj.AddComponent<Image>();

        // Nama Item
        GameObject dNameObj = new GameObject("DetailName");
        dNameObj.transform.SetParent(detailObj.transform, false);
        RectTransform dNameRect = dNameObj.AddComponent<RectTransform>();
        dNameRect.anchorMin = new Vector2(0.5f, 1);
        dNameRect.anchorMax = new Vector2(0.5f, 1);
        dNameRect.pivot = new Vector2(0.5f, 1);
        dNameRect.anchoredPosition = new Vector2(0, -78);
        dNameRect.sizeDelta = new Vector2(190, 24);
        var dNameTxt = CreateTextComponent(dNameObj, "Nama Item", 14, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter);

        // Tipe Item
        GameObject dTypeObj = new GameObject("DetailType");
        dTypeObj.transform.SetParent(detailObj.transform, false);
        RectTransform dTypeRect = dTypeObj.AddComponent<RectTransform>();
        dTypeRect.anchorMin = new Vector2(0.5f, 1);
        dTypeRect.anchorMax = new Vector2(0.5f, 1);
        dTypeRect.pivot = new Vector2(0.5f, 1);
        dTypeRect.anchoredPosition = new Vector2(0, -102);
        dTypeRect.sizeDelta = new Vector2(190, 20);
        var dTypeTxt = CreateTextComponent(dTypeObj, "Tipe: Konsumsi", 11, Color.cyan, TextAnchor.MiddleCenter);

        // Deskripsi Item
        GameObject dDescObj = new GameObject("DetailDesc");
        dDescObj.transform.SetParent(detailObj.transform, false);
        RectTransform dDescRect = dDescObj.AddComponent<RectTransform>();
        dDescRect.anchorMin = new Vector2(0, 0);
        dDescRect.anchorMax = new Vector2(1, 1);
        dDescRect.pivot = new Vector2(0.5f, 0.5f);
        dDescRect.anchoredPosition = new Vector2(0, -10);
        dDescRect.sizeDelta = new Vector2(-24, -180);
        var dDescTxt = CreateTextComponent(dDescObj, "Klik salah satu item untuk melihat detailnya di sini.", 11, new Color(0.85f, 0.85f, 0.85f), TextAnchor.UpperLeft);

        // Efek / Nilai
        GameObject dValObj = new GameObject("DetailValue");
        dValObj.transform.SetParent(detailObj.transform, false);
        RectTransform dValRect = dValObj.AddComponent<RectTransform>();
        dValRect.anchorMin = new Vector2(0.5f, 0);
        dValRect.anchorMax = new Vector2(0.5f, 0);
        dValRect.pivot = new Vector2(0.5f, 0);
        dValRect.anchoredPosition = new Vector2(0, 48);
        dValRect.sizeDelta = new Vector2(190, 20);
        var dValTxt = CreateTextComponent(dValObj, "Efek: +30 HP", 12, Color.green, TextAnchor.MiddleCenter);

        // Tombol Gunakan / Pakai
        GameObject useBtnObj = new GameObject("UseButton");
        useBtnObj.transform.SetParent(detailObj.transform, false);
        RectTransform useBtnRect = useBtnObj.AddComponent<RectTransform>();
        useBtnRect.anchorMin = new Vector2(0.5f, 0);
        useBtnRect.anchorMax = new Vector2(0.5f, 0);
        useBtnRect.pivot = new Vector2(0.5f, 0);
        useBtnRect.anchoredPosition = new Vector2(0, 10);
        useBtnRect.sizeDelta = new Vector2(180, 32);
        Image useBtnImg = useBtnObj.AddComponent<Image>();
        useBtnImg.color = new Color(0.2f, 0.6f, 0.25f, 1f);
        Button useBtn = useBtnObj.AddComponent<Button>();

        GameObject useTxtObj = new GameObject("Text");
        useTxtObj.transform.SetParent(useBtnObj.transform, false);
        RectTransform useTxtRect = useTxtObj.AddComponent<RectTransform>();
        useTxtRect.anchorMin = Vector2.zero;
        useTxtRect.anchorMax = Vector2.one;
        useTxtRect.sizeDelta = Vector2.zero;
        var useBtnTxt = CreateTextComponent(useTxtObj, "Gunakan (Pakai)", 12, Color.white, TextAnchor.MiddleCenter);

        ItemDetailPanelUI detailPanel = detailObj.AddComponent<ItemDetailPanelUI>();
        detailPanel.panelRoot = detailObj;
        detailPanel.iconImage = dIconImg;
        detailPanel.useButton = useBtn;
        AssignTextToDetail(detailPanel, dNameTxt, dTypeTxt, dDescTxt, dValTxt, useBtnTxt);

        // Buat 12 Slot di grid
        List<InventorySlotUI> slotsList = new List<InventorySlotUI>();
        for (int i = 0; i < 12; i++)
        {
            GameObject slotObj = new GameObject($"Slot_{i}");
            slotObj.transform.SetParent(slotsArea.transform, false);
            Image slotBg = slotObj.AddComponent<Image>();
            if (slotBgSprite != null)
            {
                slotBg.sprite = slotBgSprite;
                slotBg.color = Color.white;
            }
            else
            {
                slotBg.color = new Color(0.2f, 0.22f, 0.28f, 1f);
            }
            Button slotBtn = slotObj.AddComponent<Button>();

            // Icon di dalam slot
            GameObject sIconObj = new GameObject("ItemIcon");
            sIconObj.transform.SetParent(slotObj.transform, false);
            RectTransform sIconRect = sIconObj.AddComponent<RectTransform>();
            sIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            sIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            sIconRect.sizeDelta = new Vector2(44, 44);
            Image sIconImg = sIconObj.AddComponent<Image>();
            sIconImg.gameObject.SetActive(false);

            // Quantity Text di pojok kanan bawah slot
            GameObject sQtyObj = new GameObject("QtyText");
            sQtyObj.transform.SetParent(slotObj.transform, false);
            RectTransform sQtyRect = sQtyObj.AddComponent<RectTransform>();
            sQtyRect.anchorMin = new Vector2(0.4f, 0);
            sQtyRect.anchorMax = new Vector2(1, 0.6f);
            sQtyRect.anchoredPosition = new Vector2(-3, 3);
            var sQtyTxt = CreateTextComponent(sQtyObj, "", 12, Color.white, TextAnchor.LowerRight);

            InventorySlotUI slotUI = slotObj.AddComponent<InventorySlotUI>();
            slotUI.slotButton = slotBtn;
            slotUI.iconImage = sIconImg;
            if (sQtyTxt is TextMeshProUGUI tmpQty) slotUI.quantityTextTMP = tmpQty;
            else if (sQtyTxt is Text legQty) slotUI.quantityTextLegacy = legQty;

            slotsList.Add(slotUI);
        }

        // Hubungkan ke InventoryUI
        InventoryUI invUI = windowObj.AddComponent<InventoryUI>();
        invUI.inventoryWindow = windowObj;
        invUI.openButton = toggleBtn;
        invUI.closeButton = closeBtn;
        invUI.slotsContainer = slotsArea.transform;
        invUI.detailPanel = detailPanel;
        invUI.slotUIList = slotsList;

        windowObj.SetActive(false);
    }

    private void SetupEnemySpawner()
    {
        if (FindAnyObjectByType<EnemySpawner>() != null) return;

        GameObject spawnerObj = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();
        spawner.enemyPrefab = Resources.Load<GameObject>("TinyRPG/Prefabs/EnemyOrc");
        spawner.spawnInterval = 3.5f;
        spawner.maxConcurrentEnemies = 5;
        spawner.spawnRadiusMin = 4.5f;
        spawner.spawnRadiusMax = 7.5f;
    }

    private void SpawnWorldCollectibles()
    {
        Vector3 playerPos = Vector3.zero;
        if (PlayerController.Instance != null)
        {
            playerPos = PlayerController.Instance.transform.position;
        }

        GameObject potionPrefab = Resources.Load<GameObject>("TinyRPG/Prefabs/ItemPickup_Potion");
        GameObject coinPrefab = Resources.Load<GameObject>("TinyRPG/Prefabs/ItemPickup_Coin");
        GameObject gemPrefab = Resources.Load<GameObject>("TinyRPG/Prefabs/ItemPickup_Gem");
        GameObject arrowPrefab = Resources.Load<GameObject>("TinyRPG/Prefabs/ItemPickup_Arrow");

        Vector3[] spawnOffsets = new Vector3[]
        {
            new Vector3(2.5f, 1.2f, 0),
            new Vector3(-2.2f, 2.0f, 0),
            new Vector3(3.0f, -2.2f, 0),
            new Vector3(-3.0f, -1.5f, 0),
            new Vector3(1.2f, -2.8f, 0),
            new Vector3(-1.5f, -2.2f, 0)
        };

        GameObject[] prefabsToSpawn = new GameObject[]
        {
            potionPrefab,
            coinPrefab,
            gemPrefab,
            arrowPrefab,
            potionPrefab,
            coinPrefab
        };

        for (int i = 0; i < prefabsToSpawn.Length; i++)
        {
            if (prefabsToSpawn[i] != null)
            {
                Instantiate(prefabsToSpawn[i], playerPos + spawnOffsets[i], Quaternion.identity);
            }
        }

        Debug.Log("[TinyRPGGameSetup] Berhasil men-spawn collectible items di sekitar player!");
    }

    private Sprite[] LoadSortedSprites(string path)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        if (sprites == null || sprites.Length == 0) return sprites;

        System.Array.Sort(sprites, (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        return sprites;
    }

    private Component CreateTextComponent(GameObject obj, string text, int fontSize, Color color, TextAnchor legacyAlign)
    {
        if (defaultTMPFont != null)
        {
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.font = defaultTMPFont;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.text = text;
            tmp.alignment = ConvertToTMPAlignment(legacyAlign);
            return tmp;
        }
        else
        {
            Text leg = obj.AddComponent<Text>();
            leg.font = defaultLegacyFont;
            leg.fontSize = fontSize;
            leg.color = color;
            leg.text = text;
            leg.alignment = legacyAlign;
            return leg;
        }
    }

    private void AssignTextToDetail(ItemDetailPanelUI panel, Component name, Component type, Component desc, Component val, Component btn)
    {
        if (name is TextMeshProUGUI nTMP) panel.nameTextTMP = nTMP; else if (name is Text nLeg) panel.nameTextLegacy = nLeg;
        if (type is TextMeshProUGUI tTMP) panel.typeTextTMP = tTMP; else if (type is Text tLeg) panel.typeTextLegacy = tLeg;
        if (desc is TextMeshProUGUI dTMP) panel.descriptionTextTMP = dTMP; else if (desc is Text dLeg) panel.descriptionTextLegacy = dLeg;
        if (val is TextMeshProUGUI vTMP) panel.valueTextTMP = vTMP; else if (val is Text vLeg) panel.valueTextLegacy = vLeg;
        if (btn is TextMeshProUGUI bTMP) panel.useButtonTextTMP = bTMP; else if (btn is Text bLeg) panel.useButtonTextLegacy = bLeg;
    }

    private TextAlignmentOptions ConvertToTMPAlignment(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
            case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
            case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
            case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
            case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
            case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
            case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
            case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
            default: return TextAlignmentOptions.Center;
        }
    }
}
