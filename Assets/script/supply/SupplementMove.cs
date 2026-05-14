using UnityEngine;

public class SupplementMove : MonoBehaviour
{
    [Header("移動速度")]
    public float moveSpeed = 3f;

    [Header("追蹤玩家")]
    public Transform target;

    [Header("Y 偏移")]
    public float targetYOffset = 0f;

    [Header("旋轉速度")]
    public float rotateSpeed = 180f;

    void Update()
    {
        // 旋轉效果
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        // 沒有目標就往下掉
        if (target == null)
        {
            transform.position +=
                Vector3.down * moveSpeed * Time.deltaTime;

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