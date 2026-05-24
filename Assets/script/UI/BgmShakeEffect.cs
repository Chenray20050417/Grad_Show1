using UnityEngine;

public class BgmShakeEffect : MonoBehaviour
{
    [Header("目標")]
    public RectTransform targetRect;
    public Transform targetTransform;

    [Header("震動")]
    public float shakeStrength = 6f;
    public float smoothSpeed = 12f;
    public float sensitivity = 70f;
    public float threshold = 0.02f;

    [Header("限制")]
    public float maxOffset = 10f;

    private readonly float[] samples = new float[256];
    private Vector3 originalLocalPosition;
    private Vector3 currentOffset;

    private void Awake()
    {
        if (targetRect == null)
            targetRect = GetComponent<RectTransform>();

        if (targetTransform == null)
            targetTransform = transform;
    }

    private void Start()
    {
        if (targetRect != null)
            originalLocalPosition = targetRect.localPosition;
        else if (targetTransform != null)
            originalLocalPosition = targetTransform.localPosition;
    }

    private void Update()
    {
        AudioSource bgmSource = GetBgmSource();

        float energy = 0f;

        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.GetOutputData(samples, 0);

            for (int i = 0; i < samples.Length; i++)
                energy += Mathf.Abs(samples[i]);

            energy /= samples.Length;
        }

        float amount = Mathf.Clamp01((energy - threshold) * sensitivity);
        Vector2 random = Random.insideUnitCircle * shakeStrength * amount;
        random = Vector2.ClampMagnitude(random, maxOffset);

        Vector3 targetOffset = new Vector3(random.x, random.y, 0f);
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.unscaledDeltaTime * smoothSpeed);

        if (targetRect != null)
            targetRect.localPosition = originalLocalPosition + currentOffset;
        else if (targetTransform != null)
            targetTransform.localPosition = originalLocalPosition + currentOffset;
    }

    private AudioSource GetBgmSource()
    {
        if (AudioManager.Instance == null)
            return null;

        return AudioManager.Instance.GetBgmSource();
    }
}
