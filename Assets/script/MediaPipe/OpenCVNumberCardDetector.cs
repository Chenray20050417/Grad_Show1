using OpenCvSharp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CvUnity = OpenCvSharp.Unity;

public class OpenCVNumberCardDetector : MonoBehaviour
{
    [Header("Camera")]
    public bool detectionEnabled = true;
    public bool startCameraOnEnable = true;
    public string preferredDeviceName = "";
    public RawImage sharedCameraPreview;
    public RawImage debugPreview;
    public bool flipVertically = false;
    public bool flipHorizontally = false;
    public int rotationAngle = 0;

    [Header("Templates")]
    public Texture2D number1Template;
    public Texture2D number2Template;
    public Texture2D number3Template;

    [Header("Detect Area")]
    [Range(0.1f, 1f)] public float sampleWidth = 0.55f;
    [Range(0.1f, 1f)] public float sampleHeight = 0.65f;
    public int compareWidth = 160;
    public int compareHeight = 220;
    public float processInterval = 0.1f;

    [Header("Trigger")]
    [Range(0f, 1f)] public float confidenceThreshold = 0.62f;
    public float holdToUseSeconds = 0.5f;
    public float cooldownSeconds = 1.5f;

    [Header("Debug")]
    public TMP_Text statusText;
    public int currentNumber;
    public float currentConfidence;
    public bool logScores = true;

    private WebCamTexture webcamTexture;
    private bool usingSharedCamera;
    private CvUnity.TextureConversionParams textureParams;
    private Mat template1Mat;
    private Mat template2Mat;
    private Mat template3Mat;
    private int heldNumber;
    private float holdTimer;
    private float lastUseTime = -999f;
    private float nextProcessTime;

    private void Awake()
    {
        textureParams = new CvUnity.TextureConversionParams
        {
            FlipVertically = flipVertically,
            FlipHorizontally = flipHorizontally,
            RotationAngle = rotationAngle
        };
    }

    private void OnEnable()
    {
        BuildTemplates();

        if (startCameraOnEnable)
            StartCamera();
    }

    private void OnDisable()
    {
        StopCamera();
        ReleaseTemplates();
    }

    private void Update()
    {
        if (!detectionEnabled)
            return;

        TryUseSharedCamera();

        if (webcamTexture == null || !webcamTexture.isPlaying)
        {
            UpdateStatus("OpenCV 攝影機未啟動，可能被 MediaPipe 佔用");
            return;
        }

        if (webcamTexture.width <= 16 || webcamTexture.height <= 16)
        {
            UpdateStatus("OpenCV 等待攝影機畫面");
            return;
        }

        if (Time.unscaledTime < nextProcessTime)
            return;

        nextProcessTime = Time.unscaledTime + Mathf.Max(0.02f, processInterval);

        int number = DetectNumber(out float confidence);
        currentNumber = number;
        currentConfidence = confidence;

        if (number <= 0)
        {
            heldNumber = 0;
            holdTimer = 0f;
            UpdateStatus("OpenCV 未偵測到數字卡");
            return;
        }

        if (number != heldNumber)
        {
            heldNumber = number;
            holdTimer = 0f;
        }

        holdTimer += processInterval;
        UpdateStatus("OpenCV 數字卡 " + number + " " + confidence.ToString("0.00"));

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
        if (TryUseSharedCamera())
            return;

        if (webcamTexture != null && webcamTexture.isPlaying)
            return;

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices == null || devices.Length == 0)
        {
            Debug.LogWarning("OpenCVNumberCardDetector 找不到攝影機");
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
        usingSharedCamera = false;

        if (debugPreview != null)
            debugPreview.texture = webcamTexture;
    }

    public void StopCamera()
    {
        if (webcamTexture == null)
            return;

        if (!usingSharedCamera && webcamTexture.isPlaying)
            webcamTexture.Stop();

        if (debugPreview != null && debugPreview.texture == webcamTexture)
            debugPreview.texture = null;

        webcamTexture = null;
        usingSharedCamera = false;
    }

