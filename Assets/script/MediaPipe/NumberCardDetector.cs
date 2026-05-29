using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberCardDetector : MonoBehaviour
{
    public enum DigitPolarity
    {
        Auto,
        DarkDigitOnLightCard,
        LightDigitOnDarkCard
    }

    [Header("Camera")]
    public bool detectionEnabled = false;
    public bool startCameraOnEnable = false;
    public string preferredDeviceName = "";
    public RawImage debugPreview;

    [Header("Card Area")]
    [Range(0.1f, 1f)] public float sampleWidth = 0.45f;
    [Range(0.1f, 1f)] public float sampleHeight = 0.55f;
    public int sampleStep = 3;
    public DigitPolarity digitPolarity = DigitPolarity.Auto;

    [Header("Trigger")]
    public float holdToUseSeconds = 0.6f;
    public float cooldownSeconds = 1.5f;

    [Header("Threshold")]
    [Range(0f, 1f)] public float darkThreshold = 0.38f;
    [Range(0f, 1f)] public float lightThreshold = 0.68f;
    [Range(0f, 1f)] public float minConfidence = 0.58f;
    [Range(0f, 1f)] public float minInkRatio = 0.04f;

    [Header("Debug")]
    public TMP_Text statusText;
    public int currentNumber;
    public float currentConfidence;
    public float currentInkRatio;

    private static readonly int[,] Digit1 =
    {
        {0, 1, 1, 0, 0},
        {1, 1, 1, 0, 0},
        {0, 0, 1, 0, 0},
        {0, 0, 1, 0, 0},
        {0, 0, 1, 0, 0},
        {0, 0, 1, 0, 0},
        {1, 1, 1, 1, 1},
    };

    private static readonly int[,] Digit2 =
    {
        {1, 1, 1, 1, 0},
        {0, 0, 0, 0, 1},
        {0, 0, 0, 0, 1},
        {0, 1, 1, 1, 0},
        {1, 0, 0, 0, 0},
        {1, 0, 0, 0, 0},
        {1, 1, 1, 1, 1},
    };

    private static readonly int[,] Digit3 =
    {
        {1, 1, 1, 1, 0},
        {0, 0, 0, 0, 1},
        {0, 0, 0, 0, 1},
        {0, 1, 1, 1, 0},
        {0, 0, 0, 0, 1},
        {0, 0, 0, 0, 1},
        {1, 1, 1, 1, 0},
    };

    private WebCamTexture webcamTexture;
    private int heldNumber;
    private float holdTimer;
    private float lastUseTime = -999f;

    private void OnEnable()
    {
        if (!detectionEnabled)
        {
            UpdateStatus("數字卡偵測已停用");
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

        int number = DetectNumber(out float confidence, out float inkRatio);
        currentNumber = number;
        currentConfidence = confidence;
        currentInkRatio = inkRatio;

        if (number <= 0)
        {
            heldNumber = 0;
            holdTimer = 0f;
            UpdateStatus("未偵測到數字卡");
            return;
        }

        if (number != heldNumber)
        {
            heldNumber = number;
            holdTimer = 0f;
        }

        holdTimer += Time.unscaledDeltaTime;
        UpdateStatus("數字卡 " + number + " " + Mathf.Clamp01(holdTimer / holdToUseSeconds).ToString("P0"));

        if (holdTimer < holdToUseSeconds)
            return;

        if (Time.unscaledTime - lastUseTime < cooldownSeconds)
            return;

        lastUseTime = Time.unscaledTime;
        holdTimer = 0f;
        UseNumber(number);
    }

    public void StartCamera()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
            return;

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices == null || devices.Length == 0)
        {
            Debug.LogWarning("NumberCardDetector 找不到攝影機");
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

    private int DetectNumber(out float confidence, out float inkRatio)
    {
        confidence = 0f;
        inkRatio = 0f;

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

        bool darkDigit = ResolveDarkDigitPolarity(pixels, width, startX, endX, startY, endY, safeStep);

        int minX = endX;
        int maxX = startX;
        int minY = endY;
        int maxY = startY;
        int inkCount = 0;
        int totalCount = 0;

        for (int y = startY; y < endY; y += safeStep)
        {
            int row = y * width;

            for (int x = startX; x < endX; x += safeStep)
            {
                totalCount++;
                if (!IsInk(pixels[row + x], darkDigit))
                    continue;

                inkCount++;
                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }
        }

        if (totalCount <= 0)
            return 0;

        inkRatio = inkCount / (float)totalCount;

        if (inkRatio < minInkRatio || minX >= maxX || minY >= maxY)
            return 0;

        float score1 = ScoreTemplate(pixels, width, minX, maxX, minY, maxY, safeStep, darkDigit, Digit1);
        float score2 = ScoreTemplate(pixels, width, minX, maxX, minY, maxY, safeStep, darkDigit, Digit2);
        float score3 = ScoreTemplate(pixels, width, minX, maxX, minY, maxY, safeStep, darkDigit, Digit3);

        int number = 1;
        confidence = score1;

        if (score2 > confidence)
        {
            confidence = score2;
            number = 2;
        }

        if (score3 > confidence)
        {
            confidence = score3;
            number = 3;
        }

        return confidence >= minConfidence ? number : 0;
    }

    private bool ResolveDarkDigitPolarity(Color32[] pixels, int width, int startX, int endX, int startY, int endY, int step)
    {
        if (digitPolarity == DigitPolarity.DarkDigitOnLightCard)
            return true;

        if (digitPolarity == DigitPolarity.LightDigitOnDarkCard)
            return false;

        int darkCount = 0;
        int lightCount = 0;

        for (int y = startY; y < endY; y += step)
        {
            int row = y * width;

            for (int x = startX; x < endX; x += step)
            {
                float luma = GetLuma(pixels[row + x]);

                if (luma <= darkThreshold)
                    darkCount++;
                else if (luma >= lightThreshold)
                    lightCount++;
            }
        }

        return darkCount <= lightCount;
    }

    private float ScoreTemplate(Color32[] pixels, int width, int minX, int maxX, int minY, int maxY, int step, bool darkDigit, int[,] template)
    {
        const int columns = 5;
        const int rows = 7;

        int matches = 0;
        int total = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int cellMinX = Mathf.RoundToInt(Mathf.Lerp(minX, maxX, column / (float)columns));
                int cellMaxX = Mathf.RoundToInt(Mathf.Lerp(minX, maxX, (column + 1) / (float)columns));
                int cellMinY = Mathf.RoundToInt(Mathf.Lerp(minY, maxY, row / (float)rows));
                int cellMaxY = Mathf.RoundToInt(Mathf.Lerp(minY, maxY, (row + 1) / (float)rows));

                float occupancy = GetCellInkOccupancy(pixels, width, cellMinX, cellMaxX, cellMinY, cellMaxY, step, darkDigit);
                bool hasInk = occupancy >= 0.25f;
                bool expectedInk = template[rows - 1 - row, column] == 1;

                if (hasInk == expectedInk)
                    matches++;

                total++;
            }
        }

        return total > 0 ? matches / (float)total : 0f;
    }

    private float GetCellInkOccupancy(Color32[] pixels, int width, int minX, int maxX, int minY, int maxY, int step, bool darkDigit)
    {
        int ink = 0;
        int total = 0;

        for (int y = minY; y <= maxY; y += step)
        {
            int row = y * width;

            for (int x = minX; x <= maxX; x += step)
            {
                total++;
                if (IsInk(pixels[row + x], darkDigit))
                    ink++;
            }
        }

        return total > 0 ? ink / (float)total : 0f;
    }

    private bool IsInk(Color32 pixel, bool darkDigit)
    {
        float luma = GetLuma(pixel);
        return darkDigit ? luma <= darkThreshold : luma >= lightThreshold;
    }

    private float GetLuma(Color32 pixel)
    {
        return (pixel.r * 0.2126f + pixel.g * 0.7152f + pixel.b * 0.0722f) / 255f;
    }

    private void UseNumber(int number)
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("NumberCardDetector 找不到 InventoryManager");
            return;
        }

        int slotIndex = Mathf.Clamp(number - 1, 0, 2);
        InventoryManager.Instance.UseItem(slotIndex);
        Debug.Log("數字卡使用物品欄格子：" + number);
    }

    private void UpdateStatus(string text)
    {
        if (statusText != null)
            statusText.text = text;
    }
}
