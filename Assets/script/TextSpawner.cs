using UnityEngine;

public class TextSpawner : MonoBehaviour
{
    public GameObject textPrefab;
    public RectTransform canvasRect;

    public string[] messages =
    {
        "加油！",
        "太棒了",
        "安安",
        "繼續保持",
        "燃燒脂肪"
    };

    public float spawnRate = 2f;
    public float startDelay = 1f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnText), startDelay, spawnRate);
    }

    void SpawnText()
    {
        if (textPrefab == null || canvasRect == null) return;

        // ✅ 1. 生成
        GameObject obj = Instantiate(textPrefab);

        // ✅ 2. 強制放進 Canvas（關鍵）
        obj.transform.SetParent(canvasRect, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        HorizontalFlyText fly = obj.GetComponent<HorizontalFlyText>();

        // ✅ 3. 取得 Canvas 範圍
        float halfWidth = canvasRect.rect.width / 2f;

        // 👉 從右邊生成（Canvas內邊界）
        float startX = halfWidth - 50f;

        // 👉 固定高度（Canvas中心）
        float startY = 0f;

        string msg = messages[Random.Range(0, messages.Length)];

        fly.Initialize(msg, -500f, startX, startY);
    }
}