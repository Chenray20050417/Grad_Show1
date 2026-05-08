using UnityEngine;
using Mediapipe.Tasks.Components.Containers;

public class YAGesturePauseController : MonoBehaviour
{
    public PauseManager pauseManager;
    public float cooldown = 1.5f;

    private float lastTriggerTime = -999f;
    private bool requestPause = false;

    public void CheckHand(NormalizedLandmarks lm)
    {
        if (lm.landmarks == null || lm.landmarks.Count < 21)
            return;

        bool indexUp = lm.landmarks[8].y < lm.landmarks[6].y;
        bool middleUp = lm.landmarks[12].y < lm.landmarks[10].y;
        bool ringDown = lm.landmarks[16].y > lm.landmarks[14].y;
        bool pinkyDown = lm.landmarks[20].y > lm.landmarks[18].y;

        if (indexUp && middleUp && ringDown && pinkyDown)
            requestPause = true;
    }

    void Update()
    {
        if (requestPause && Time.unscaledTime - lastTriggerTime > cooldown)
        {
            requestPause = false;
            lastTriggerTime = Time.unscaledTime;

            if (pauseManager != null)
                pauseManager.ShowPause();
        }
    }
}