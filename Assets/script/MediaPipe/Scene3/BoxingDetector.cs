using UnityEngine;
using Mediapipe.Tasks.Components.Containers;

public class BoxingDetector : MonoBehaviour
{
    [Header("Animator")]
    public Animator playerAnimator;
    public string pushAnimName = "boxing_sprite_ani";
    public PushHitBoxController pushHitBox;

    [Header("HitBox Direction")]
    public Transform pushHitBoxTransform;
    public bool mirrorPushHitBoxPosition = true;
    public bool pushHitBoxOnRightByDefault = true;

    [Header("Turn Detection")]
    public SpriteRenderer playerSpriteRenderer;
    public Transform playerVisual;
    public bool useSpriteFlipX = true;
    public bool spriteFacesRightByDefault = true;
    public bool invertFacing = false;
    public float shoulderSwitchDeadZone = 0.06f;
    public float facingHoldTime = 0.3f;
    public int stableFramesRequired = 3;

    [Header("Debug")]
    public float leftSpeed;
    public float rightSpeed;
    public float leftReturnDistance;
    public float rightReturnDistance;
    public float shoulderXDiff;
    public bool isFacingRight = true;
    public bool leftReady;
    public bool rightReady;
    public int leftReturnFrames;
    public int rightReturnFrames;
    public int leftPunchFrames;
    public int rightPunchFrames;

    [Header("Settings")]
    public bool useSpeedCheck = false;
    public bool useForwardMoveCheck = false;
    public float speedThreshold = 3.5f;
    public float returnDistance = 0.18f; // 手腕接近肩膀多少算收拳
    public float punchDistance = 0.28f; // 手腕離肩膀多遠才算真的打出去
    public float minPunchExtensionFromReturn = 0.1f; // 出拳距離至少要比收拳距離多多少
    public float minForwardMove = 0.03f; // 這一幀手腕要往外伸多少，避免原地晃動誤判
    public int returnStableFrames = 4; // 連續幾幀收拳才進入準備狀態
    public int punchStableFrames = 2; // 連續幾幀伸出去才觸發出拳
    public float cooldown = 0.8f;

    private Vector2 leftShoulder;
    private Vector2 rightShoulder;
    private Vector2 nose;
    private Vector2 leftWrist;
    private Vector2 rightWrist;

    private Vector2 lastLeftWrist;
    private Vector2 lastRightWrist;

    private bool hasPose = false;
    private bool hasLast = false;
    private bool pendingFacingRight = true;
    private int stableFacingFrames = 0;
    private float stableFacingTimer = 0f;
    private float originalPushHitBoxLocalX;
    private bool hasOriginalPushHitBoxLocalX = false;

    private float lastTriggerTime = -999f;

    void Awake()
    {
        if (playerAnimator != null)
        {
            if (playerVisual == null)
            {
                playerVisual = playerAnimator.transform;
            }

            if (playerSpriteRenderer == null)
            {
                playerSpriteRenderer = playerAnimator.GetComponent<SpriteRenderer>();
            }
        }

        if (pushHitBoxTransform == null && pushHitBox != null)
        {
            pushHitBoxTransform = pushHitBox.transform;
        }

        StorePushHitBoxStartPosition();
        ApplyFacing();
    }

    public void CheckPose(NormalizedLandmark[] lm)
    {
        if (lm == null || lm.Length < 33) return;

        leftShoulder = new Vector2(lm[11].x, lm[11].y);
        rightShoulder = new Vector2(lm[12].x, lm[12].y);
        nose = new Vector2(lm[0].x, lm[0].y);

        leftWrist = new Vector2(lm[15].x, lm[15].y);
        rightWrist = new Vector2(lm[16].x, lm[16].y);

        hasPose = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerPunch("空白鍵");
        }

        if (!hasPose) return;

        UpdatePlayerFacing();

        if (playerAnimator == null) return;

        if (!hasLast)
        {
            lastLeftWrist = leftWrist;
            lastRightWrist = rightWrist;
            hasLast = true;
            return;
        }

        Vector2 previousLeftWrist = lastLeftWrist;
        Vector2 previousRightWrist = lastRightWrist;

        leftSpeed = Vector2.Distance(leftWrist, previousLeftWrist) / Mathf.Max(Time.deltaTime, 0.0001f);
        rightSpeed = Vector2.Distance(rightWrist, previousRightWrist) / Mathf.Max(Time.deltaTime, 0.0001f);

        lastLeftWrist = leftWrist;
        lastRightWrist = rightWrist;

        leftReturnDistance = Vector2.Distance(leftWrist, leftShoulder);
        rightReturnDistance = Vector2.Distance(rightWrist, rightShoulder);

        float leftForwardMove = leftReturnDistance - Vector2.Distance(previousLeftWrist, leftShoulder);
        float rightForwardMove = rightReturnDistance - Vector2.Distance(previousRightWrist, rightShoulder);
        float effectivePunchDistance = Mathf.Max(punchDistance, returnDistance + minPunchExtensionFromReturn);

