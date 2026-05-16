using UnityEngine;

public class StressBall : MonoBehaviour
{
    [Header("速度")]
    public float moveSpeed = 8f;

    [Header("傷害")]
    public float damage = 10f;

    [Header("破碎特效")]
    public GameObject breakEffectPrefab;
    public Sprite breakEffectSprite;
    public float breakEffectScale = 1f;
    public float breakEffectLifetime = 0.35f;

    private Vector2 moveDirection;
    private bool isDestroyed = false;

    public void SetDirection(Vector2 dir)
    {
        moveDirection = dir.normalized;
    }

    void Update()
    {
        if (isDestroyed) return;

        transform.Translate(
            moveDirection * moveSpeed * Time.deltaTime,
            Space.World
        );

        if (transform.position.y < 150f ||
            transform.position.x < 50f ||
            transform.position.x > 170f)
        {
            DestroySelf(false);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;

        // 肩推成功：壓力球消失，不扣血
        if (other.CompareTag("PlayerPush"))
        {
            DestroySelf(true);
            return;
        }

        // 撞到玩家身體
        if (other.CompareTag("Player"))
        {
            PushHitBoxController push =
                FindObjectOfType<PushHitBoxController>();

            // 肩推狀態中：壓力球消失，不扣血
            if (push != null && push.isAttacking)
            {
                DestroySelf(true);
                return;
            }

            // 沒有肩推：扣血 + 血條震動
            HealthUI healthUI =
                FindObjectOfType<HealthUI>();

            if (healthUI != null)
            {
                healthUI.TakeDamage(damage);
            }

            // 玩家閃紅
            PlayerDamageEffect effect =
                FindObjectOfType<PlayerDamageEffect>();

            if (effect != null)
            {
                effect.FlashRed();
            }

            DestroySelf(true);
            return;
        }
    }

    private void DestroySelf(bool playBreakEffect)
    {
        if (isDestroyed) return;

        isDestroyed = true;

        if (playBreakEffect && breakEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                breakEffectPrefab,
                transform.position,
                Quaternion.identity
            );

            effect.transform.localScale =
                transform.lossyScale * breakEffectScale;

            StressBallBreakEffect breakEffect =
                effect.GetComponent<StressBallBreakEffect>();

            if (breakEffect != null)
            {
                breakEffect.lifetime = breakEffectLifetime;
                breakEffect.Show(breakEffectSprite);
            }
            else
            {
                Destroy(effect, breakEffectLifetime);
            }
        }

        Destroy(gameObject);
    }
}
