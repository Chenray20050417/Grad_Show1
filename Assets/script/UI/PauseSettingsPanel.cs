using UnityEngine;
using UnityEngine.UI;

public class PauseSettingsPanel : MonoBehaviour
{
    public Button soundButton;
    public GameObject pauseCanvas;
    public float buttonStep = 0.1f;

    private GameObject settingsPanel;
    private Slider bgmSlider;
    private Slider sfxSlider;
    private Text bgmValueText;
    private Text sfxValueText;
    private Text muteButtonText;
    private bool isRefreshing;

    private static readonly Color Yellow = new Color(1f, 0.72f, 0f, 1f);
    private static readonly Color Blue = new Color(0.2f, 0.5f, 1f, 1f);
    private static readonly Color PanelGrey = new Color(0.36f, 0.36f, 0.36f, 0.72f);
    private static readonly Color DarkTrack = new Color(0.04f, 0.04f, 0.04f, 1f);

    private void Awake()
    {
        if (soundButton == null)
            soundButton = GetComponent<Button>();

        if (pauseCanvas == null)
        {
            GameObject pauseCanvasObject = GameObject.Find("PauseCanvas");
            if (pauseCanvasObject != null)
                pauseCanvas = pauseCanvasObject;
        }

        BindSoundButton();
    }

    private void OnEnable()
    {
        BindSoundButton();
    }

    public void OpenSettings()
    {
        EnsureSettingsPanel();

        if (settingsPanel == null)
            return;

        settingsPanel.SetActive(true);
        settingsPanel.transform.SetAsLastSibling();
        RefreshUI();
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void ToggleMute()
    {
        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.ToggleMuted();
        RefreshUI();
    }

    public void BgmMinus()
    {
        ChangeBgm(-buttonStep);
    }

    public void BgmPlus()
    {
        ChangeBgm(buttonStep);
    }

    public void SfxMinus()
    {
        ChangeSfx(-buttonStep);
    }

    public void SfxPlus()
    {
        ChangeSfx(buttonStep);
    }

    private void BindSoundButton()
    {
        if (soundButton == null)
            return;

        soundButton.onClick.RemoveListener(OpenSettings);
        soundButton.onClick.AddListener(OpenSettings);
    }

    private void ChangeBgm(float delta)
    {
        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.SetBgmVolume(AudioManager.Instance.bgmVolume + delta);
        RefreshUI();
    }

    private void ChangeSfx(float delta)
    {
        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.SetSfxVolume(AudioManager.Instance.sfxVolume + delta);
        RefreshUI();
    }

    private void OnBgmSliderChanged(float value)
    {
        if (isRefreshing || AudioManager.Instance == null)
            return;

        AudioManager.Instance.SetBgmVolume(value);
        RefreshUI();
    }

    private void OnSfxSliderChanged(float value)
    {
        if (isRefreshing || AudioManager.Instance == null)
            return;

        AudioManager.Instance.SetSfxVolume(value);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (AudioManager.Instance == null)
            return;

        isRefreshing = true;

        if (bgmSlider != null)
            bgmSlider.value = AudioManager.Instance.bgmVolume;

        if (sfxSlider != null)
            sfxSlider.value = AudioManager.Instance.sfxVolume;

        if (bgmValueText != null)
            bgmValueText.text = "BGM  " + Mathf.RoundToInt(AudioManager.Instance.bgmVolume * 100f) + "%";

        if (sfxValueText != null)
            sfxValueText.text = "SFX  " + Mathf.RoundToInt(AudioManager.Instance.sfxVolume * 100f) + "%";

        if (muteButtonText != null)
            muteButtonText.text = AudioManager.Instance.muted ? "解除靜音" : "靜音";

        isRefreshing = false;
    }

    private void EnsureSettingsPanel()
    {
        if (settingsPanel != null)
            return;

        Transform parent = pauseCanvas != null ? pauseCanvas.transform : transform.parent;
        if (parent == null)
            return;

        settingsPanel = CreatePanel(parent);
        CreateBackdrop(settingsPanel.transform);
        CreateText(settingsPanel.transform, "設定", 0f, 280f, 96f, Yellow, 70, TextAnchor.MiddleCenter);

        CreateVolumeRow(settingsPanel.transform, "BGM", 100f, out bgmSlider, out bgmValueText, OnBgmSliderChanged, BgmMinus, BgmPlus);
        CreateVolumeRow(settingsPanel.transform, "SFX", -115f, out sfxSlider, out sfxValueText, OnSfxSliderChanged, SfxMinus, SfxPlus);

        Button muteButton = CreateButton(settingsPanel.transform, "MuteButton", "靜音", -420f, -330f, 260f, 76f, PanelGrey, Yellow);
        muteButtonText = muteButton.GetComponentInChildren<Text>();
        muteButton.onClick.AddListener(ToggleMute);

        Button closeButton = CreateButton(settingsPanel.transform, "SettingsBackButton", "返回", 420f, -330f, 260f, 76f, PanelGrey, Yellow);
        closeButton.onClick.AddListener(CloseSettings);

        settingsPanel.SetActive(false);
    }

    private GameObject CreatePanel(Transform parent)
    {
        GameObject panelObject = new GameObject("PauseSettingsPanel", typeof(RectTransform), typeof(Image));
        RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;

        Image image = panelObject.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.08f);

        return panelObject;
    }

