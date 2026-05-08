using UnityEngine;
using Mediapipe.Tasks.Components.Containers;

public class SquatDetector : MonoBehaviour
{
    [Header("Animator")]
    public Animator playerAnimator;
    public string squatAnimName = "squat";

    [Header("Debug")]
    public int squatCount = 0;
    public float hipY;
    public float kneeY;
    public float diff;

    [Header("判定參數")]
    public float squatDownDiff = 0.03f;
    public float standUpDiff = 0.12f;
    public float cooldown = 1.2f;

    private enum SquatState
    {
        Standing,
        Down
    }

    private SquatState state = SquatState.Standing;
    private bool requestPlay = false;
    private float lastTriggerTime = -999f;

    public void CheckPose(NormalizedLandmark[] lm)
    {
        if (lm == null || lm.Length < 33)
            return;

        hipY = (lm[23].y + lm[24].y) / 2f;
        kneeY = (lm[25].y + lm[26].y) / 2f;
        diff = kneeY - hipY;

        // diff 小：臀部接近膝蓋，代表蹲下
        // diff 大：臀部高於膝蓋，代表站起來

        if (state == SquatState.Standing)
        {
            if (diff <= squatDownDiff)
            {
                state = SquatState.Down;
                Debug.Log("⬇ 深蹲：蹲下");
            }
        }
        else if (state == SquatState.Down)
        {
            if (diff >= standUpDiff)
            {
                state = SquatState.Standing;
                requestPlay = true;
                Debug.Log("⬆ 深蹲：起立，準備觸發動畫");
            }
        }
    }

    void Update()
    {
        if (requestPlay && Time.time - lastTriggerTime > cooldown)
        {
            requestPlay = false;
            lastTriggerTime = Time.time;

            squatCount++;

            Debug.Log("✅ 完成深蹲次數：" + squatCount);

            if (playerAnimator != null)
                playerAnimator.Play(squatAnimName, 0, 0f);
        }
    }
}