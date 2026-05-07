using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthUI : MonoBehaviour
{
    [Header("血條")]
    public Slider healthSlider;

    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("UI震動")]
    public RectTransform healthPanel;

    public float shakeDuration = 0.2f;
    public float shakeStrength = 10f;

    private Vector3 originalPos;

    void Start()
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        if (healthPanel != null)
        {
            originalPos = healthPanel.localPosition;
        }
    }

    void Update()
    {
        // H 扣血測試
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10);
        }

        // J 補血測試
        if (Input.GetKeyDown(KeyCode.J))
        {
            Heal(10);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        currentHealth =
            Mathf.Clamp(currentHealth, 0, maxHealth);

        healthSlider.value = currentHealth;

        // UI抖動
        if (healthPanel != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShakeRoutine());
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;

        currentHealth =
            Mathf.Clamp(currentHealth, 0, maxHealth);

        healthSlider.value = currentHealth;
    }

    IEnumerator ShakeRoutine()
    {
        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;

            float x =
                Random.Range(-shakeStrength, shakeStrength);

            float y =
                Random.Range(-shakeStrength, shakeStrength);

            healthPanel.localPosition =
                originalPos + new Vector3(x, y, 0);

            yield return null;
        }

        healthPanel.localPosition = originalPos;
    }
}