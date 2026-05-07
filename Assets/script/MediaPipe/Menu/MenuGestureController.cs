using UnityEngine;
using Mediapipe.Tasks.Components.Containers;

public class MenuGestureController : MonoBehaviour
{
    [Header("背景")]
    public RectTransform menuBackground;
    public float moveRangeX = 60f;
    public float moveRangeY = 30f;
    public float smoothSpeed = 5f;

    [Header("Canvas")]
    public Canvas canvas;

    [Header("手部游標")]
    public RectTransform fingerCursor;
    public bool mirrorX = true;

    private Vector3 originalPos;

    private float targetX;
    private float targetY;

    private float handX = 0.5f;
    private float handY = 0.5f;
    private bool hasHand = false;

    private readonly object poseLock = new object();

    void Start()
    {
        if (menuBackground != null)
            originalPos = menuBackground.localPosition;
    }

    void Update()
    {
        UpdateBackground();
        UpdateHandCursor();
    }

    public void CheckPose(NormalizedLandmark[] lm)
    {
        if (lm == null || lm.Length < 17) return;

        // 鼻子 = 0
        float noseX = lm[0].x;
        float noseY = lm[0].y;

        float bgX = -(noseX - 0.5f) * moveRangeX * 2f;
        float bgY = (noseY - 0.5f) * moveRangeY * 2f;

        // 右手腕 = 16
        float x = lm[16].x;
        float y = lm[16].y;

        if (mirrorX)
            x = 1f - x;

        lock (poseLock)
        {
            targetX = bgX;
            targetY = bgY;

            handX = x;
            handY = y;
            hasHand = true;
        }
    }

    void UpdateBackground()
    {
        if (menuBackground == null) return;

        float x;
        float y;

        lock (poseLock)
        {
            x = targetX;
            y = targetY;
        }

        Vector3 targetPos = originalPos + new Vector3(x, y, 0);

        menuBackground.localPosition = Vector3.Lerp(
            menuBackground.localPosition,
            targetPos,
            Time.deltaTime * smoothSpeed
        );
    }

    void UpdateHandCursor()
    {
        if (canvas == null || fingerCursor == null) return;

        float x;
        float y;
        bool valid;

        lock (poseLock)
        {
            x = handX;
            y = handY;
            valid = hasHand;
        }

        if (!valid) return;

        Vector2 screenPos = new Vector2(
            x * Screen.width,
            (1f - y) * Screen.height
        );

        Camera uiCamera = null;

        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            uiCamera,
            out Vector2 localPos
        );

        fingerCursor.anchoredPosition = localPos;
    }
}