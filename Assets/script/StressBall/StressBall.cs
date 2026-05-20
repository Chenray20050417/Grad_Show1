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
    private Collider2D stressBallCollider;

    private void Awake()
    {
        stressBallCollider = GetComponent<Collider2D>();
    }

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

        // 出拳/肩推成功：壓力球消失，不扣血
        if (other.CompareTag("PlayerPush") ||
            other.CompareTag("PushHitBox") ||
            other.GetComponentInParent<PushHitBoxController>() != null)
        {
            DestroySelf(true);
            return;
        }

        // 撞到玩家身體
        if (other.CompareTag("Player"))
        {
            // 只有壓力球真的碰到啟用中的攻擊 HitBox，才算成功打掉。
            // 避免右邊出拳時，左邊壓力球撞到玩家卻被全域攻擊狀態擋掉。
            if (IsTouchingActivePushHitBox())
            {
                DestroySelf(true);
                return;
            }

            // 沒有肩推：扣血 + 血條震動
            HealthUI healthUI = HealthUI.Instance;

            if (healthUI == null)
            {
                healthUI = FindObjectOfType<HealthUI>();
            }

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

    private bool IsTouchingActivePushHitBox()
    {
        PushHitBoxController push = FindObjectOfType<PushHitBoxController>();

        if (push == null ||
            !push.isAttacking ||
            push.hitBox == null ||
            !push.hitBox.enabled ||
            stressBallCollider == null)
        {
            return false;
        }

        return stressBallCollider.IsTouching(push.hitBox);
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
