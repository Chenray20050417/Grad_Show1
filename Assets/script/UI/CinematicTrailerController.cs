using UnityEngine;
using UnityEngine.Video;
using Mediapipe.Tasks.Components.Containers;
using Stopwatch = System.Diagnostics.Stopwatch;

public class CinematicTrailerController : MonoBehaviour
{
    [Header("影片")]
    public VideoClip cinematicClip;

    [Header("播放完前往")]
    public string nextSceneName = "level1";

    [Header("跳過")]
    public bool allowSkip = true;
    public float fistHoldToSkipSeconds = 2f;
    public string skipHandLabel = "Left";
    public SpriteFrameAnimator skipGestureAnimator;
    public GameObject[] skipPromptObjects;
    public float skipPromptDelaySeconds = 10f;
    public float skipPromptFadeSeconds = 0.5f;
    public GameObject[] delayedStartupObjects;
    public float missingHandResetDelay = 0.2f;

    private VideoPlayer videoPlayer;
    private float trailerElapsedSeconds;
    private float skipPromptFadeTimer;
    private float fistHoldTimer;
    private bool isLoadingNextScene;
    private bool skipPromptVisible;
    private bool skipPromptFading;
    private bool delayedStartupEnabled;
    private CanvasGroup[] skipPromptCanvasGroups;
    private bool latestRightHandFist;
    private bool skipHandMissing;
    private bool hasReceivedSkipHand;
    private long lastSkipHandSignalTicks;
    private readonly object handLock = new object();

    private void Start()
    {
        Time.timeScale = 1f;

        Camera targetCamera = Camera.main;

        if (targetCamera == null)
            targetCamera = FindObjectOfType<Camera>();

        if (targetCamera == null)
        {
            Debug.LogError("CinematicTrailer 找不到 Camera");
            LoadNextScene();
            return;
        }

        videoPlayer = targetCamera.gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = targetCamera;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.isLooping = false;
        videoPlayer.clip = cinematicClip;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.loopPointReached += OnTrailerFinished;
        videoPlayer.prepareCompleted += OnTrailerPrepared;

        if (cinematicClip == null)
        {
            Debug.LogError("CinematicTrailer 沒有指定影片");
            LoadNextScene();
            return;
        }

        if (skipGestureAnimator != null)
            skipGestureAnimator.ResetToFirstFrame();

        SetSkipPromptVisible(false);
        SetDelayedStartupObjects(false);

        videoPlayer.Prepare();
    }

    private void Update()
    {
        if (!allowSkip) return;
        if (videoPlayer == null || !videoPlayer.isPlaying) return;

        trailerElapsedSeconds += Time.unscaledDeltaTime;

        if (!skipPromptVisible && trailerElapsedSeconds >= skipPromptDelaySeconds)
        {
            SetSkipPromptVisible(true);
            SetDelayedStartupObjects(true);
        }

        UpdateSkipPromptFade();

        if (!skipPromptVisible)
        {
            ResetFistHold();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetMouseButtonDown(0))
        {
            LoadNextScene();
        }

        bool isFist;
        bool shouldResetMissingHand = false;

        lock (handLock)
        {
            if (skipHandMissing && hasReceivedSkipHand)
            {
                double missingSeconds = (Stopwatch.GetTimestamp() - lastSkipHandSignalTicks) / (double)Stopwatch.Frequency;
                shouldResetMissingHand = missingSeconds >= missingHandResetDelay;
            }

            if (shouldResetMissingHand)
            {
                latestRightHandFist = false;
                skipHandMissing = false;
            }

            isFist = latestRightHandFist;
        }

        if (isFist)
        {
            fistHoldTimer += Time.unscaledDeltaTime;

            if (skipGestureAnimator != null)
                skipGestureAnimator.SetProgress(fistHoldTimer / Mathf.Max(0.01f, fistHoldToSkipSeconds));

            if (fistHoldTimer >= fistHoldToSkipSeconds)
            {
                Debug.Log($"右手握拳 {fistHoldToSkipSeconds:0.#} 秒，跳過前導片");
                LoadNextScene();
            }
        }
        else
        {
            ResetFistHold();
        }
    }

