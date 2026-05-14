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
    public int squatCount = 0;
    public float hipY;
    public float kneeY;
    public float diff;

    [Header("判定參數")]
    public float squatDownDiff = 0.03f;
    public float standUpDiff = 0.12f;
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

    private int validFrameCount = 0;

    public void CheckPose(NormalizedLandmark[] lm)
    {
        Debug.Log("Squat CheckPose 有進來");

        if (lm == null || lm.Length < 33)
        {
            Debug.Log("沒有完整人體");
            return;
        }

        hipY = (lm[23].y + lm[24].y) / 2f;
        kneeY = (lm[25].y + lm[26].y) / 2f;

        diff = kneeY - hipY;

        Debug.Log(
            "hipY=" + hipY.ToString("F2") +
            " kneeY=" + kneeY.ToString("F2") +
            " diff=" + diff.ToString("F2") +
            " state=" + state +
            " frame=" + validFrameCount
        );

        // -------------------------
        // 蹲下
        // -------------------------
        if (state == SquatState.Standing)
        {
            if (diff <= squatDownDiff)
            {
                validFrameCount++;

                if (validFrameCount >= requiredFrames)
                {
                    state = SquatState.Down;
                    validFrameCount = 0;

                    Debug.Log("⬇ 偵測到蹲下");
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
            if (diff >= standUpDiff)
            {
                validFrameCount++;

                if (validFrameCount >= requiredFrames)
                {
                    state = SquatState.Standing;

                    validFrameCount = 0;

                    requestSquat = true;

                    Debug.Log("⬆ 起立成功");
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

            Debug.Log("✅ 完成深蹲：" + squatCount);

            if (playerAnimator != null)
            {
                playerAnimator.Play(squatAnimName, 0, 0f);

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