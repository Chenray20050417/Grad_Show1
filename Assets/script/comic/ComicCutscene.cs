using UnityEngine;

public class ComicCutscene : MonoBehaviour
{
    [System.Serializable]
    public class VoiceLine
    {
        public AudioClip clip;
        public float delay = 0.3f;
        public bool shake = false;
    }

    [System.Serializable]
    public class ComicFrame
    {
        [Header("位置與縮放")]
        public Vector2 position;
        public Vector3 scale = Vector3.one;

        [Header("移動到這一幀的時間")]
        public float moveTime = 1.2f;

        [Header("到達這一幀後播放的配音")]
        public VoiceLine[] voices;
    }

    [Header("漫畫圖片")]
    public RectTransform comicImage;

    [Header("所有 Key Frame")]
    public ComicFrame[] frames;

    [Header("震動設定")]
    public float shakeDuration = 0.45f;
    public float shakeStrength = 55f;

    private AudioSource audioSource;

    private int currentFrame = 0;
    private int currentVoice = 0;

    private float timer = 0f;
    private bool moving = false;
    private bool waitingVoice = false;
    private bool waitingDelay = false;

    private float delayTimer = 0f;
    private float currentDelay = 0f;

    private bool isShaking = false;
    private float shakeTimer = 0f;
    private Vector2 shakeBasePos;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    void Start()
    {
        if (frames == null || frames.Length == 0 || comicImage == null)
            return;

        comicImage.anchoredPosition = frames[0].position;
        comicImage.localScale = frames[0].scale;

        currentFrame = 0;
        StartMoveToNextFrame();
    }

    void Update()
    {
        UpdateShake();

        if (waitingVoice)
        {
            if (!audioSource.isPlaying)
            {
                waitingVoice = false;
                PlayNextVoiceOrMove();
            }

            return;
        }

        if (waitingDelay)
        {
            delayTimer += Time.deltaTime;

            if (delayTimer >= currentDelay)
            {
                waitingDelay = false;
                PlayCurrentVoice();
            }

            return;
        }

        if (moving)
        {
            MoveToFrame();
        }
    }

    void StartMoveToNextFrame()
    {
        if (currentFrame >= frames.Length - 1)
            return;

        moving = true;
        timer = 0f;
    }

    void MoveToFrame()
    {
        int nextFrame = currentFrame + 1;

        ComicFrame from = frames[currentFrame];
        ComicFrame to = frames[nextFrame];

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / to.moveTime);
        t = Smooth(t);

        comicImage.anchoredPosition =
            Vector2.Lerp(from.position, to.position, t);

        comicImage.localScale =
            Vector3.Lerp(from.scale, to.scale, t);

        if (timer >= to.moveTime)
        {
            moving = false;

            comicImage.anchoredPosition = to.position;
            comicImage.localScale = to.scale;

            currentFrame = nextFrame;
            currentVoice = 0;

            PlayNextVoiceOrMove();
        }
    }

    void PlayNextVoiceOrMove()
    {
        ComicFrame frame = frames[currentFrame];

        if (frame.voices != null && currentVoice < frame.voices.Length)
        {
            VoiceLine line = frame.voices[currentVoice];

            currentDelay = line.delay;
            delayTimer = 0f;
            waitingDelay = true;

            return;
        }

        StartMoveToNextFrame();
    }

    void PlayCurrentVoice()
    {
        ComicFrame frame = frames[currentFrame];

        if (frame.voices == null || currentVoice >= frame.voices.Length)
        {
            StartMoveToNextFrame();
            return;
        }

        VoiceLine line = frame.voices[currentVoice];

        if (line.shake)
            ShakeScreen();

        currentVoice++;

        if (line.clip == null)
        {
            PlayNextVoiceOrMove();
            return;
        }

        audioSource.Stop();
        audioSource.clip = line.clip;
        audioSource.Play();

        waitingVoice = true;
    }

    void ShakeScreen()
    {
        shakeBasePos = comicImage.anchoredPosition;
        isShaking = true;
        shakeTimer = 0f;
    }

    void UpdateShake()
    {
        if (!isShaking) return;

        shakeTimer += Time.deltaTime;

        Vector2 randomPos = Random.insideUnitCircle * shakeStrength;
        comicImage.anchoredPosition = shakeBasePos + randomPos;

        if (shakeTimer >= shakeDuration)
        {
            isShaking = false;
            shakeTimer = 0f;
            comicImage.anchoredPosition = shakeBasePos;
        }
    }

    float Smooth(float t)
    {
        return t * t * (3f - 2f * t);
    }
}