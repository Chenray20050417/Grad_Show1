using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComicEndingSequence : MonoBehaviour
{
    [Header("漫畫")]
    public RectTransform comicImage;
    public float comicHoldSeconds = 1.2f;
    public float shrinkSeconds = 1.5f;
    public float endScale = 0.08f;

    [Header("黑場")]
    public CanvasGroup endingCanvasGroup;
    public Image blackImage;
    public float blackFadeSeconds = 0.8f;

    [Header("結算")]
    public TMP_Text resultText;
    public Text resultTextLegacy;
    public bool useGameStats = true;
    public int brokenStressBalls;
    public int usedSupplements;
    public int hurtCount;
    public float resultHoldSeconds = 2.2f;

    [Header("排行榜")]
    public bool saveToLocalLeaderboard = true;
    public int maxPlayerNameLength = 12;

    [Header("工廠系統關機")]
    public TMP_Text systemText;
    public Text systemTextLegacy;
    public TMP_Text thanksText;
    public Text thanksTextLegacy;
    public float typeInterval = 0.055f;
    public float systemHoldSeconds = 1.2f;
    public float thanksHoldSeconds = 1.4f;
    public string mainMenuSceneName = "MainMenu";

    private bool isPlaying;
    private bool nameSubmitted;
    private string playerNameInput = "";

    public void PlayEnding(RectTransform targetComicImage = null)
    {
        if (isPlaying)
            return;

        if (targetComicImage != null)
            comicImage = targetComicImage;

        if (!HasRequiredManualCanvas())
            return;

        StartCoroutine(EndingRoutine());
    }

    private IEnumerator EndingRoutine()
    {
        isPlaying = true;

        if (endingCanvasGroup != null)
        {
            endingCanvasGroup.gameObject.SetActive(true);
            endingCanvasGroup.alpha = 0f;
        }

        SetText(resultText, resultTextLegacy, string.Empty);
        SetText(systemText, systemTextLegacy, string.Empty);
        SetText(thanksText, thanksTextLegacy, string.Empty);
        SetBlackAlpha(0f);

        yield return Wait(comicHoldSeconds);

        if (comicImage != null)
            yield return ShrinkComic();

        yield return FadeBlack();

        int brokenCount = useGameStats ? GameStats.BrokenStressBalls : brokenStressBalls;
        int supplementCount = useGameStats ? GameStats.UsedSupplements : usedSupplements;
        int damageCount = useGameStats ? GameStats.HurtCount : hurtCount;

        SetText(resultText, resultTextLegacy,
            "挑戰完成\n\n" +
            "擊破壓力球：" + brokenCount + "\n" +
            "使用補劑：" + supplementCount + "\n" +
            "受傷次數：" + damageCount);

        yield return Wait(resultHoldSeconds);

        if (saveToLocalLeaderboard)
            yield return AskPlayerNameAndSave(brokenCount, supplementCount, damageCount);

        SetText(resultText, resultTextLegacy, string.Empty);
        yield return TypeText(systemText, systemTextLegacy,
            "SWEAT FACTORY SYSTEM\n" +
            "TRAINING COMPLETE\n" +
            "SHUTTING DOWN...");

        yield return Wait(systemHoldSeconds);

        SetText(systemText, systemTextLegacy, string.Empty);
        yield return TypeText(thanksText, thanksTextLegacy, "感謝遊玩");

        yield return Wait(thanksHoldSeconds);
        ReturnToMainMenu();
    }

    private IEnumerator AskPlayerNameAndSave(int brokenCount, int supplementCount, int damageCount)
    {
        nameSubmitted = false;
        playerNameInput = "";

        while (!nameSubmitted)
        {
            HandleNameInput();

            string visibleName = string.IsNullOrEmpty(playerNameInput) ? "_" : playerNameInput + "_";
            int score = LocalLeaderboard.CalculateScore(brokenCount, supplementCount, damageCount);

            SetText(resultText, resultTextLegacy,
                "輸入玩家名稱\n\n" +
                visibleName + "\n\n" +
                "分數：" + score + "\n" +
                "按 Enter 儲存");

            yield return null;
        }

        LocalLeaderboard.AddEntry(playerNameInput, brokenCount, supplementCount, damageCount);

        SetText(resultText, resultTextLegacy,
            "成績已儲存\n\n" +
            "玩家：" + CleanPlayerName(playerNameInput));

        yield return Wait(1f);
    }

    private void HandleNameInput()
    {
        string input = Input.inputString;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '\b')
            {
                if (playerNameInput.Length > 0)
                    playerNameInput = playerNameInput.Substring(0, playerNameInput.Length - 1);

                continue;
            }

            if (c == '\n' || c == '\r')
            {
                nameSubmitted = true;
                continue;
            }

            if (playerNameInput.Length >= maxPlayerNameLength)
                continue;

            if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
                playerNameInput += char.ToUpperInvariant(c);
        }
    }

    private IEnumerator ShrinkComic()
    {
        Vector2 startPosition = comicImage.anchoredPosition;
        Vector3 startScale = comicImage.localScale;
        Image comicGraphic = comicImage.GetComponent<Image>();
        Color startColor = comicGraphic != null ? comicGraphic.color : Color.white;
        Color endColor = new Color(0.82f, 0.82f, 0.82f, startColor.a);

        float timer = 0f;

        while (timer < shrinkSeconds)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / Mathf.Max(0.01f, shrinkSeconds));
            t = Mathf.SmoothStep(0f, 1f, t);

            comicImage.anchoredPosition = Vector2.Lerp(startPosition, Vector2.zero, t);
            comicImage.localScale = Vector3.Lerp(startScale, Vector3.one * endScale, t);

            if (comicGraphic != null)
                comicGraphic.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }
    }

    private IEnumerator FadeBlack()
    {
        float timer = 0f;

        while (timer < blackFadeSeconds)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / Mathf.Max(0.01f, blackFadeSeconds));

            if (endingCanvasGroup != null)
                endingCanvasGroup.alpha = t;

            SetBlackAlpha(t);
            yield return null;
        }
    }

    private IEnumerator TypeText(TMP_Text target, Text legacyTarget, string content)
    {
        if (target == null && legacyTarget == null)
            yield break;

        SetText(target, legacyTarget, string.Empty);

        for (int i = 0; i < content.Length; i++)
        {
            string current = GetText(target, legacyTarget) + content[i];
            SetText(target, legacyTarget, current);
            yield return Wait(typeInterval);
        }
    }

    private IEnumerator Wait(float seconds)
    {
        float timer = 0f;

        while (timer < seconds)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private bool HasRequiredManualCanvas()
    {
        AutoBindManualCanvas();

        if (endingCanvasGroup != null &&
            blackImage != null &&
            HasText(resultText, resultTextLegacy) &&
            HasText(systemText, systemTextLegacy) &&
            HasText(thanksText, thanksTextLegacy))
        {
            return true;
        }

        Debug.LogError(
            "ComicEndingSequence 缺少手動 Canvas 欄位，請拖入 Ending Canvas Group、Black Image、Result Text、System Text、Thanks Text。");
        return false;
    }

    private void AutoBindManualCanvas()
    {
        if (endingCanvasGroup == null)
        {
            GameObject endingCanvas = FindSceneObject("EndingCanvas");
            if (endingCanvas != null)
                endingCanvasGroup = endingCanvas.GetComponent<CanvasGroup>();
        }

        if (blackImage == null)
        {
            GameObject blackObject = FindSceneObject("BlackImage");
            if (blackObject != null)
                blackImage = blackObject.GetComponent<Image>();
        }

        AutoBindText("ResultText", ref resultText, ref resultTextLegacy);
        AutoBindText("SystemText", ref systemText, ref systemTextLegacy);
        AutoBindText("ThanksText", ref thanksText, ref thanksTextLegacy);
    }

    private void AutoBindText(string objectName, ref TMP_Text tmpText, ref Text legacyText)
    {
        if (tmpText != null || legacyText != null)
            return;

        GameObject textObject = FindSceneObject(objectName);
        if (textObject == null)
            return;

        tmpText = textObject.GetComponent<TMP_Text>();
        legacyText = textObject.GetComponent<Text>();
    }

    private GameObject FindSceneObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();

        for (int i = 0; i < objects.Length; i++)
        {
            GameObject obj = objects[i];
            if (obj.name == objectName && obj.scene.IsValid())
                return obj;
        }

        return null;
    }

    private void SetBlackAlpha(float alpha)
    {
        if (blackImage == null)
            return;

        blackImage.color = new Color(0f, 0f, 0f, alpha);
    }

    private bool HasText(TMP_Text target, Text legacyTarget)
    {
        return target != null || legacyTarget != null;
    }

    private string GetText(TMP_Text target, Text legacyTarget)
    {
        if (target != null)
            return target.text;

        if (legacyTarget != null)
            return legacyTarget.text;

        return string.Empty;
    }

    private void SetText(TMP_Text target, Text legacyTarget, string content)
    {
        if (target != null)
            target.text = content;

        if (legacyTarget != null)
            legacyTarget.text = content;
    }

    private string CleanPlayerName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "PLAYER";

        return value.Trim();
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneTransitionManager.LoadScene(mainMenuSceneName);
    }
}
