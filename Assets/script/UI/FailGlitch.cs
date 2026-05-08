using UnityEngine;

public class FailGlitch : MonoBehaviour
{
    RectTransform rect;
    CanvasGroup cg;

    Vector3 originPos;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();

        originPos = rect.localPosition;

        InvokeRepeating(nameof(Glitch), 0f, 0.05f);
    }

    void Glitch()
    {
        // 閃爍透明度
        cg.alpha = Random.Range(0.75f, 1f);

        // 微縮放
        float scale = Random.Range(0.97f, 1.05f);

        rect.localScale = new Vector3(scale, scale, 1);

        // 微震動
        rect.localPosition = originPos + new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-2f, 2f),
            0
        );
    }
}