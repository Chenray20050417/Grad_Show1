using UnityEngine;
using Mediapipe.Tasks.Components.Containers;

public class BoxingDetector : MonoBehaviour
{
    [Header("Animator")]
    public Animator playerAnimator;
    public string pushAnimName = "boxing_sprite_ani";

    [Header("Debug")]
    public float leftSpeed;
    public float rightSpeed;
    public float leftReturnDistance;
    public float rightReturnDistance;

    [Header("Settings")]
    public float speedThreshold = 0.9f;
    public float returnDistance = 0.18f; // 手腕接近肩膀多少算收拳
    public float cooldown = 0.5f;

    private Vector2 leftShoulder;
    private Vector2 rightShoulder;
    private Vector2 leftWrist;
    private Vector2 rightWrist;

    private Vector2 lastLeftWrist;
    private Vector2 lastRightWrist;

    private bool hasPose = false;
    private bool hasLast = false;
    private bool canPunch = true;

    private float lastTriggerTime = -999f;

    public void CheckPose(NormalizedLandmark[] lm)
    {
        if (lm == null || lm.Length < 33) return;

        leftShoulder = new Vector2(lm[11].x, lm[11].y);
        rightShoulder = new Vector2(lm[12].x, lm[12].y);

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
        if (playerAnimator == null) return;

        if (!hasLast)
        {
            lastLeftWrist = leftWrist;
            lastRightWrist = rightWrist;
            hasLast = true;
            return;
        }

        leftSpeed = Vector2.Distance(leftWrist, lastLeftWrist) / Mathf.Max(Time.deltaTime, 0.0001f);
        rightSpeed = Vector2.Distance(rightWrist, lastRightWrist) / Mathf.Max(Time.deltaTime, 0.0001f);

        lastLeftWrist = leftWrist;
        lastRightWrist = rightWrist;

        leftReturnDistance = Vector2.Distance(leftWrist, leftShoulder);
        rightReturnDistance = Vector2.Distance(rightWrist, rightShoulder);

        // 收拳後才允許下一拳
        if (leftReturnDistance < returnDistance || rightReturnDistance < returnDistance)
        {
            canPunch = true;
        }

        Debug.Log(
            "左速=" + leftSpeed.ToString("F2") +
            " 右速=" + rightSpeed.ToString("F2") +
            " 左收拳距離=" + leftReturnDistance.ToString("F2") +
            " 右收拳距離=" + rightReturnDistance.ToString("F2") +
            " canPunch=" + canPunch
        );

        if (!canPunch) return;
        if (Time.time - lastTriggerTime < cooldown) return;

        if (leftSpeed > speedThreshold)
        {
            TriggerPunch("左拳");
        }
        else if (rightSpeed > speedThreshold)
        {
            TriggerPunch("右拳");
        }
    }

    void TriggerPunch(string hand)
    {
        canPunch = false;
        lastTriggerTime = Time.time;

        Debug.Log("🔥 " + hand + " 瞬間加速出拳成功");

        playerAnimator.Play(pushAnimName, 0, 0f);
    }
}