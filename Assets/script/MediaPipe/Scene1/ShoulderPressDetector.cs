using UnityEngine;
using Mediapipe.Tasks.Vision.PoseLandmarker;

public class ShoulderPressDetector : MonoBehaviour
{   


    [Header("攻擊判定")]
public PushHitBoxController pushHitBox;

    [Header("Animator")]
    public Animator playerAnimator;
    public string pushAnimName = "push";

    [Header("偵測參數")]
    public float cooldown = 1.2f;
    public float downOffset = 0.03f;
    public float upOffset = 0.08f;
    public float minMoveDistance = 0.10f;

    [Header("穩定度")]
    public int requiredFrames = 2;
    public float minShoulderWidth = 0.12f;

    private bool ready = false;
    private bool requestPush = false;
    private float lastTriggerTime = -999f;

    private float lowestLeftY;
    private float lowestRightY;
    private int validFrameCount = 0;

    void Update()
    {
        if (requestPush && Time.time - lastTriggerTime > cooldown)
        {
            requestPush = false;
            ready = false;
            validFrameCount = 0;
            lastTriggerTime = Time.time;

            Debug.Log("主執行緒播放 push");

            if (playerAnimator != null)
            {
                playerAnimator.Play(pushAnimName, 0, 0f);
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayLevel1PlayerAnimation();
                }

                if (pushHitBox != null)
{
    pushHitBox.ActivateHitBox();
}
                playerAnimator.Update(0f);
            }
            else
            {
                Debug.LogWarning("Player Animator 沒設定");
            }
        }
    }

    public void CheckPose(PoseLandmarkerResult result)
    {
        Debug.Log("CheckPose 有進來");

        if (result.poseLandmarks == null)
        {
            Debug.Log("沒有 poseLandmarks");
            return;
        }

        if (result.poseLandmarks.Count == 0)
        {
            Debug.Log("沒有偵測到人");
            return;
        }

        var lm = result.poseLandmarks[0].landmarks;

        var leftShoulder = lm[11];
        var rightShoulder = lm[12];
        var leftElbow = lm[13];
        var rightElbow = lm[14];
        var leftWrist = lm[15];
        var rightWrist = lm[16];

        float shoulderWidth = Mathf.Abs(leftShoulder.x - rightShoulder.x);

        if (shoulderWidth < minShoulderWidth)
        {
            Debug.Log("人太遠 / 肩膀太小，不判斷 shoulderWidth=" + shoulderWidth.ToString("F2"));
            return;
        }

        float avgShoulderY = (leftShoulder.y + rightShoulder.y) / 2f;
        float avgWristY = (leftWrist.y + rightWrist.y) / 2f;

        // MediaPipe：Y 越小代表越高

        // -------------------------
        // 放下判斷：雙手都要放下
        // -------------------------
        bool leftElbowDown = leftElbow.y > leftShoulder.y + downOffset;
        bool rightElbowDown = rightElbow.y > rightShoulder.y + downOffset;

        // 雙手肘都下降才算放下
        bool elbowsDown = leftElbowDown && rightElbowDown;

        // 雙手手腕平均位置不能太高
        bool wristsNotHigh = avgWristY > avgShoulderY - 0.10f;

        bool handsDown = elbowsDown && wristsNotHigh;

        // -------------------------
        // 推上去判斷：雙手一起高於肩膀
        // -------------------------
        bool handsUp = avgWristY < avgShoulderY - upOffset;

        if (handsDown)
        {
            ready = true;
            validFrameCount = 0;

            lowestLeftY = leftWrist.y;
            lowestRightY = rightWrist.y;

            Debug.Log("雙手已放下，可以準備肩推");
        }

        float leftMoveUp = lowestLeftY - leftWrist.y;
        float rightMoveUp = lowestRightY - rightWrist.y;

        // 雙手都要有明顯上移
        bool movedEnough =
            leftMoveUp > minMoveDistance &&
            rightMoveUp > minMoveDistance;

        // 雙手肘都要接近肩膀以上
        bool elbowsUp =
            leftElbow.y < leftShoulder.y + 0.08f &&
            rightElbow.y < rightShoulder.y + 0.08f;

        Debug.Log(
            "肩寬=" + shoulderWidth.ToString("F2") +
            " 肩Y=" + avgShoulderY.ToString("F2") +
            " 手Y=" + avgWristY.ToString("F2") +
            " 左上移=" + leftMoveUp.ToString("F2") +
            " 右上移=" + rightMoveUp.ToString("F2") +
            " leftElbowDown=" + leftElbowDown +
            " rightElbowDown=" + rightElbowDown +
            " handsDown=" + handsDown +
            " handsUp=" + handsUp +
            " movedEnough=" + movedEnough +
            " elbowsUp=" + elbowsUp +
            " ready=" + ready +
            " frame=" + validFrameCount
        );

        if (ready && handsUp && movedEnough && elbowsUp)
        {
            validFrameCount++;
        }
        else
        {
            validFrameCount = 0;
        }

        if (validFrameCount >= requiredFrames)
        {
            Debug.Log("成功偵測雙手肩推一次");

            requestPush = true;
            ready = false;
            validFrameCount = 0;
        }
    }
}
