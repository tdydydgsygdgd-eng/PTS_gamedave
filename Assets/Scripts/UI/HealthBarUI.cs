using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    public static HealthBarUI Instance { get; private set; }

    [Header("UI Elements")]
    public Slider healthSlider;
    public Image fillImage;
    public TMP_Text healthTextTMP;
    public Text healthTextLegacy;

    [Header("Settings")]
    public bool smoothTransition = true;
    public float smoothSpeed = 8f;

    private float targetFill = 1f;
    private float currentFill = 1f;

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
        if (PlayerController.Instance != null)
        {
            UpdateHealth(PlayerController.Instance.currentHP, PlayerController.Instance.maxHP);
        }
    }

    void Update()
    {
        if (smoothTransition && Mathf.Abs(currentFill - targetFill) > 0.001f)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, smoothSpeed * Time.deltaTime);
            ApplyFill(currentFill);
        }
    }

    public void UpdateHealth(float currentHP, float maxHP)
    {
        if (maxHP <= 0) maxHP = 100;
        targetFill = Mathf.Clamp01(currentHP / maxHP);

        if (!smoothTransition)
        {
            currentFill = targetFill;
            ApplyFill(currentFill);
        }

        string hpString = $"{Mathf.CeilToInt(currentHP)} / {Mathf.CeilToInt(maxHP)}";

        if (healthTextTMP != null)
        {
            healthTextTMP.text = hpString;
        }

        if (healthTextLegacy != null)
        {
            healthTextLegacy.text = hpString;
        }
    }

    private void ApplyFill(float fill)
    {
        if (healthSlider != null)
        {
            healthSlider.value = fill;
        }
        else if (fillImage != null)
        {
            fillImage.fillAmount = fill;
        }
    }
}
