using UnityEngine;
using UnityEngine.UI;

public class SpriteFrameAnimator : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 8f;
    [SerializeField] private bool playOnEnable = true;

    private float timer;
    private int frameIndex;
    private bool isPlaying;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!isPlaying || targetImage == null || frames == null || frames.Length == 0)
        {
            return;
        }

        float interval = 1f / Mathf.Max(1f, framesPerSecond);
        timer += Time.unscaledDeltaTime;

        while (timer >= interval)
        {
            timer -= interval;
            frameIndex = (frameIndex + 1) % frames.Length;
            targetImage.sprite = frames[frameIndex];
        }
    }

    public void Play()
    {
        isPlaying = true;
        timer = 0f;
        frameIndex = 0;

        ApplyFrame();
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void SetTargetImage(Image image)
    {
        targetImage = image;
    }

    public void SetFrames(Sprite[] newFrames)
    {
        frames = newFrames;
    }

    public void SetPlayOnEnable(bool shouldPlayOnEnable)
    {
        playOnEnable = shouldPlayOnEnable;
    }

    public void ResetToFirstFrame()
    {
        isPlaying = false;
        timer = 0f;
        frameIndex = 0;
        ApplyFrame();
    }

    public void SetProgress(float normalizedProgress)
    {
        if (targetImage == null || frames == null || frames.Length == 0)
        {
            return;
        }

        isPlaying = false;
        timer = 0f;
        float clampedProgress = Mathf.Clamp01(normalizedProgress);
        frameIndex = Mathf.Clamp(Mathf.FloorToInt(clampedProgress * frames.Length), 0, frames.Length - 1);
        ApplyFrame();
    }

    private void ApplyFrame()
    {
        if (targetImage != null && frames != null && frames.Length > 0)
        {
            targetImage.sprite = frames[frameIndex];
        }
    }
}
