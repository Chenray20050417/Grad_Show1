using UnityEngine;
using System.Collections;

public class PlayerDamageEffect : MonoBehaviour
{
    public SpriteRenderer playerSprite;

    public float flashTime = 0.15f;

    private Color originalColor;

    void Start()
    {
        originalColor = playerSprite.color;
    }

    public void FlashRed()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        playerSprite.color = Color.red;

        yield return new WaitForSeconds(flashTime);

        playerSprite.color = originalColor;
    }
}