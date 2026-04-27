using UnityEngine;

public class LegacyHoverButton : MonoBehaviour
{
    public Rect buttonRect = new Rect(100, 100, 50, 50);
    public string buttonText = "開始";
    private bool isHover = false;
    private float slideOffset = 100f;
    private float currentOffset = 100f;

    void OnGUI()
    {
        // 判斷滑鼠是否在按鈕上
        Vector2 mousePos = Event.current.mousePosition;
        isHover = buttonRect.Contains(mousePos);

        // 平滑滑動
        float target = isHover ? 0 : slideOffset;
        currentOffset = Mathf.Lerp(currentOffset, target, Time.deltaTime * 10f);

        // 畫按鈕（只有圖示）
        if (GUI.Button(buttonRect, "圖示")) { Debug.Log("Click!"); }

        // 畫滑出文字
        Rect textRect = new Rect(buttonRect.xMax + currentOffset, buttonRect.y, 60, buttonRect.height);
        GUI.Label(textRect, buttonText);
    }
}