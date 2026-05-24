using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("物品欄設定")]
    public int maxItemCount = 3;

    [Header("物品欄圖片")]
    public Image[] slotImages;

    [Header("物品欄 Slot")]
    public RectTransform[] slots;

    [Header("補劑圖示")]
    public Sprite proteinIcon;
    public Sprite creatineIcon;
    public Sprite riceIcon;
    public Sprite testosteroneIcon;
    public Sprite vitaminIcon;

    [Header("睪固酮無敵")]
    public SpriteRenderer playerSprite;
    public GameObject invinciblePanel;
    public Text invincibleCountdownText;
    public Image invincibleFillImage;
    public float testosteroneDuration = 5f;
    public Color testosteroneColor = new Color(0.75f, 0.25f, 1f, 1f);
    public Color testosteroneBarColor = new Color(0.72f, 0.18f, 1f, 1f);

    [Header("維他命慢動作")]
    public float vitaminSlowMotionDuration = 10f;
    public float vitaminSlowMotionScale = 0.45f;
    public GameObject slowMotionPanel;
    public Text slowMotionCountdownText;
    public Image slowMotionFillImage;
    public Color vitaminBarColor = new Color(0.1f, 0.85f, 1f, 1f);

    [Header("目前數量")]
    public int currentItemCount;

    private List<SupplementType> items = new List<SupplementType>();
    private Coroutine testosteroneRoutine;
    private Coroutine vitaminRoutine;
    private Color originalPlayerColor = Color.white;
    private bool testosteroneActive = false;
    private bool vitaminActive = false;
    private float originalTimeScale = 1f;
    private float originalFixedDeltaTime = 0.02f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (invinciblePanel != null)
        {
            invinciblePanel.SetActive(false);
        }

        if (slowMotionPanel != null)
        {
            slowMotionPanel.SetActive(false);
        }

        UpdateStatusPanel(invinciblePanel, invincibleFillImage, invincibleCountdownText, 0f, 1f, "", testosteroneBarColor);
        UpdateStatusPanel(slowMotionPanel, slowMotionFillImage, slowMotionCountdownText, 0f, 1f, "", vitaminBarColor);
        UpdateUI();
    }

    private void OnDisable()
    {
        if (testosteroneRoutine != null)
        {
            StopCoroutine(testosteroneRoutine);
            testosteroneRoutine = null;
        }

        if (vitaminRoutine != null)
        {
            StopCoroutine(vitaminRoutine);
            vitaminRoutine = null;
        }

        if (HealthUI.Instance != null)
        {
            HealthUI.Instance.SetInvincible(false);
        }

        if (testosteroneActive && playerSprite != null)
        {
            playerSprite.color = originalPlayerColor;
        }

        if (invinciblePanel != null)
        {
            invinciblePanel.SetActive(false);
        }

        if (slowMotionPanel != null)
        {
            slowMotionPanel.SetActive(false);
        }

        RestoreSlowMotion();
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopSlowMotionAudio(false);
        }

        UpdateStatusPanel(invinciblePanel, invincibleFillImage, invincibleCountdownText, 0f, 1f, "", testosteroneBarColor);
        UpdateStatusPanel(slowMotionPanel, slowMotionFillImage, slowMotionCountdownText, 0f, 1f, "", vitaminBarColor);
        testosteroneActive = false;
        vitaminActive = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            UseItem(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            UseItem(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            UseItem(2);
    }

    public bool AddItem(SupplementType type)
    {
        if (items.Count >= maxItemCount)
        {
            Debug.Log("物品欄滿了");
            return false;
        }

        items.Add(type);
        currentItemCount = items.Count;

        UpdateUI();

        Debug.Log("加入物品欄：" + type);
        return true;
    }

    public void UseItem(int index)
    {
        if (index < 0 || index >= items.Count)
        {
            Debug.Log("這格沒有道具：" + (index + 1));
            return;
        }

        SupplementType type = items[index];

        Debug.Log("使用補劑：" + type);

        ApplyEffect(type);
        GameStats.AddUsedSupplement();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySupplementUse();

        items.RemoveAt(index);
        currentItemCount = items.Count;

        UpdateUI();
    }

    public bool UseFirstItemOfType(SupplementType type)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != type)
                continue;

            UseItem(i);
            return true;
        }

        Debug.Log("物品欄沒有這個道具：" + type);
        return false;
    }

    private void ApplyEffect(SupplementType type)
    {
        switch (type)
        {
            case SupplementType.Protein:
                if (GameTimer.Instance != null)
                {
                    GameTimer.Instance.AddTime(10f);
                    Debug.Log("蛋白粉效果：時間 +10 秒");
                }
                else
                {
                    Debug.LogError("找不到 GameTimer.Instance");
                }
                break;

            case SupplementType.Creatine:
                if (HealthUI.Instance != null)
                {
                    HealthUI.Instance.HealPercent(0.15f);
                    Debug.Log("肌酸效果：回血 15%");
                }
                break;

            case SupplementType.Rice:
                Debug.Log("白飯保留");
                break;

            case SupplementType.Testosterone:
                if (testosteroneRoutine != null)
                {
                    StopCoroutine(testosteroneRoutine);
                }

                testosteroneRoutine = StartCoroutine(TestosteroneRoutine());
                break;

            case SupplementType.Vitamin:
                if (vitaminRoutine != null)
                {
                    StopCoroutine(vitaminRoutine);
                    RestoreSlowMotion();
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.StopSlowMotionAudio(false);
                    }
                }

                vitaminRoutine = StartCoroutine(VitaminSlowMotionRoutine());
                break;
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (i < items.Count)
            {
                slotImages[i].sprite = GetIcon(items[i]);
                slotImages[i].enabled = true;
                slotImages[i].color = Color.white;
            }
            else
            {
                slotImages[i].sprite = null;
                slotImages[i].enabled = false;
            }
        }
    }

    private IEnumerator TestosteroneRoutine()
    {
        if (playerSprite != null)
        {
            if (!testosteroneActive)
            {
                originalPlayerColor = playerSprite.color;
            }

            playerSprite.color = testosteroneColor;
        }

        testosteroneActive = true;

        if (HealthUI.Instance != null)
        {
            HealthUI.Instance.SetInvincible(true);
        }

        float timer = testosteroneDuration;

        while (timer > 0f)
        {
            UpdateStatusPanel(invinciblePanel, invincibleFillImage, invincibleCountdownText, timer, testosteroneDuration, "INVINCIBLE", testosteroneBarColor);
            timer -= Time.unscaledDeltaTime;
            yield return null;
        }

        UpdateStatusPanel(invinciblePanel, invincibleFillImage, invincibleCountdownText, 0f, testosteroneDuration, "INVINCIBLE", testosteroneBarColor);

        if (HealthUI.Instance != null)
        {
            HealthUI.Instance.SetInvincible(false);
        }

        if (playerSprite != null)
        {
            playerSprite.color = originalPlayerColor;
        }

        if (invinciblePanel != null)
        {
            invinciblePanel.SetActive(false);
        }

        testosteroneActive = false;
        testosteroneRoutine = null;
        Debug.Log("睪固酮無敵結束");
    }

    private IEnumerator VitaminSlowMotionRoutine()
    {
        vitaminActive = true;
        originalTimeScale = Time.timeScale;
        originalFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = Mathf.Clamp(vitaminSlowMotionScale, 0.05f, 1f);
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StartSlowMotionAudio();
        }

        float timer = vitaminSlowMotionDuration;

        while (timer > 0f)
        {
            UpdateStatusPanel(slowMotionPanel, slowMotionFillImage, slowMotionCountdownText, timer, vitaminSlowMotionDuration, "SLOW", vitaminBarColor);
            timer -= Time.unscaledDeltaTime;
            yield return null;
        }

        RestoreSlowMotion();
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopSlowMotionAudio();
        }

        UpdateStatusPanel(slowMotionPanel, slowMotionFillImage, slowMotionCountdownText, 0f, vitaminSlowMotionDuration, "SLOW", vitaminBarColor);

        vitaminActive = false;
        vitaminRoutine = null;
        Debug.Log("維他命慢動作結束");
    }

    private void RestoreSlowMotion()
    {
        if (!vitaminActive) return;

        if (Time.timeScale > 0f)
        {
            Time.timeScale = originalTimeScale;
        }

        Time.fixedDeltaTime = originalFixedDeltaTime;
    }

    private void UpdateStatusPanel(GameObject panel, Image fillImage, Text labelText, float timeLeft, float duration, string label, Color fillColor)
    {
        if (panel != null)
        {
            panel.SetActive(timeLeft > 0f);
        }

        if (fillImage != null)
        {
            float safeDuration = Mathf.Max(duration, 0.01f);
            fillImage.fillAmount = Mathf.Clamp01(timeLeft / safeDuration);
            fillImage.color = fillColor;
        }

        if (labelText == null) return;

        labelText.text = label;
    }

    public Sprite GetIcon(SupplementType type)
    {
        switch (type)
        {
            case SupplementType.Protein:
                return proteinIcon;

            case SupplementType.Creatine:
                return creatineIcon;

            case SupplementType.Rice:
                return riceIcon;

            case SupplementType.Testosterone:
                return testosteroneIcon;

            case SupplementType.Vitamin:
                return vitaminIcon;

            default:
                return null;
        }
    }
}
