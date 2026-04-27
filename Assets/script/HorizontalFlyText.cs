using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 控制文字移動、碰撞與反彈
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Collider2D))]
public class HorizontalFlyText : MonoBehaviour
{
    [Header("移動")]
    public float speed;

    private RectTransform rectTransform;
    private RectTransform canvasRect;
    private TextMeshProUGUI textUI;

    private bool hasHitPlayer = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        textUI = GetComponent<TextMeshProUGUI>();
        canvasRect = rectTransform.parent as RectTransform;
    }

    /// <summary>
    /// 初始化文字
    /// </summary>
    public void Initialize(string message, float moveSpeed, float startX, float startY)
    {
        textUI.text = message;
        speed = moveSpeed;
        rectTransform.anchoredPosition = new Vector2(startX, startY);

        Destroy(gameObject, 5f);
    }

    void Update()
    {
        Move();
        BounceOnWall();
    }

    void Move()
    {
        rectTransform.anchoredPosition += Vector2.right * speed * Time.deltaTime;
    }

    // 撞到畫面邊界反彈
    void BounceOnWall()
    {
        if (canvasRect == null) return;

        float halfWidth = canvasRect.rect.width / 2f;
        float x = rectTransform.anchoredPosition.x;

        if (x < -halfWidth || x > halfWidth)
        {
            speed *= -1;
        }
    }

    // 撞到 Player
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || hasHitPlayer) return;

        hasHitPlayer = true;

        // 反彈
        speed *= -1;

        // 玩家變紅
        SpriteRenderer sr = other.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            StartCoroutine(PlayerFlashRed(sr));
        }
    }

    IEnumerator PlayerFlashRed(SpriteRenderer sr)
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.05f);
        sr.color = Color.white;
    }
}