    public void CheckHand(NormalizedLandmarks lm, string handLabel)
    {
        if (!allowSkip || !skipPromptVisible || lm.landmarks == null || lm.landmarks.Count < 21)
        {
            ClearHand();
            return;
        }

        if (handLabel != skipHandLabel)
            return;

        bool isFist = IsFist(lm);

        lock (handLock)
        {
            latestRightHandFist = isFist;
            skipHandMissing = false;
            hasReceivedSkipHand = true;
            lastSkipHandSignalTicks = Stopwatch.GetTimestamp();
        }
    }

    public bool IsSkipHandLabel(string handLabel)
    {
        return handLabel == skipHandLabel;
    }

    public void ClearHand()
    {
        lock (handLock)
        {
            skipHandMissing = true;
        }
    }

    private void ResetFistHold()
    {
        fistHoldTimer = 0f;

        if (skipGestureAnimator != null)
            skipGestureAnimator.ResetToFirstFrame();
    }

    private void SetSkipPromptVisible(bool isVisible)
    {
        skipPromptVisible = isVisible;
        EnsureSkipPromptCanvasGroups();

        if (skipPromptCanvasGroups != null)
        {
            foreach (CanvasGroup canvasGroup in skipPromptCanvasGroups)
            {
                if (canvasGroup == null)
                    continue;

                canvasGroup.gameObject.SetActive(isVisible);
                canvasGroup.alpha = isVisible ? 0f : 0f;
            }
        }

        skipPromptFadeTimer = 0f;
        skipPromptFading = isVisible;
        ResetFistHold();
    }

    private void EnsureSkipPromptCanvasGroups()
    {
        if (skipPromptCanvasGroups != null || skipPromptObjects == null)
            return;

        skipPromptCanvasGroups = new CanvasGroup[skipPromptObjects.Length];

        for (int i = 0; i < skipPromptObjects.Length; i++)
        {
            GameObject promptObject = skipPromptObjects[i];

            if (promptObject == null)
                continue;

            CanvasGroup canvasGroup = promptObject.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = promptObject.AddComponent<CanvasGroup>();

            skipPromptCanvasGroups[i] = canvasGroup;
        }
    }

    private void UpdateSkipPromptFade()
    {
        if (!skipPromptFading)
            return;

        skipPromptFadeTimer += Time.unscaledDeltaTime;
        float alpha = Mathf.Clamp01(skipPromptFadeTimer / Mathf.Max(0.01f, skipPromptFadeSeconds));

        if (skipPromptCanvasGroups != null)
        {
            foreach (CanvasGroup canvasGroup in skipPromptCanvasGroups)
            {
                if (canvasGroup != null)
                    canvasGroup.alpha = alpha;
            }
        }

        if (alpha >= 1f)
            skipPromptFading = false;
    }

    private bool IsFist(NormalizedLandmarks lm)
    {
        var p = lm.landmarks;

        bool indexFold = p[8].y > p[6].y;
        bool middleFold = p[12].y > p[10].y;
        bool ringFold = p[16].y > p[14].y;
        bool pinkyFold = p[20].y > p[18].y;

        int foldCount = 0;

        if (indexFold) foldCount++;
        if (middleFold) foldCount++;
        if (ringFold) foldCount++;
        if (pinkyFold) foldCount++;

        return foldCount >= 3;
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnTrailerFinished;
            videoPlayer.prepareCompleted -= OnTrailerPrepared;
        }
    }

    private void OnTrailerPrepared(VideoPlayer source)
    {
        source.Play();
    }

    private void OnTrailerFinished(VideoPlayer source)
    {
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (isLoadingNextScene) return;

        isLoadingNextScene = true;

        allowSkip = false;
        SetSkipPromptVisible(false);
        SetDelayedStartupObjects(false);

        if (videoPlayer != null)
            videoPlayer.Pause();

        SceneTransitionManager.LoadScene(nextSceneName);
    }

    private void SetDelayedStartupObjects(bool isActive)
    {
        delayedStartupEnabled = isActive;

        if (delayedStartupObjects == null)
            return;

        foreach (GameObject startupObject in delayedStartupObjects)
        {
            if (startupObject != null)
                startupObject.SetActive(isActive);
        }
    }

}