        UpdateHandState(
            leftReturnDistance,
            leftSpeed,
            leftForwardMove,
            ref leftReady,
            ref leftReturnFrames,
            ref leftPunchFrames,
            "左拳",
            effectivePunchDistance
        );

        UpdateHandState(
            rightReturnDistance,
            rightSpeed,
            rightForwardMove,
            ref rightReady,
            ref rightReturnFrames,
            ref rightPunchFrames,
            "右拳",
            effectivePunchDistance
        );

        Debug.Log(
            "左速=" + leftSpeed.ToString("F2") +
            " 右速=" + rightSpeed.ToString("F2") +
            " 左收拳距離=" + leftReturnDistance.ToString("F2") +
            " 右收拳距離=" + rightReturnDistance.ToString("F2") +
            " 出拳門檻=" + effectivePunchDistance.ToString("F2") +
            " leftReady=" + leftReady +
            " rightReady=" + rightReady +
            " 左收拳幀=" + leftReturnFrames +
            " 右收拳幀=" + rightReturnFrames +
            " 左出拳幀=" + leftPunchFrames +
            " 右出拳幀=" + rightPunchFrames
        );
    }

    void TriggerPunch(string hand)
    {
        lastTriggerTime = Time.time;
        leftReady = false;
        rightReady = false;
        leftReturnFrames = 0;
        rightReturnFrames = 0;
        leftPunchFrames = 0;
        rightPunchFrames = 0;

        Debug.Log("🔥 " + hand + " 瞬間加速出拳成功");

        if (pushHitBox != null)
        {
            pushHitBox.ActivateHitBox();
        }

        playerAnimator.Play(pushAnimName, 0, 0f);
    }

    void UpdateHandState(
        float returnDistanceValue,
        float speed,
        float forwardMove,
        ref bool ready,
        ref int returnFrames,
        ref int punchFrames,
        string handName,
        float effectivePunchDistance)
    {
        if (returnDistanceValue < returnDistance)
        {
            returnFrames++;
            punchFrames = 0;
        }
        else
        {
            returnFrames = 0;
        }

        if (!ready && returnFrames >= returnStableFrames)
        {
            ready = true;
        }

        if (!ready)
        {
            punchFrames = 0;
            return;
        }

        bool canPunch =
            Time.time - lastTriggerTime >= cooldown &&
            (!useSpeedCheck || speed > speedThreshold) &&
            (!useForwardMoveCheck || forwardMove > minForwardMove) &&
            returnDistanceValue > effectivePunchDistance;

        if (!canPunch)
        {
            punchFrames = 0;
            return;
        }

        punchFrames++;
        if (punchFrames < punchStableFrames) return;

        ready = false;
        TriggerPunch(handName);
    }

    void UpdatePlayerFacing()
    {
        float shoulderCenterX = (leftShoulder.x + rightShoulder.x) * 0.5f;
        shoulderXDiff = nose.x - shoulderCenterX;

        if (Mathf.Abs(shoulderXDiff) < shoulderSwitchDeadZone)
        {
            stableFacingFrames = 0;
            stableFacingTimer = 0f;
            return;
        }

        bool detectedFacingRight = shoulderXDiff > 0f;
        if (invertFacing)
        {
            detectedFacingRight = !detectedFacingRight;
        }

        if (detectedFacingRight != pendingFacingRight)
        {
            pendingFacingRight = detectedFacingRight;
            stableFacingFrames = 0;
            stableFacingTimer = 0f;
        }

        stableFacingFrames++;
        stableFacingTimer += Time.deltaTime;

        bool facingStable = stableFacingTimer >= facingHoldTime;

        if (!facingStable || isFacingRight == pendingFacingRight)
        {
            return;
        }

        isFacingRight = pendingFacingRight;
        ApplyFacing();
    }

    void ApplyFacing()
    {
        if (useSpriteFlipX && playerSpriteRenderer != null)
        {
            playerSpriteRenderer.flipX = spriteFacesRightByDefault ? !isFacingRight : isFacingRight;
            ApplyPushHitBoxFacing();
            return;
        }

        if (playerVisual == null) return;

        Vector3 scale = playerVisual.localScale;
        float sign = isFacingRight == spriteFacesRightByDefault ? 1f : -1f;
        scale.x = Mathf.Abs(scale.x) * sign;
        playerVisual.localScale = scale;

        ApplyPushHitBoxFacing();
    }

    void StorePushHitBoxStartPosition()
    {
        if (pushHitBoxTransform == null || hasOriginalPushHitBoxLocalX) return;

        originalPushHitBoxLocalX = Mathf.Abs(pushHitBoxTransform.localPosition.x);
        hasOriginalPushHitBoxLocalX = true;
    }

    void ApplyPushHitBoxFacing()
    {
        if (!mirrorPushHitBoxPosition || pushHitBoxTransform == null) return;

        StorePushHitBoxStartPosition();

        float sign = isFacingRight == pushHitBoxOnRightByDefault ? 1f : -1f;
        Vector3 localPosition = pushHitBoxTransform.localPosition;
        localPosition.x = originalPushHitBoxLocalX * sign;
        pushHitBoxTransform.localPosition = localPosition;
    }
}
