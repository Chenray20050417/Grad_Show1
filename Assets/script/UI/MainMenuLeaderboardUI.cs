using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuLeaderboardUI : MonoBehaviour
{
    [Header("實體 UI")]
    public Button leaderboardButton;
    public GameObject leaderboardPanel;
    public TMP_Text titleText;
    public TMP_Text headerText;
    public TMP_Text rowsText;
    public Button closeButton;

    [Header("捲動")]
    public float rowHeight = 42f;
    public float scrollWheelSensitivity = 35f;

    [Header("顏色")]
    public Color panelColor = new Color(0.02f, 0.02f, 0.02f, 0.92f);
    public Color yellow = new Color(1f, 0.72f, 0f, 1f);
    public Color blue = new Color(0.25f, 0.5f, 1f, 1f);

    private void Start()
    {
        AutoBind();
        EnsureScrollView();
        WireLeaderboardButton();
        WireCloseButton();

        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);
    }

    public void OpenLeaderboard()
    {
        AutoBind();
        EnsureScrollView();
        RefreshLeaderboardText();

        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(true);
    }

    public void CloseLeaderboard()
    {
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);
    }

    private void AutoBind()
    {
        if (leaderboardPanel == null)
            leaderboardPanel = FindSceneObject("LeaderboardPanel");

        if (titleText == null)
            titleText = FindTMP("LeaderboardTitleText");

        if (headerText == null)
            headerText = FindTMP("LeaderboardHeaderText");

        if (rowsText == null)
            rowsText = FindTMP("LeaderboardRowsText");

        if (closeButton == null)
        {
            GameObject closeObject = FindSceneObject("LeaderboardCloseButton");
            if (closeObject != null)
                closeButton = closeObject.GetComponent<Button>();
        }
    }

    private void WireLeaderboardButton()
    {
        if (leaderboardButton == null)
        {
            GameObject buttonObject = GameObject.Find("rankingbtn");
            if (buttonObject != null)
                leaderboardButton = buttonObject.GetComponent<Button>();
        }

        if (leaderboardButton == null)
        {
            Debug.LogWarning("找不到 rankingbtn，排行榜按鈕沒有接上。");
            return;
        }

        leaderboardButton.onClick.RemoveListener(OpenLeaderboard);
        leaderboardButton.onClick.AddListener(OpenLeaderboard);
    }

    private void WireCloseButton()
    {
        if (closeButton == null)
            return;

        closeButton.onClick.RemoveListener(CloseLeaderboard);
        closeButton.onClick.AddListener(CloseLeaderboard);
    }

    private void RefreshLeaderboardText()
    {
        List<LocalLeaderboard.Entry> entries = LocalLeaderboard.GetEntries();

        if (titleText != null)
            titleText.text = "排行榜";

        if (headerText != null)
            headerText.text = FormatLeaderboardRow("排名", "名稱", "分數", "擊破", "補劑", "受傷");

        string content = "";

        if (entries.Count == 0)
        {
            content = "尚無紀錄";
        }
        else
        {
            for (int i = 0; i < entries.Count; i++)
            {
                LocalLeaderboard.Entry e = entries[i];
                content += FormatLeaderboardRow(
                    (i + 1).ToString("00"),
                    e.playerName,
                    e.score.ToString(),
                    e.brokenStressBalls.ToString(),
                    e.usedSupplements.ToString(),
                    e.hurtCount.ToString()) + "\n";
            }
        }

        if (rowsText != null)
        {
            rowsText.text = content;
            ResizeRowsText(entries.Count);
        }
    }

    private string FormatLeaderboardRow(string rank, string playerName, string score, string broken, string supplements, string hurt)
    {
        return string.Format(
            "{0}<pos=12%>{1}<pos=49%>{2}<pos=63%>{3}<pos=76%>{4}<pos=89%>{5}",
            rank,
            playerName,
            score,
            broken,
            supplements,
            hurt);
    }

    private void EnsureScrollView()
    {
        if (rowsText == null)
            return;

        ScrollRect existingScrollRect = rowsText.GetComponentInParent<ScrollRect>();
        if (existingScrollRect != null && existingScrollRect.content == rowsText.rectTransform)
            return;

        RectTransform rowsRect = rowsText.rectTransform;
        Transform originalParent = rowsRect.parent;

        if (originalParent == null)
            return;

        int siblingIndex = rowsRect.GetSiblingIndex();
        Vector2 anchorMin = rowsRect.anchorMin;
        Vector2 anchorMax = rowsRect.anchorMax;
        Vector2 anchoredPosition = rowsRect.anchoredPosition;
        Vector2 sizeDelta = rowsRect.sizeDelta;
        Vector2 pivot = rowsRect.pivot;

        GameObject scrollObject = new GameObject("LeaderboardScrollView", typeof(RectTransform), typeof(ScrollRect));
        RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
        scrollRect.SetParent(originalParent, false);
        scrollRect.SetSiblingIndex(siblingIndex);
        scrollRect.anchorMin = anchorMin;
        scrollRect.anchorMax = anchorMax;
        scrollRect.anchoredPosition = anchoredPosition;
        scrollRect.sizeDelta = sizeDelta;
        scrollRect.pivot = pivot;

        GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.SetParent(scrollRect, false);
        StretchToParent(viewportRect);

        Image viewportImage = viewportObject.GetComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);

        Mask viewportMask = viewportObject.GetComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        rowsRect.SetParent(viewportRect, false);
        rowsRect.anchorMin = new Vector2(0f, 1f);
        rowsRect.anchorMax = new Vector2(1f, 1f);
        rowsRect.pivot = new Vector2(0.5f, 1f);
        rowsRect.anchoredPosition = Vector2.zero;
        rowsRect.sizeDelta = new Vector2(0f, sizeDelta.y);

        rowsText.enableWordWrapping = false;
        rowsText.overflowMode = TextOverflowModes.Overflow;
        rowsText.alignment = TextAlignmentOptions.TopLeft;
        rowsText.raycastTarget = true;

        ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
        scroll.viewport = viewportRect;
        scroll.content = rowsRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.inertia = true;
        scroll.scrollSensitivity = scrollWheelSensitivity;
    }

    private void ResizeRowsText(int entryCount)
    {
        RectTransform rowsRect = rowsText.rectTransform;
        RectTransform viewportRect = rowsRect.parent as RectTransform;
        float viewportHeight = viewportRect != null ? viewportRect.rect.height : rowsRect.rect.height;
        float targetHeight = Mathf.Max(viewportHeight, Mathf.Max(1, entryCount) * rowHeight);

        rowsRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);

        ScrollRect scroll = rowsText.GetComponentInParent<ScrollRect>();
        if (scroll != null)
            scroll.verticalNormalizedPosition = 1f;
    }

    private void StretchToParent(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
    }

    private TMP_Text FindTMP(string objectName)
    {
        GameObject obj = FindSceneObject(objectName);
        return obj != null ? obj.GetComponent<TMP_Text>() : null;
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
}
