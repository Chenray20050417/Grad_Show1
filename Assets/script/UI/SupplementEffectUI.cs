using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SupplementEffectUI : MonoBehaviour
{
    public static SupplementEffectUI Instance;

    [Header("補劑提示文字")]
    public TMP_Text noticeText;

    [Header("補劑圖示")]
    public Image noticeIcon;

    [Header("黑色遮罩")]
    public GameObject overlay;

    [Header("慢動作設定")]
    [Range(0.01f, 1f)]
    public float slowMotionScale = 0.15f;
    public float showTime = 3f;
    public float recoverSpeed = 2f;

    [Header("Camera Shake")]
    public Transform cameraTransform;
    public float shakeTime = 0.2f;
    public float shakeStrength = 0.15f;

    private bool proteinShown = false;
    private bool creatineShown = false;
    private bool testosteroneShown = false;
    private bool riceShown = false;
    private bool vitaminShown = false;

    private Vector3 cameraOriginalPos;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (noticeText != null)
            noticeText.gameObject.SetActive(false);

        if (noticeIcon != null)
            noticeIcon.gameObject.SetActive(false);

        if (overlay != null)
            overlay.SetActive(false);

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    public void ShowSupplementEffect(SupplementType type)
    {
        if (AlreadyShown(type))
            return;

        SetShown(type);

        StopAllCoroutines();
        StartCoroutine(ShowRoutine(type));
    }

    private IEnumerator ShowRoutine(SupplementType type)
    {
        float originalFixedDelta = Time.fixedDeltaTime;
        float safeSlowMotionScale = Mathf.Clamp(slowMotionScale, 0.01f, 1f);

        Time.timeScale = safeSlowMotionScale;
        Time.fixedDeltaTime = originalFixedDelta * Time.timeScale;

        if (overlay != null)
            overlay.SetActive(true);

        if (noticeText != null)
        {
            noticeText.gameObject.SetActive(true);
            noticeText.text = GetNoticeText(type);
            noticeText.transform.localScale = Vector3.one * 0.6f;
        }

        if (noticeIcon != null)
        {
            noticeIcon.gameObject.SetActive(true);

            if (InventoryManager.Instance != null)
                noticeIcon.sprite = InventoryManager.Instance.GetIcon(type);

            noticeIcon.transform.localScale = Vector3.one * 0.6f;
            noticeIcon.transform.rotation = Quaternion.identity;
        }

        StartCoroutine(CameraShake());

        float popTime = 0.25f;
        float popTimer = 0f;

        while (popTimer < popTime)
        {
            popTimer += Time.unscaledDeltaTime;
            float t = popTimer / popTime;
            float scale = Mathf.Lerp(0.6f, 1.3f, t);

            if (noticeText != null)
                noticeText.transform.localScale = Vector3.one * scale;

            if (noticeIcon != null)
                noticeIcon.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        float waitTimer = 0f;

        while (waitTimer < showTime)
        {
            waitTimer += Time.unscaledDeltaTime;

            if (noticeIcon != null)
                noticeIcon.transform.Rotate(0, 0, 180f * Time.unscaledDeltaTime);

            yield return null;
        }

        float recoverTimer = 0f;

        while (recoverTimer < 1f)
        {
            recoverTimer += Time.unscaledDeltaTime * recoverSpeed;

            Time.timeScale = Mathf.Lerp(safeSlowMotionScale, 1f, recoverTimer);
            Time.fixedDeltaTime = originalFixedDelta * Time.timeScale;

            float scale = Mathf.Lerp(1.3f, 1f, recoverTimer);

            if (noticeText != null)
                noticeText.transform.localScale = Vector3.one * scale;

            if (noticeIcon != null)
                noticeIcon.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDelta;

        if (noticeText != null)
            noticeText.gameObject.SetActive(false);

        if (noticeIcon != null)
            noticeIcon.gameObject.SetActive(false);

        if (overlay != null)
            overlay.SetActive(false);
    }

    private IEnumerator CameraShake()
    {
        if (cameraTransform == null)
            yield break;

        cameraOriginalPos = cameraTransform.position;

        float timer = 0f;

        while (timer < shakeTime)
        {
            timer += Time.unscaledDeltaTime;

            Vector3 offset = Random.insideUnitSphere * shakeStrength;
            offset.z = 0;

            cameraTransform.position = cameraOriginalPos + offset;

            yield return null;
        }

        cameraTransform.position = cameraOriginalPos;
    }

    private bool AlreadyShown(SupplementType type)
    {
        switch (type)
        {
            case SupplementType.Protein:
                return proteinShown;

            case SupplementType.Creatine:
                return creatineShown;

            case SupplementType.Testosterone:
                return testosteroneShown;

            case SupplementType.Rice:
                return riceShown;

            case SupplementType.Vitamin:
                return vitaminShown;

            default:
                return false;
        }
    }

    private void SetShown(SupplementType type)
    {
        switch (type)
        {
            case SupplementType.Protein:
                proteinShown = true;
                break;

            case SupplementType.Creatine:
                creatineShown = true;
                break;

            case SupplementType.Testosterone:
                testosteroneShown = true;
                break;

            case SupplementType.Rice:
                riceShown = true;
                break;

            case SupplementType.Vitamin:
                vitaminShown = true;
                break;
        }
    }

    private string GetNoticeText(SupplementType type)
    {
        switch (type)
        {
            case SupplementType.Protein:
                return "蛋白粉\n時間 +10 秒";

            case SupplementType.Creatine:
                return "肌酸\n回血 15%";

            case SupplementType.Testosterone:
                return "睪固酮\n無敵 5 秒";

            case SupplementType.Rice:
                return "白飯\n效果保留";

            case SupplementType.Vitamin:
                return "維他命\n慢動作 10 秒";

            default:
                return "補劑";
        }
    }
}
