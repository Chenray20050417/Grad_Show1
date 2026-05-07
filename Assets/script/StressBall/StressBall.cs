using UnityEngine;

public class StressBall : MonoBehaviour
{
    [Header("速度")]
    public float moveSpeed = 8f;

    [Header("傷害")]
    public float damage = 10f;

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
            isDestroyed = true;
            Destroy(gameObject);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;

        // 肩推成功：壓力球消失，不扣血
        if (other.CompareTag("PlayerPush"))
        {
            isDestroyed = true;
            Destroy(gameObject);
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
                isDestroyed = true;
                Destroy(gameObject);
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

            isDestroyed = true;
            Destroy(gameObject);
            return;
        }
    }
}