using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthUI : MonoBehaviour
{
    public static HealthUI Instance;

    [Header("血條")]
    public Slider healthSlider;

    public float maxHealth = 100f;
    public float startHealth = 100f;
    public float currentHealth;

    [Header("UI震動")]
    public RectTransform healthPanel;
    public float shakeDuration = 0.2f;
    public float shakeStrength = 10f;

    private Vector3 originalPos;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentHealth = Mathf.Clamp(startHealth, 0, maxHealth);

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthPanel != null)
            originalPos = healthPanel.localPosition;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            TakeDamage(10);

        if (Input.GetKeyDown(KeyCode.J))
            Heal(10);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        if (healthPanel != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShakeRoutine());
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        Debug.Log("回血：" + amount);
    }

    public void HealPercent(float percent)
    {
        float healAmount = maxHealth * percent;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        Debug.Log("回血：" + healAmount);
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;
    }

    IEnumerator ShakeRoutine()
    {
        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float x = Random.Range(-shakeStrength, shakeStrength);
            float y = Random.Range(-shakeStrength, shakeStrength);

            healthPanel.localPosition =
                originalPos + new Vector3(x, y, 0);

            yield return null;
        }

        healthPanel.localPosition = originalPos;
    }
}