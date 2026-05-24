using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartTrainingIntroController : MonoBehaviour
{
    [Header("流程")]
    public string cinematicSceneName = "CinematicTrailer";
    public float introSeconds = 2f;

    [Header("畫面")]
    public CanvasGroup overlayGroup;
    public Image warningFlashImage;
    public TMP_Text statusText;

    private bool isPlaying;

    public void StartTraining()
    {
        if (isPlaying)
            return;

        StartCoroutine(StartRoutine());
    }

    private IEnumerator StartRoutine()
    {
        isPlaying = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTransition();
        }

        if (overlayGroup != null)
        {
            overlayGroup.gameObject.SetActive(true);
            overlayGroup.alpha = 0f;
        }

        if (warningFlashImage != null)
            warningFlashImage.color = new Color(1f, 0.72f, 0f, 0f);

        yield return FadeOverlay(0.35f, 0f, 0.72f);
        yield return FlashWarning();
        yield return TypeStatus("TRAINING SYSTEM START");
        yield return Wait(Mathf.Max(0f, introSeconds - 1.35f));

        SceneTransitionManager.LoadScene(cinematicSceneName);
    }

    private IEnumerator FadeOverlay(float duration, float from, float to)
    {
        if (overlayGroup == null)
            yield break;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / Mathf.Max(0.01f, duration));
            overlayGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
    }

    private IEnumerator FlashWarning()
    {
        if (warningFlashImage == null)
            yield break;

        for (int i = 0; i < 2; i++)
        {
            warningFlashImage.color = new Color(1f, 0.72f, 0f, 0.35f);
            yield return Wait(0.12f);
            warningFlashImage.color = new Color(1f, 0.72f, 0f, 0f);
            yield return Wait(0.12f);
        }
    }

    private IEnumerator TypeStatus(string content)
    {
        if (statusText == null)
            yield break;

        statusText.text = "";

        for (int i = 0; i < content.Length; i++)
        {
            statusText.text += content[i];
            yield return Wait(0.035f);
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
}
