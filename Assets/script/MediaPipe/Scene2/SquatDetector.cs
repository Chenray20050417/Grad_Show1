using UnityEngine;
using Mediapipe.Tasks.Components.Containers;

public class SquatDetector : MonoBehaviour
{
    [Header("攻擊判定")]
    public PushHitBoxController pushHitBox;

    [Header("Animator")]
    public Animator playerAnimator;
    public string squatAnimName = "squat2";

    [Header("Debug")]
    public bool debugLog = false;
    public int squatCount = 0;
    public float headY;
    public float standingHeadY;
    public float diff;

    [Header("判定參數")]
    public float headDownDistance = 0.08f;
    public float standUpDistance = 0.03f;
    [Range(0f, 1f)] public float standingBaselineLerp = 0.08f;
    public float cooldown = 1.2f;

    [Header("穩定度")]
    public int requiredFrames = 2;

    private enum SquatState
    {
        Standing,
        Down
    }

    private SquatState state = SquatState.Standing;

    private bool requestSquat = false;
    private float lastTriggerTime = -999f;
    private bool hasStandingBaseline = false;

    private int validFrameCount = 0;

    public void CheckPose(NormalizedLandmark[] lm)
    {
        if (debugLog)
            Debug.Log("Squat CheckPose 有進來");

        if (lm == null || lm.Length < 1)
        {
            if (debugLog)
                Debug.Log("沒有偵測到頭部");
            return;
        }

        headY = lm[0].y;

        if (!hasStandingBaseline)
        {
            standingHeadY = headY;
            hasStandingBaseline = true;
        }

        diff = headY - standingHeadY;

        if (debugLog)
        {
            Debug.Log(
                "headY=" + headY.ToString("F2") +
                " standingHeadY=" + standingHeadY.ToString("F2") +
                " diff=" + diff.ToString("F2") +
                " state=" + state +
                " frame=" + validFrameCount
            );
        }

        // -------------------------
        // 蹲下
        // -------------------------
        if (state == SquatState.Standing)
        {
            if (diff <= standUpDistance)
            {
                standingHeadY = Mathf.Lerp(
                    standingHeadY,
                    headY,
                    standingBaselineLerp
                );
            }

            diff = headY - standingHeadY;

            if (diff >= headDownDistance)
            {
                validFrameCount++;

                if (validFrameCount >= requiredFrames)
                {
                    state = SquatState.Down;
                    validFrameCount = 0;
                    requestSquat = true;

                    if (debugLog)
                        Debug.Log("偵測到蹲下");
                }
            }
            else
            {
                validFrameCount = 0;
            }
        }

        // -------------------------
        // 起立
        // -------------------------
        else if (state == SquatState.Down)
        {
            if (diff <= standUpDistance)
            {
                validFrameCount++;

                if (validFrameCount >= requiredFrames)
                {
                    state = SquatState.Standing;

                    validFrameCount = 0;

                    if (debugLog)
                        Debug.Log("起立成功");
                }
            }
            else
            {
                validFrameCount = 0;
            }
        }
    }

    void Update()
    {
        if (requestSquat && Time.time - lastTriggerTime > cooldown)
        {
            requestSquat = false;

            lastTriggerTime = Time.time;

            squatCount++;

            if (debugLog)
                Debug.Log("完成深蹲：" + squatCount);

            if (playerAnimator != null)
            {
                playerAnimator.Play(squatAnimName, 0, 0f);
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayLevel2PlayerAnimation();
                }

                playerAnimator.Update(0f);

                // 開啟 HitBox
                if (pushHitBox != null)
                {
                    pushHitBox.ActivateHitBox();
                }
            }
            else
            {
                Debug.LogWarning("Player Animator 沒設定");
            }
        }
    }
}