    private bool TryUseSharedCamera()
    {
        if (sharedCameraPreview == null)
            return false;

        WebCamTexture sharedTexture = sharedCameraPreview.texture as WebCamTexture;
        if (sharedTexture == null)
            return false;

        webcamTexture = sharedTexture;
        usingSharedCamera = true;

        if (debugPreview != null)
            debugPreview.texture = sharedTexture;

        return webcamTexture.isPlaying;
    }

    private void BuildTemplates()
    {
        ReleaseTemplates();
        template1Mat = BuildTemplate(number1Template);
        template2Mat = BuildTemplate(number2Template);
        template3Mat = BuildTemplate(number3Template);
    }

    private Mat BuildTemplate(Texture2D texture)
    {
        if (texture == null)
            return null;

        using (Mat source = CvUnity.TextureToMat(texture))
        {
            return PreprocessForCompare(source);
        }
    }

    private int DetectNumber(out float confidence)
    {
        confidence = 0f;

        if (template1Mat == null && template2Mat == null && template3Mat == null)
        {
            UpdateStatus("OpenCV 至少要拖一張數字模板");
            return 0;
        }

        using (Mat frame = CvUnity.TextureToMat(webcamTexture, textureParams))
        using (Mat roi = CropCenter(frame))
        using (Mat compare = PreprocessForCompare(roi))
        {
            float score1 = template1Mat != null ? MatchScore(compare, template1Mat) : -1f;
            float score2 = template2Mat != null ? MatchScore(compare, template2Mat) : -1f;
            float score3 = template3Mat != null ? MatchScore(compare, template3Mat) : -1f;

            int number = 0;
            confidence = -1f;

            if (score1 > confidence)
            {
                confidence = score1;
                number = 1;
            }

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

            if (logScores)
                Debug.Log($"OpenCV 數字卡分數 1={score1:0.00} 2={score2:0.00} 3={score3:0.00}");

            return number > 0 && confidence >= confidenceThreshold ? number : 0;
        }
    }

    private Mat CropCenter(Mat frame)
    {
        int width = frame.Width;
        int height = frame.Height;
        int roiWidth = Mathf.Clamp(Mathf.RoundToInt(width * sampleWidth), 1, width);
        int roiHeight = Mathf.Clamp(Mathf.RoundToInt(height * sampleHeight), 1, height);
        int x = Mathf.Clamp((width - roiWidth) / 2, 0, width - roiWidth);
        int y = Mathf.Clamp((height - roiHeight) / 2, 0, height - roiHeight);

        OpenCvSharp.Rect rect = new OpenCvSharp.Rect(x, y, roiWidth, roiHeight);
        return new Mat(frame, rect).Clone();
    }

    private Mat PreprocessForCompare(Mat source)
    {
        Mat gray = new Mat();
        Mat resized = new Mat();
        Mat blurred = new Mat();
        Mat binary = new Mat();

        try
        {
            if (source.Channels() == 1)
                gray = source.Clone();
            else
                Cv2.CvtColor(source, gray, ColorConversionCodes.BGR2GRAY);

            Cv2.Resize(gray, resized, new OpenCvSharp.Size(compareWidth, compareHeight));
            Cv2.GaussianBlur(resized, blurred, new OpenCvSharp.Size(3, 3), 0);
            Cv2.Threshold(blurred, binary, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            return binary.Clone();
        }
        finally
        {
            gray.Dispose();
            resized.Dispose();
            blurred.Dispose();
            binary.Dispose();
        }
    }

    private float MatchScore(Mat compare, Mat template)
    {
        using (Mat result = new Mat())
        {
            Cv2.MatchTemplate(compare, template, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out _, out double maxValue, out _, out _);
            return Mathf.Clamp01((float)maxValue);
        }
    }

    private void UseNumber(int number)
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("OpenCVNumberCardDetector 找不到 InventoryManager");
            return;
        }

        int slotIndex = Mathf.Clamp(number - 1, 0, 2);
        InventoryManager.Instance.UseItem(slotIndex);
        Debug.Log("OpenCV 數字卡使用物品欄格子：" + number);
    }

    private void ReleaseTemplates()
    {
        template1Mat?.Dispose();
        template2Mat?.Dispose();
        template3Mat?.Dispose();
        template1Mat = null;
        template2Mat = null;
        template3Mat = null;
    }

    private void UpdateStatus(string text)
    {
        if (statusText != null)
            statusText.text = text;
    }
}
