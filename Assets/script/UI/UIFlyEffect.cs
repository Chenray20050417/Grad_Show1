using UnityEngine;
using System.Collections;

public class UIFlyEffect : MonoBehaviour
{
    public static UIFlyEffect Instance;

    [Header("飛行圖片")]
    public RectTransform flyImage;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayFlyEffect(Vector3 worldPos, RectTransform targetSlot)
    {
        if (flyImage == null || targetSlot == null) return;

        StopAllCoroutines();
        StartCoroutine(Fly(worldPos, targetSlot));
    }

    IEnumerator Fly(Vector3 worldPos, RectTransform target)
    {
        flyImage.gameObject.SetActive(true);

        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        flyImage.position = screenPos;

        float t = 0f;
        Vector3 start = flyImage.position;
        Vector3 end = target.position;

        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            flyImage.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        target.localScale = Vector3.one * 1.2f;
        yield return new WaitForSeconds(0.1f);
        target.localScale = Vector3.one;

        flyImage.gameObject.SetActive(false);
    }
}