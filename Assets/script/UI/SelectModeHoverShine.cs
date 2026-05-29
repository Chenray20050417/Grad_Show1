using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectModeHoverShine : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public Color shineColor = new Color(1f, 0.82f, 0.2f, 0.34f);
    public float sweepDuration = 0.55f;
    public float repeatDelay = 0.35f;
    public float stripeWidth = 86f;
    public float stripeAngle = -22f;
    public float hoverScale = 1f;
    public float scaleSpeed = 14f;

    private RectTransform rectTransform;
    private RectTransform shineRect;
    private Image shineImage;
    private Vector3 baseScale;
    private bool hovering;
    private float sweepTimer;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        baseScale = rectTransform.localScale;
        EnsureClipper();
        EnsureShine();
        HideShine();
    }

    private void OnDisable()
    {
        hovering = false;
        if (rectTransform != null)
            rectTransform.localScale = baseScale;

        HideShine();
    }

    private void Update()
    {
        Vector3 targetScale = hovering ? baseScale * hoverScale : baseScale;
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);

        if (!hovering)
            return;

        AnimateShine();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopHover();
    }

    public void OnSelect(BaseEventData eventData)
    {
        StartHover();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        StopHover();
    }

    public void SetHovered(bool value)
    {
        if (value)
            StartHover();
        else
            StopHover();
    }

    private void StartHover()
    {
        hovering = true;
        sweepTimer = 0f;
        if (shineImage != null)
            shineImage.enabled = true;
    }

    private void StopHover()
    {
        hovering = false;
        HideShine();
    }

    private void AnimateShine()
    {
        if (shineRect == null || shineImage == null)
            return;

        float cycleDuration = Mathf.Max(0.05f, sweepDuration) + Mathf.Max(0f, repeatDelay);
        sweepTimer = (sweepTimer + Time.unscaledDeltaTime) % cycleDuration;

        if (sweepTimer > sweepDuration)
        {
            shineImage.enabled = false;
            return;
        }

        shineImage.enabled = true;

        float progress = Mathf.Clamp01(sweepTimer / Mathf.Max(0.05f, sweepDuration));
        float eased = 1f - Mathf.Pow(1f - progress, 3f);
        float travel = rectTransform.rect.width + stripeWidth * 3f;
        float x = Mathf.Lerp(-travel * 0.5f, travel * 0.5f, eased);

        shineRect.anchoredPosition = new Vector2(x, 0f);
        shineImage.color = new Color(shineColor.r, shineColor.g, shineColor.b, shineColor.a * Mathf.Sin(progress * Mathf.PI));
    }

    private void EnsureClipper()
    {
        if (GetComponent<RectMask2D>() == null)
            gameObject.AddComponent<RectMask2D>();
    }

    private void EnsureShine()
    {
        Transform existing = transform.Find("HoverSlashReflection");
        GameObject shineObject = existing != null ? existing.gameObject : new GameObject("HoverSlashReflection", typeof(RectTransform), typeof(Image));
        shineObject.transform.SetParent(transform, false);
        shineObject.transform.SetAsLastSibling();

        shineRect = shineObject.GetComponent<RectTransform>();
        shineRect.anchorMin = new Vector2(0.5f, 0.5f);
        shineRect.anchorMax = new Vector2(0.5f, 0.5f);
        shineRect.pivot = new Vector2(0.5f, 0.5f);
        shineRect.sizeDelta = new Vector2(stripeWidth, rectTransform.rect.height * 1.35f);
        shineRect.localRotation = Quaternion.Euler(0f, 0f, stripeAngle);

        shineImage = shineObject.GetComponent<Image>();
        shineImage.raycastTarget = false;
        shineImage.color = shineColor;
    }

    private void HideShine()
    {
        if (shineImage != null)
            shineImage.enabled = false;
    }
}
