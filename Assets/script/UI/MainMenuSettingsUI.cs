using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuSettingsUI : MonoBehaviour
{
    [Header("Open / Close")]
    public Button settingButton;
    public GameObject settingsPanel;
    public Button closeButton;
    public RectTransform fingerCursor;
    public CanvasGroup buttonGroupCanvasGroup;

    [Header("Volume Controls")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Button bgmMinusButton;
    public Button bgmPlusButton;
    public Button sfxMinusButton;
    public Button sfxPlusButton;
    public Button muteButton;

    [Header("TMP Text")]
    public TMP_Text bgmValueText;
    public TMP_Text sfxValueText;
    public TMP_Text muteButtonText;

    [Header("Settings")]
    public float buttonStep = 0.1f;
    public bool hidePanelOnStart = true;
    public bool disableButtonGroupWhileOpen = true;

    private bool isRefreshing;

    private void Awake()
    {
        AutoFindSettingButton();
        AutoFindButtonGroup();
        BindButtons();
        BindSliders();
    }

    private void Start()
    {
        RefreshUI();

        if (hidePanelOnStart && settingsPanel != null)
            settingsPanel.SetActive(false);

        SetButtonGroupBlocked(false);
    }

    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("Settings Panel 沒有指定");
            return;
        }

        settingsPanel.SetActive(true);
        settingsPanel.transform.SetAsLastSibling();
        SetButtonGroupBlocked(true);
        BringFingerCursorToFront();
        RefreshUI();
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        SetButtonGroupBlocked(false);
    }

    private void OnDisable()
    {
        SetButtonGroupBlocked(false);
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
            bgmValueText.text = "BGM " + Mathf.RoundToInt(AudioManager.Instance.bgmVolume * 100f) + "%";

        if (sfxValueText != null)
            sfxValueText.text = "SFX " + Mathf.RoundToInt(AudioManager.Instance.sfxVolume * 100f) + "%";

        if (muteButtonText != null)
            muteButtonText.text = AudioManager.Instance.muted ? "解除靜音" : "靜音";

        isRefreshing = false;
    }

    private void AutoFindSettingButton()
    {
        if (settingButton != null)
            return;

        GameObject obj = GameObject.Find("settingbtn ");
        if (obj == null)
            obj = GameObject.Find("settingbtn");

        if (obj != null)
            settingButton = obj.GetComponent<Button>();

        if (fingerCursor == null)
        {
            GameObject cursorObject = GameObject.Find("FingerCursor");
            if (cursorObject != null)
                fingerCursor = cursorObject.GetComponent<RectTransform>();
        }
    }

    private void AutoFindButtonGroup()
    {
        if (buttonGroupCanvasGroup != null)
            return;

        GameObject buttonGroup = GameObject.Find("ButtonGroup");
        if (buttonGroup == null)
            return;

        buttonGroupCanvasGroup = buttonGroup.GetComponent<CanvasGroup>();
        if (buttonGroupCanvasGroup == null)
            buttonGroupCanvasGroup = buttonGroup.AddComponent<CanvasGroup>();
    }

    private void SetButtonGroupBlocked(bool blocked)
    {
        if (!disableButtonGroupWhileOpen)
            return;

        if (buttonGroupCanvasGroup == null)
            AutoFindButtonGroup();

        if (buttonGroupCanvasGroup == null)
            return;

        buttonGroupCanvasGroup.interactable = !blocked;
        buttonGroupCanvasGroup.blocksRaycasts = !blocked;
    }

    private void BringFingerCursorToFront()
    {
        if (fingerCursor == null)
        {
            GameObject cursorObject = GameObject.Find("FingerCursor");
            if (cursorObject != null)
                fingerCursor = cursorObject.GetComponent<RectTransform>();
        }

        if (fingerCursor != null)
            fingerCursor.SetAsLastSibling();
    }

    private void BindButtons()
    {
        if (settingButton != null)
        {
            settingButton.onClick.RemoveListener(OpenSettings);
            settingButton.onClick.AddListener(OpenSettings);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseSettings);
            closeButton.onClick.AddListener(CloseSettings);
        }

        if (muteButton != null)
        {
            muteButton.onClick.RemoveListener(ToggleMute);
            muteButton.onClick.AddListener(ToggleMute);
        }

        if (bgmMinusButton != null)
        {
            bgmMinusButton.onClick.RemoveListener(BgmMinus);
            bgmMinusButton.onClick.AddListener(BgmMinus);
        }

        if (bgmPlusButton != null)
        {
            bgmPlusButton.onClick.RemoveListener(BgmPlus);
            bgmPlusButton.onClick.AddListener(BgmPlus);
        }

        if (sfxMinusButton != null)
        {
            sfxMinusButton.onClick.RemoveListener(SfxMinus);
            sfxMinusButton.onClick.AddListener(SfxMinus);
        }

        if (sfxPlusButton != null)
        {
            sfxPlusButton.onClick.RemoveListener(SfxPlus);
            sfxPlusButton.onClick.AddListener(SfxPlus);
        }
    }

    private void BindSliders()
    {
        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0f;
            bgmSlider.maxValue = 1f;
            bgmSlider.onValueChanged.RemoveListener(OnBgmSliderChanged);
            bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }
    }
}
