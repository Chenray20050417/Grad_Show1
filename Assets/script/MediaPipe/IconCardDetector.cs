using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IconCardDetector : MonoBehaviour
{
    private enum IconCardType
    {
        None,
        Green,
        Blue,
        Purple
    }

    public enum UseMode
    {
        UseSupplementType,
        UseInventorySlot
    }

    [Header("Camera")]
    public bool detectionEnabled = false;
    public bool startCameraOnEnable = false;
    public string preferredDeviceName = "";
    public RawImage debugPreview;

    [Header("Sample Area")]
    [Range(0.1f, 1f)] public float sampleWidth = 0.35f;
    [Range(0.1f, 1f)] public float sampleHeight = 0.35f;
    public int sampleStep = 6;

    [Header("Trigger")]
    public float holdToUseSeconds = 0.6f;
    public float cooldownSeconds = 1.5f;
    public UseMode useMode = UseMode.UseSupplementType;

    [Header("Card Mapping")]
    public SupplementType greenCardSupplement = SupplementType.Protein;
    public SupplementType blueCardSupplement = SupplementType.Vitamin;
    public SupplementType purpleCardSupplement = SupplementType.Testosterone;
    public int greenCardSlotIndex = 0;
    public int blueCardSlotIndex = 1;
    public int purpleCardSlotIndex = 2;

    [Header("Debug")]
    public TMP_Text statusText;
    public IconCardTypeForInspector currentCard = IconCardTypeForInspector.None;
    public float currentConfidence;

    private WebCamTexture webcamTexture;
    private IconCardType heldCard = IconCardType.None;
    private float holdTimer;
    private float lastUseTime = -999f;

    public enum IconCardTypeForInspector
    {
        None,
        Green,
        Blue,
        Purple
    }

    private void OnEnable()
    {
        if (!detectionEnabled)
        {
            UpdateStatus("圖示卡偵測已停用");
            return;
        }

        if (startCameraOnEnable)
            StartCamera();
    }

    private void OnDisable()
    {
        StopCamera();
    }

    private void Update()
    {
        if (!detectionEnabled)
            return;

        if (webcamTexture == null || !webcamTexture.isPlaying)
            return;

        if (webcamTexture.width <= 16 || webcamTexture.height <= 16)
            return;

        IconCardType detected = DetectCenterCard(out float confidence);
        currentConfidence = confidence;
        currentCard = ToInspectorType(detected);

        if (detected == IconCardType.None)
        {
            heldCard = IconCardType.None;
            holdTimer = 0f;
            UpdateStatus("未偵測到圖示卡");
            return;
        }

        if (detected != heldCard)
        {
            heldCard = detected;
            holdTimer = 0f;
        }

        holdTimer += Time.unscaledDeltaTime;
        UpdateStatus(GetCardName(detected) + " " + Mathf.Clamp01(holdTimer / holdToUseSeconds).ToString("P0"));

        if (holdTimer < holdToUseSeconds)
            return;

        if (Time.unscaledTime - lastUseTime < cooldownSeconds)
            return;

        lastUseTime = Time.unscaledTime;
        holdTimer = 0f;
        UseDetectedCard(detected);
    }

    public void StartCamera()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
            return;

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices == null || devices.Length == 0)
        {
            Debug.LogWarning("IconCardDetector 找不到攝影機");
            return;
        }

        string deviceName = devices[0].name;

        if (!string.IsNullOrWhiteSpace(preferredDeviceName))
        {
            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i].name.Contains(preferredDeviceName))
                {
                    deviceName = devices[i].name;
                    break;
                }
            }
        }

        webcamTexture = new WebCamTexture(deviceName, 640, 480, 30);
        webcamTexture.Play();

        if (debugPreview != null)
            debugPreview.texture = webcamTexture;
    }

    public void StopCamera()
    {
        if (webcamTexture == null)
            return;

        if (webcamTexture.isPlaying)
            webcamTexture.Stop();

        if (debugPreview != null && debugPreview.texture == webcamTexture)
            debugPreview.texture = null;

        webcamTexture = null;
    }

    private IconCardType DetectCenterCard(out float confidence)
    {
        Color32[] pixels = webcamTexture.GetPixels32();
        int width = webcamTexture.width;
        int height = webcamTexture.height;

        int areaWidth = Mathf.RoundToInt(width * sampleWidth);
        int areaHeight = Mathf.RoundToInt(height * sampleHeight);
        int startX = Mathf.Clamp((width - areaWidth) / 2, 0, width - 1);
        int endX = Mathf.Clamp(startX + areaWidth, 0, width);
        int startY = Mathf.Clamp((height - areaHeight) / 2, 0, height - 1);
        int endY = Mathf.Clamp(startY + areaHeight, 0, height);
        int safeStep = Mathf.Max(1, sampleStep);

        int greenCount = 0;
        int blueCount = 0;
        int purpleCount = 0;
        int validCount = 0;

        for (int y = startY; y < endY; y += safeStep)
        {
            int row = y * width;

            for (int x = startX; x < endX; x += safeStep)
            {
                Color32 pixel = pixels[row + x];
                Color.RGBToHSV(pixel, out float hue, out float saturation, out float value);

                if (saturation < 0.35f || value < 0.18f)
                    continue;

                validCount++;
                float degree = hue * 360f;

                if (degree >= 75f && degree <= 170f)
                    greenCount++;
                else if (degree >= 185f && degree <= 250f)
                    blueCount++;
                else if (degree >= 260f && degree <= 330f)
                    purpleCount++;
            }
        }

        if (validCount <= 0)
        {
            confidence = 0f;
            return IconCardType.None;
        }

        int bestCount = greenCount;
        IconCardType bestType = IconCardType.Green;

        if (blueCount > bestCount)
        {
            bestCount = blueCount;
            bestType = IconCardType.Blue;
        }

        if (purpleCount > bestCount)
        {
            bestCount = purpleCount;
            bestType = IconCardType.Purple;
        }

        confidence = bestCount / (float)validCount;
        return confidence >= 0.45f ? bestType : IconCardType.None;
    }

    private void UseDetectedCard(IconCardType card)
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("IconCardDetector 找不到 InventoryManager");
            return;
        }

        if (useMode == UseMode.UseInventorySlot)
        {
            InventoryManager.Instance.UseItem(GetSlotIndex(card));
            Debug.Log("圖示卡使用物品欄格子：" + (GetSlotIndex(card) + 1));
            return;
        }

        SupplementType type = GetSupplementType(card);
        bool success = InventoryManager.Instance.UseFirstItemOfType(type);
        Debug.Log(success ? "圖示卡使用道具：" + type : "圖示卡偵測到，但物品欄沒有：" + type);
    }

    private SupplementType GetSupplementType(IconCardType card)
    {
        switch (card)
        {
            case IconCardType.Green:
                return greenCardSupplement;
            case IconCardType.Blue:
                return blueCardSupplement;
            case IconCardType.Purple:
                return purpleCardSupplement;
            default:
                return greenCardSupplement;
        }
    }

    private int GetSlotIndex(IconCardType card)
    {
        switch (card)
        {
            case IconCardType.Green:
                return greenCardSlotIndex;
            case IconCardType.Blue:
                return blueCardSlotIndex;
            case IconCardType.Purple:
                return purpleCardSlotIndex;
            default:
                return 0;
        }
    }

    private string GetCardName(IconCardType card)
    {
        switch (card)
        {
            case IconCardType.Green:
                return "綠色圖示卡";
            case IconCardType.Blue:
                return "藍色圖示卡";
            case IconCardType.Purple:
                return "紫色圖示卡";
            default:
                return "無";
        }
    }

    private IconCardTypeForInspector ToInspectorType(IconCardType card)
    {
        switch (card)
        {
            case IconCardType.Green:
                return IconCardTypeForInspector.Green;
            case IconCardType.Blue:
                return IconCardTypeForInspector.Blue;
            case IconCardType.Purple:
                return IconCardTypeForInspector.Purple;
            default:
                return IconCardTypeForInspector.None;
        }
    }

    private void UpdateStatus(string text)
    {
        if (statusText != null)
            statusText.text = text;
    }
}
