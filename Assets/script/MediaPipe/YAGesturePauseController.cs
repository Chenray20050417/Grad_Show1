using UnityEngine;
using UnityEngine.SceneManagement;
using Mediapipe.Tasks.Components.Containers;

public class YAGesturePauseController : MonoBehaviour
{
    public PauseManager pauseManager;
    public float holdTime = 1f;
    public float cooldown = 2f;
    public int gestureGraceFrames = 6;
    public string pauseHandLabel = "Left";
    public bool debugLog;

    private float lastTriggerTime = -999f;
    private volatile bool receivedYaGesture = false;
    private float holdTimer = 0f;
    private int framesSinceYaGesture = 999;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        ResolvePauseManager();
        ResetGestureState();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResolvePauseManager();
        ResetGestureState();
    }

    public void CheckHand(NormalizedLandmarks lm)
    {
        CheckHand(lm, pauseHandLabel);
    }

    public void CheckHand(NormalizedLandmarks lm, string handLabel)
    {
        if (lm.landmarks == null || lm.landmarks.Count < 21)
            return;

        if (!string.IsNullOrEmpty(pauseHandLabel) && handLabel != pauseHandLabel)
            return;

        bool indexUp = lm.landmarks[8].y < lm.landmarks[6].y;
        bool middleUp = lm.landmarks[12].y < lm.landmarks[10].y;
        bool ringDown = lm.landmarks[16].y > lm.landmarks[14].y;
        bool pinkyDown = lm.landmarks[20].y > lm.landmarks[18].y;

        bool isYaGesture = indexUp && middleUp && ringDown && pinkyDown;

        if (debugLog && isYaGesture)
        {
            Debug.Log("YA 手勢偵測中");
        }

        if (isYaGesture)
        {
            receivedYaGesture = true;
        }
    }

    void Update()
    {
        ResolvePauseManager();

        if (receivedYaGesture)
        {
            framesSinceYaGesture = 0;
        }
        else
        {
            framesSinceYaGesture++;
        }

        receivedYaGesture = false;

        if (pauseManager != null && pauseManager.IsPaused)
        {
            holdTimer = 0f;
            return;
        }

        bool hasRecentYaGesture = framesSinceYaGesture <= gestureGraceFrames;

        if (!hasRecentYaGesture || Time.unscaledTime - lastTriggerTime <= cooldown)
        {
            holdTimer = 0f;
            return;
        }

        holdTimer += Time.unscaledDeltaTime;
        if (holdTimer < holdTime) return;

        holdTimer = 0f;
        lastTriggerTime = Time.unscaledTime;

        if (pauseManager != null)
        {
            Debug.Log("YA 暫停觸發");
            pauseManager.ShowPause();
        }
        else
        {
            Debug.LogWarning("YA 暫停失敗：找不到 PauseManager");
        }
    }

    private void ResolvePauseManager()
    {
        if (pauseManager != null) return;

        pauseManager = FindObjectOfType<PauseManager>();
    }

    private void ResetGestureState()
    {
        lastTriggerTime = -999f;
        receivedYaGesture = false;
        holdTimer = 0f;
        framesSinceYaGesture = 999;
    }
}
