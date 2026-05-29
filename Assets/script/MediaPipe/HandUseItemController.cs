using UnityEngine;
using Mediapipe.Tasks.Components.Containers;

public class HandUseItemController : MonoBehaviour
{
    public static HandUseItemController Instance;

    [Header("手勢冷卻")]
    public float gestureCooldown = 1.2f;

    [Header("指定手")]
    public string useItemHandLabel = "Right";

    private float lastUseTime = -999f;

    private int pendingGesture = 0;
    private bool hasNewGesture = false;

    private readonly object handLock = new object();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        int gesture = 0;

        lock (handLock)
        {
            if (hasNewGesture)
            {
                gesture = pendingGesture;
                hasNewGesture = false;
            }
        }

        if (gesture == 0) return;

        if (Time.time - lastUseTime < gestureCooldown)
            return;

        if (InventoryManager.Instance == null)
            return;

        if (gesture == 1)
        {
            InventoryManager.Instance.UseItem(0);
            lastUseTime = Time.time;
        }
        else if (gesture == 2)
        {
            InventoryManager.Instance.UseItem(1);
            lastUseTime = Time.time;
        }
        else if (gesture == 3)
        {
            InventoryManager.Instance.UseItem(2);
            lastUseTime = Time.time;
        }
    }

    public void CheckHand(NormalizedLandmarks hand)
    {
        CheckHand(hand, useItemHandLabel);
    }

    public void CheckHand(NormalizedLandmarks hand, string handLabel)
    {
        if (hand.landmarks == null || hand.landmarks.Count < 21)
            return;

        if (!string.IsNullOrEmpty(useItemHandLabel) && handLabel != useItemHandLabel)
            return;

        int gestureNumber = DetectNumber(hand);

        lock (handLock)
        {
            pendingGesture = gestureNumber;
            hasNewGesture = true;
        }
    }

    private int DetectNumber(NormalizedLandmarks hand)
    {
        var lm = hand.landmarks;

        bool indexUp = lm[8].y < lm[6].y;
        bool middleUp = lm[12].y < lm[10].y;
        bool ringUp = lm[16].y < lm[14].y;
        bool pinkyUp = lm[20].y < lm[18].y;

        if (indexUp && !middleUp && !ringUp && !pinkyUp)
            return 1;

        if (indexUp && middleUp && !ringUp && !pinkyUp)
            return 2;

        if (indexUp && middleUp && ringUp && !pinkyUp)
            return 3;

        return 0;
    }
}
