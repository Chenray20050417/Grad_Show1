using System.Collections;
using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("時間設定")]
    public float totalTime = 90f;

    [Header("目前時間")]
    public float currentTime;

    [Header("UI")]
    public TMP_Text timerText;
    public RectTransform timerPanel;

    [Header("顏色")]
    public Color normalColor = Color.white;
    public Color warningColor = new Color(1f, 0.5f, 0f);
    public Color dangerColor = Color.red;

    [Header("震動")]
    public float shakeDuration = 0.4f;
    public float baseShakeStrength = 5f;

    private bool isRunning = true;

    private Vector3 originalPos;
    private int lastShakeSecond = -1;

    void Start()
    {
        currentTime = totalTime;

        if (timerPanel != null)
            originalPos = timerPanel.localPosition;

        UpdateTimerUI();
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            isRunning = false;

            Debug.Log("時間到！");
        }

        UpdateTimerUI();
        CheckShake();
    }

    void UpdateTimerUI()
    {
        int minute = Mathf.FloorToInt(currentTime / 60);
        int second = Mathf.FloorToInt(currentTime % 60);

        if (timerText != null)
            timerText.text = minute.ToString("00") + ":" + second.ToString("00");

        if (timerText == null) return;

        if (currentTime <= 10)
            timerText.color = dangerColor;
        else if (currentTime <= 30)
            timerText.color = warningColor;
        else
            timerText.color = normalColor;
    }

    void CheckShake()
    {
        if (timerPanel == null) return;

        int currentSecond = Mathf.CeilToInt(currentTime);

        if (currentSecond % 10 == 0 && currentSecond != lastShakeSecond)
        {
            lastShakeSecond = currentSecond;

            float progress = 1f - (currentTime / totalTime);
            float shakePower = baseShakeStrength + (progress * 20f);

            StartCoroutine(Shake(shakePower));
        }
    }

    IEnumerator Shake(float strength)
    {
        float timer = 0f;

        while (timer < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;

            timerPanel.localPosition =
                originalPos + new Vector3(x, y, 0);

            timer += Time.deltaTime;

            yield return null;
        }

        timerPanel.localPosition = originalPos;
    }
}