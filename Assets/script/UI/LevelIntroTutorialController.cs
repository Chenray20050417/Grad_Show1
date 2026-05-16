using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using UnityEngine;

public class LevelIntroTutorialController : MonoBehaviour
{
    [Header("顯示")]
    public bool showOnStart = true;
    public CanvasGroup tutorialCanvasGroup;
    public float fadeSeconds = 0.35f;
    public float autoHideSeconds = 5f;

    [Header("跳過")]
    public float doubleFistHoldSeconds = 1.2f;
    public SpriteFrameAnimator skipAnimator;

    private float holdTimer;
    private float fadeTimer;
    private float showTimer;
    private bool isShowing;
    private bool hasTwoFists;

    private void Start()
    {
        if (showOnStart)
            ShowTutorial();
    }

    private void Update()
    {
        if (!isShowing)
            return;

        if (tutorialCanvasGroup != null)
        {
            fadeTimer += Time.unscaledDeltaTime;
            tutorialCanvasGroup.alpha = Mathf.Clamp01(fadeTimer / Mathf.Max(0.01f, fadeSeconds));
        }

        showTimer += Time.unscaledDeltaTime;

        if (autoHideSeconds > 0f && showTimer >= autoHideSeconds)
        {
            HideTutorial();
            return;
        }

        if (hasTwoFists)
        {
            holdTimer += Time.unscaledDeltaTime;

            if (skipAnimator != null)
                skipAnimator.SetProgress(holdTimer / Mathf.Max(0.01f, doubleFistHoldSeconds));

            if (holdTimer >= doubleFistHoldSeconds)
                HideTutorial();
        }
        else
        {
            holdTimer = 0f;

            if (skipAnimator != null)
                skipAnimator.ResetToFirstFrame();
        }
    }

    public void CheckHands(IReadOnlyList<NormalizedLandmarks> hands)
    {
        if (!isShowing || hands == null)
            return;

        int fistCount = 0;

        for (int i = 0; i < hands.Count; i++)
        {
            if (IsFist(hands[i]))
                fistCount++;
        }

        hasTwoFists = fistCount >= 2;
    }

    public void ClearHands()
    {
        hasTwoFists = false;
    }

    private void ShowTutorial()
    {
        Time.timeScale = 0f;
        isShowing = true;
        hasTwoFists = false;
        holdTimer = 0f;
        fadeTimer = 0f;
        showTimer = 0f;

        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.gameObject.SetActive(true);
            tutorialCanvasGroup.alpha = 0f;
        }

        if (skipAnimator != null)
            skipAnimator.ResetToFirstFrame();
    }

    private void HideTutorial()
    {
        isShowing = false;
        hasTwoFists = false;
        Time.timeScale = 1f;

        if (tutorialCanvasGroup != null)
            tutorialCanvasGroup.gameObject.SetActive(false);
    }

    private bool IsFist(NormalizedLandmarks lm)
    {
        if (lm.landmarks == null || lm.landmarks.Count < 21)
            return false;

        var p = lm.landmarks;
        int folded = 0;

        if (p[8].y > p[6].y) folded++;
        if (p[12].y > p[10].y) folded++;
        if (p[16].y > p[14].y) folded++;
        if (p[20].y > p[18].y) folded++;

        return folded >= 3;
    }
}
