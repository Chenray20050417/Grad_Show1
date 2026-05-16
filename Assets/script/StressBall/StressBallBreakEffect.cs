using UnityEngine;

public class StressBallBreakEffect : MonoBehaviour
{
    public float lifetime = 0.35f;

    private SpriteRenderer spriteRenderer;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Show(Sprite sprite)
    {
        timer = 0f;

        if (spriteRenderer != null)
            spriteRenderer.sprite = sprite;
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime;

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
