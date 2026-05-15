using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    public int rightHandPoseIndex = 15;

    [Header("握拳點擊")]
    public float clickCooldown = 1f;
    public bool rightHandOnly = true;
    public float rightHandMaxDistance = 0.25f;

    private Vector3 originalPos;

    private float targetX;
    private float targetY;

    private float handX = 0.5f;
    private float handY = 0.5f;
    private bool hasHand = false;
    private bool hasRightHandPose = false;

    private bool lastFist = false;
    private bool requestClick = false;
    private float lastClickTime = -999f;

    private readonly object poseLock = new object();

    void Start()
    {
        if (menuBackground != null)
            originalPos = menuBackground.localPosition;

        Debug.Log("MenuGestureController 啟動完成");
    }

    void Update()
    {
        UpdateBackground();
        UpdateHandCursor();

        // 只能在 Unity 主執行緒點擊
        if (requestClick)
        {
            requestClick = false;
            TryClick();
        }
    }

    public void CheckPose(NormalizedLandmark[] lm)
    {
        if (lm == null || lm.Length <= rightHandPoseIndex) return;

        float noseX = lm[0].x;
        float noseY = lm[0].y;

        float bgX = -(noseX - 0.5f) * moveRangeX * 2f;
        float bgY = (noseY - 0.5f) * moveRangeY * 2f;

        float x = lm[rightHandPoseIndex].x;
        float y = lm[rightHandPoseIndex].y;

        if (mirrorX)
            x = 1f - x;

        lock (poseLock)
        {
            targetX = bgX;
            targetY = bgY;

            handX = x;
            handY = y;
            hasHand = true;
            hasRightHandPose = true;
        }
    }
public void CheckHand(NormalizedLandmarks lm)
{
    if (lm.landmarks == null || lm.landmarks.Count < 21)
        return;

    bool fist = IsFist(lm);

    if (rightHandOnly && !IsNearRightHand(lm))
    {
        if (fist)
            Debug.Log("忽略非右手握拳");

        lastFist = false;
        return;
    }

    if (fist)
    {
        Debug.Log("👊 握拳");

        if (!lastFist)
            requestClick = true;
    }
    else
    {
        Debug.Log("🖐 張開");
    }

    lastFist = fist;
}

    bool IsNearRightHand(NormalizedLandmarks lm)
    {
        float rightX;
        float rightY;
        bool hasPose;

        lock (poseLock)
        {
            rightX = handX;
            rightY = handY;
            hasPose = hasRightHandPose;
        }

        if (!hasPose)
            return true;

        float wristX = lm.landmarks[0].x;
        float wristY = lm.landmarks[0].y;

        if (mirrorX)
            wristX = 1f - wristX;

        float distance = Vector2.Distance(
            new Vector2(wristX, wristY),
            new Vector2(rightX, rightY)
        );

        return distance <= rightHandMaxDistance;
    }

    bool IsFist(NormalizedLandmarks lm)
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

    void TryClick()
    {
        if (fingerCursor == null)
        {
            Debug.LogWarning("FingerCursor 沒有指定");
            return;
        }

        if (EventSystem.current == null)
        {
            Debug.LogWarning("場景沒有 EventSystem");
            return;
        }

        if (Time.time - lastClickTime < clickCooldown)
        {
            Debug.Log("點擊冷卻中");
            return;
        }

        lastClickTime = Time.time;

        PointerEventData pointer =
            new PointerEventData(EventSystem.current);

        Camera uiCamera = null;

        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera;

        pointer.position =
            RectTransformUtility.WorldToScreenPoint(uiCamera, fingerCursor.position);

        var results =
            new System.Collections.Generic.List<RaycastResult>();

        EventSystem.current.RaycastAll(pointer, results);

        Debug.Log("Raycast 打到物件數量：" + results.Count);

        foreach (var r in results)
        {
            Button btn = r.gameObject.GetComponent<Button>();

            if (btn == null)
                btn = r.gameObject.GetComponentInParent<Button>();

            if (btn != null)
            {
                Debug.Log("握拳點擊：" + btn.name);
                btn.onClick.Invoke();
                break;
            }
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

        Vector3 targetPos =
            originalPos + new Vector3(x, y, 0);

        menuBackground.localPosition =
            Vector3.Lerp(
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
