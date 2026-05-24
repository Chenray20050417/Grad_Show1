using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    private const float DefaultFadeSeconds = 0.45f;
    private const float DefaultStripeSweepSeconds = 0.55f;

    private static SceneTransitionManager instance;

    private Canvas transitionCanvas;
    private Image transitionBlackImage;
    private RectTransform warningStripe;
    private bool isTransitioning;

    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        Instance.StartTransition(sceneName, -1);
    }

    public static void LoadScene(int sceneBuildIndex)
    {
        Instance.StartTransition(null, sceneBuildIndex);
    }

    private static SceneTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject managerObject = new GameObject("SceneTransitionManager");
                instance = managerObject.AddComponent<SceneTransitionManager>();
                DontDestroyOnLoad(managerObject);
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        BuildTransitionOverlay();
    }

    private void StartTransition(string sceneName, int sceneBuildIndex)
    {
        if (isTransitioning)
            return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTransition();

        Time.timeScale = 1f;
        StartCoroutine(PlayTransitionAndLoadScene(sceneName, sceneBuildIndex));
    }

    private IEnumerator PlayTransitionAndLoadScene(string sceneName, int sceneBuildIndex)
    {
        isTransitioning = true;
        BuildTransitionOverlay();

        transitionCanvas.gameObject.SetActive(true);
        transitionBlackImage.color = new Color(0f, 0f, 0f, 0f);

        float stripeStartX = -1250f;
        float stripeEndX = 1250f;
        warningStripe.anchoredPosition = new Vector2(stripeStartX, 0f);

        float duration = Mathf.Max(DefaultFadeSeconds, DefaultStripeSweepSeconds, 0.01f);
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float fadeProgress = Mathf.Clamp01(timer / DefaultFadeSeconds);
            float sweepProgress = Mathf.Clamp01(timer / DefaultStripeSweepSeconds);
            sweepProgress = Mathf.SmoothStep(0f, 1f, sweepProgress);

            transitionBlackImage.color = new Color(0f, 0f, 0f, fadeProgress);
            warningStripe.anchoredPosition = new Vector2(Mathf.Lerp(stripeStartX, stripeEndX, sweepProgress), 0f);

            yield return null;
        }

        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else if (sceneBuildIndex >= 0)
            SceneManager.LoadScene(sceneBuildIndex);

        yield return null;

        transitionBlackImage.color = new Color(0f, 0f, 0f, 0f);
        transitionCanvas.gameObject.SetActive(false);
        isTransitioning = false;
    }

    private void BuildTransitionOverlay()
    {
        if (transitionCanvas != null)
            return;

        GameObject canvasObject = new GameObject("SceneTransitionCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        transitionCanvas = canvasObject.GetComponent<Canvas>();
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        transitionCanvas.sortingOrder = 5000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject blackObject = new GameObject("BlackFade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        blackObject.transform.SetParent(canvasObject.transform, false);
        RectTransform blackRect = blackObject.GetComponent<RectTransform>();
        blackRect.anchorMin = Vector2.zero;
        blackRect.anchorMax = Vector2.one;
        blackRect.offsetMin = Vector2.zero;
        blackRect.offsetMax = Vector2.zero;
        transitionBlackImage = blackObject.GetComponent<Image>();
        transitionBlackImage.color = new Color(0f, 0f, 0f, 0f);
        transitionBlackImage.raycastTarget = false;

        GameObject stripeObject = new GameObject("WarningStripeSweep", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        stripeObject.transform.SetParent(canvasObject.transform, false);
        warningStripe = stripeObject.GetComponent<RectTransform>();
        warningStripe.anchorMin = new Vector2(0.5f, 0.5f);
        warningStripe.anchorMax = new Vector2(0.5f, 0.5f);
        warningStripe.sizeDelta = new Vector2(420f, 1500f);
        warningStripe.localRotation = Quaternion.Euler(0f, 0f, -18f);
        warningStripe.anchoredPosition = new Vector2(-1250f, 0f);

        Image stripeImage = stripeObject.GetComponent<Image>();
        stripeImage.sprite = CreateWarningStripeSprite();
        stripeImage.type = Image.Type.Tiled;
        stripeImage.raycastTarget = false;

        canvasObject.SetActive(false);
    }

    private Sprite CreateWarningStripeSprite()
    {
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Point;

        Color yellow = new Color(1f, 0.68f, 0f, 1f);
        Color black = new Color(0.02f, 0.02f, 0.02f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int stripe = ((x + y) / 24) % 2;
                texture.SetPixel(x, y, stripe == 0 ? yellow : black);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
