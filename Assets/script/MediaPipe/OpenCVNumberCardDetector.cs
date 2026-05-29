using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenCVNumberCardDetector : MonoBehaviour
{
    [Header("Camera")]
    public bool detectionEnabled = false;
    public bool startCameraOnEnable = false;
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
    public bool logScores = false;

    private void OnEnable()
    {
        currentNumber = 0;
        currentConfidence = 0f;
        UpdateStatus("OpenCV 數字卡偵測已停用");
    }

    public void StartCamera()
    {
        UpdateStatus("OpenCV 數字卡偵測已停用");
    }

    public void StopCamera()
    {
        if (debugPreview != null)
            debugPreview.texture = null;
    }

    private void UpdateStatus(string text)
    {
        if (statusText != null)
            statusText.text = text;
    }
}
