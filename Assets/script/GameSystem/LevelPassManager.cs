using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class LevelPassManager : MonoBehaviour
{
    [Header("測試")]
    public bool showPassCanvasOnStart = false;
    public GameObject testCanvasOnStart;

    [Header("Canvas")]
    public GameObject passCanvas;
    public GameObject failCanvas;

    [Header("系統")]
    public GameTimer gameTimer;
    public HealthUI healthUI;
    public float trialResultHoldSeconds = 4f;

    private bool resultShown = false;

    void Start()
    {
        if (passCanvas != null)
            passCanvas.SetActive(false);

        if (failCanvas != null)
            failCanvas.SetActive(false);

        if (showPassCanvasOnStart && testCanvasOnStart == null)
            testCanvasOnStart = passCanvas;

        if (testCanvasOnStart != null)
            StartCoroutine(ShowTestCanvasOnStart());
    }

    IEnumerator ShowTestCanvasOnStart()
    {
        yield return null;

        resultShown = true;
        Time.timeScale = 0f;

        if (passCanvas != null)
            passCanvas.SetActive(passCanvas == testCanvasOnStart);

        if (failCanvas != null)
            failCanvas.SetActive(failCanvas == testCanvasOnStart);

        if (testCanvasOnStart != null)
            testCanvasOnStart.SetActive(true);
    }

    void Update()
    {
        if (resultShown) return;
        if (gameTimer == null || healthUI == null) return;

        if (healthUI.IsDead())
        {
            ShowFail();
            Debug.Log("血量為 0，失敗");
            return;
        }

        if (gameTimer.currentTime <= 0)
        {
            if (GameModeState.IsTrialMode)
            {
                ShowTrialResult();
                return;
            }

            resultShown = true;
            Time.timeScale = 0f;

            if (!healthUI.IsDead())
            {
                if (passCanvas != null)
                    passCanvas.SetActive(true);

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayPass();

                Debug.Log("時間到，血量大於 0，過關");
            }
            else
            {
                ShowFail();
                Debug.Log("時間到，血量為 0，失敗");
            }
        }
    }

    private void ShowFail()
    {
        resultShown = true;
        Time.timeScale = 0f;

        if (failCanvas != null)
            failCanvas.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayFail();
    }

    private void ShowTrialResult()
    {
        resultShown = true;
        Time.timeScale = 0f;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPass();

        StartCoroutine(TrialResultRoutine());
    }

    private IEnumerator TrialResultRoutine()
    {
        GameObject panel = CreateTrialResultPanel();

        float timer = 0f;
        while (timer < trialResultHoldSeconds)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (panel != null)
            Destroy(panel);

        Time.timeScale = 1f;
        string returnScene = GameModeState.ReturnSceneName;
        GameModeState.ClearTrialMode();
        SceneTransitionManager.LoadScene(returnScene);
    }

    private GameObject CreateTrialResultPanel()
    {
        GameObject canvasObject = new GameObject("TrialResultCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject overlay = new GameObject("Overlay", typeof(RectTransform), typeof(Image));
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.SetParent(canvasObject.transform, false);
        StretchToParent(overlayRect);
        overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);

        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.SetParent(canvasObject.transform, false);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(980f, 560f);
        panel.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f, 0.94f);

        CreateResultText(panel.transform);
        return canvasObject;
    }

    private void CreateResultText(Transform parent)
    {
        GameObject textObject = new GameObject("ResultText", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(80f, 60f);
        rect.offsetMax = new Vector2(-80f, -60f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = FindTrialResultFont();
        text.fontSize = 54;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.72f, 0f, 1f);
        text.raycastTarget = false;
        text.text =
            "挑戰完成\n\n" +
            "擊破壓力球：" + GameStats.BrokenStressBalls + "\n" +
            "使用補劑：" + GameStats.UsedSupplements + "\n" +
            "受傷次數：" + GameStats.HurtCount + "\n\n" +
            "返回...";
    }

    private TMP_FontAsset FindTrialResultFont()
    {
        TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();

        for (int i = 0; i < fonts.Length; i++)
        {
            if (fonts[i].name.Contains("Cubic"))
                return fonts[i];
        }

        return TMP_Settings.defaultFontAsset;
    }

    private void StretchToParent(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
