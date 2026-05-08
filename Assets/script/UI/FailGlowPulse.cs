using UnityEngine;
using UnityEngine.UI;

public class FailGlowPulse : MonoBehaviour
{
    public float speed = 4f;
    public float minAlpha = 0.15f;
    public float maxAlpha = 0.55f;

    Image img;

    void Start()
    {
        img = GetComponent<Image>();
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;

        Color c = img.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        img.color = c;
    }
}