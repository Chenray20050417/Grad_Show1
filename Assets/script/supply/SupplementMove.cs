using UnityEngine;

public class SupplementMove : MonoBehaviour
{
    public enum MoveMode
    {
        ChaseTarget,
        Direction
    }

    [Header("移動模式")]
    public MoveMode moveMode = MoveMode.ChaseTarget;

    [Header("移動速度")]
    public float moveSpeed = 3f;

    [Header("追蹤玩家")]
    public Transform target;

    [Header("Y 偏移")]
    public float targetYOffset = 0f;

    [Header("水平移動")]
    public Vector2 moveDirection = Vector2.left;

    [Header("旋轉速度")]
    public float rotateSpeed = 180f;

    [Header("自動消失")]
    public float destroyAfterSeconds = 12f;

    private float aliveTimer;

    void Update()
    {
        aliveTimer += Time.deltaTime;
        if (destroyAfterSeconds > 0f && aliveTimer >= destroyAfterSeconds)
        {
            Destroy(gameObject);
            return;
        }

        // 旋轉效果
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        if (moveMode == MoveMode.Direction)
        {
            Vector2 direction = moveDirection.sqrMagnitude > 0.0001f ? moveDirection.normalized : Vector2.left;
            transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
            return;
        }

        // 沒有目標就往下掉
        if (target == null)
        {
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;
            return;
        }

        // 玩家位置
        Vector3 targetPos =
            target.position +
            new Vector3(0f, targetYOffset, 0f);

        // 朝玩家移動
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );
    }
}