    private void CreateBackdrop(Transform parent)
    {
        GameObject backdropObject = new GameObject("SettingsBackdrop", typeof(RectTransform), typeof(Image));
        RectTransform backdropRect = backdropObject.GetComponent<RectTransform>();
        SetupRect(backdropRect, parent, 0f, 0f, 1120f, 650f);

        Image image = backdropObject.GetComponent<Image>();
        image.color = PanelGrey;
    }

    private void CreateVolumeRow(
        Transform parent,
        string label,
        float y,
        out Slider slider,
        out Text valueText,
        UnityEngine.Events.UnityAction<float> onSliderChanged,
        UnityEngine.Events.UnityAction minusAction,
        UnityEngine.Events.UnityAction plusAction)
    {
        CreateButton(parent, label + "MinusButton", "-", -520f, y, 90f, 70f, Blue, Yellow).onClick.AddListener(minusAction);

        slider = CreateSlider(parent, label + "Slider", 0f, y, 560f, 46f);
        slider.onValueChanged.AddListener(onSliderChanged);

        CreateButton(parent, label + "PlusButton", "+", 520f, y, 90f, 70f, Yellow, Blue).onClick.AddListener(plusAction);
        valueText = CreateText(parent, label + "  100%", 0f, y - 82f, 360f, Yellow, 36, TextAnchor.MiddleCenter);
    }

    private Slider CreateSlider(Transform parent, string objectName, float x, float y, float width, float height)
    {
        GameObject sliderObject = new GameObject(objectName, typeof(RectTransform), typeof(Slider));
        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        SetupRect(sliderRect, parent, x, y, width, height);

        GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        SetupStretch(backgroundRect, sliderRect, 0f, 0f);
        Image backgroundImage = backgroundObject.GetComponent<Image>();
        backgroundImage.color = DarkTrack;

        GameObject fillAreaObject = new GameObject("Fill Area", typeof(RectTransform));
        RectTransform fillAreaRect = fillAreaObject.GetComponent<RectTransform>();
        SetupStretch(fillAreaRect, sliderRect, 0f, 0f);

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        SetupStretch(fillRect, fillAreaRect, 0f, 0f);
        Image fillImage = fillObject.GetComponent<Image>();
        fillImage.color = Yellow;

        GameObject handleAreaObject = new GameObject("Handle Slide Area", typeof(RectTransform));
        RectTransform handleAreaRect = handleAreaObject.GetComponent<RectTransform>();
        SetupStretch(handleAreaRect, sliderRect, 0f, 0f);

        GameObject handleObject = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        SetupRect(handleRect, handleAreaRect, 0f, 0f, 34f, 70f);
        Image handleImage = handleObject.GetComponent<Image>();
        handleImage.color = Blue;

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.targetGraphic = handleImage;
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private Button CreateButton(Transform parent, string objectName, string text, float x, float y, float width, float height, Color backgroundColor, Color textColor)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        SetupRect(buttonRect, parent, x, y, width, height);

        Image image = buttonObject.GetComponent<Image>();
        image.color = backgroundColor;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        CreateText(buttonObject.transform, text, 0f, 0f, width, textColor, Mathf.RoundToInt(height * 0.58f), TextAnchor.MiddleCenter);

        return button;
    }

    private Text CreateText(Transform parent, string text, float x, float y, float width, Color color, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        SetupRect(textRect, parent, x, y, width, fontSize * 1.5f);

        Text textComponent = textObject.GetComponent<Text>();
        textComponent.text = text;
        textComponent.color = color;
        textComponent.fontSize = fontSize;
        textComponent.alignment = alignment;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return textComponent;
    }

    private void SetupRect(RectTransform rectTransform, Transform parent, float x, float y, float width, float height)
    {
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(x, y);
        rectTransform.sizeDelta = new Vector2(width, height);
    }

    private void SetupStretch(RectTransform rectTransform, Transform parent, float horizontalPadding, float verticalPadding)
    {
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = new Vector2(horizontalPadding, verticalPadding);
        rectTransform.offsetMax = new Vector2(-horizontalPadding, -verticalPadding);
    }
}
