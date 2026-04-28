using UnityEngine;
using Mediapipe.Tasks.Vision.PoseLandmarker;

public class ShoulderPressDetector : MonoBehaviour
{
    public Animator playerAnimator;
    public string pushAnimName = "push";
    public float triggerCooldown = 1.2f;

    private bool requestPush = false;
    private bool readyForNextPush = true;
    private float lastTriggerTime = -999f;

    void Update()
    {
        if (requestPush && Time.time - lastTriggerTime > triggerCooldown)
        {
            requestPush = false;
            readyForNextPush = false;
            lastTriggerTime = Time.time;

            Debug.Log("主執行緒播放 push");

            playerAnimator.Play(pushAnimName, 0, 0f);
            playerAnimator.Update(0f);
        }
    }

    public void CheckPose(PoseLandmarkerResult result)
    {
        if (result.poseLandmarks == null) return;
        if (result.poseLandmarks.Count == 0) return;

        var landmarks = result.poseLandmarks[0].landmarks;

        var leftWrist = landmarks[15];
        var rightWrist = landmarks[16];

        Debug.Log("左手Y=" + leftWrist.y + " 右手Y=" + rightWrist.y);

        bool handUp =
            leftWrist.y < 0.55f ||
            rightWrist.y < 0.55f;

        bool handDown =
            leftWrist.y > 0.75f &&
            rightWrist.y > 0.75f;

        if (handDown)
        {
            readyForNextPush = true;
            Debug.Log("手已放下，可以下一次");
        }

        if (readyForNextPush && handUp)
        {
            Debug.Log("偵測到一次肩推");
            requestPush = true;
        }
    }
}