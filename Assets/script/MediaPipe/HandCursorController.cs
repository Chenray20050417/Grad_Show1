using UnityEngine;
using Mediapipe.Tasks.Components.Containers;

public class HandCursorController : MonoBehaviour
{
    public RectTransform fingerCursor;
    public RectTransform canvasRect;

    [Header("顯示游標的 Canvas")]
    public GameObject pauseCanvas;
    public GameObject failCanvas;
    public GameObject passCanvas;
    public bool forceShowCursor = false;

    public float smoothSpeed = 20f;

    private Vector2 targetPos;
    private bool hasHand = false;

    private float canvasWidth;
    private float canvasHeight;

    void Start()
    {
        if (canvasRect != null)
        {
            canvasWidth = canvasRect.rect.width;
            canvasHeight = canvasRect.rect.height;
        }

        if (fingerCursor != null)
            fingerCursor.gameObject.SetActive(false);
    }

    public void CheckHand(NormalizedLandmarks lm)
    {
        if (lm.landmarks == null || lm.landmarks.Count < 21)
            return;

        float x = lm.landmarks[8].x;
        float y = lm.landmarks[8].y;

        float uiX = (x - 0.5f) * canvasWidth;
        float uiY = (0.5f - y) * canvasHeight;

        targetPos = new Vector2(uiX, uiY);
        hasHand = true;
    }

    void Update()
    {
        bool pauseOpen = pauseCanvas != null && pauseCanvas.activeSelf;
        bool failOpen = failCanvas != null && failCanvas.activeSelf;
        bool passOpen = passCanvas != null && passCanvas.activeSelf;

        bool uiOpened = forceShowCursor || pauseOpen || failOpen || passOpen;

        if (fingerCursor != null)
            fingerCursor.gameObject.SetActive(uiOpened);

        if (!uiOpened) return;
        if (!hasHand || fingerCursor == null) return;

        fingerCursor.anchoredPosition = Vector2.Lerp(
            fingerCursor.anchoredPosition,
            targetPos,
            smoothSpeed * Time.unscaledDeltaTime
        );
    }
}
