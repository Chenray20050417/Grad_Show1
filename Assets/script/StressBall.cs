using UnityEngine;

public class StressBall : MonoBehaviour
{
    [Header("速度")]
    public float moveSpeed = 8f;

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
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;

        if (other.CompareTag("PlayerPush"))
        {
            isDestroyed = true;
            Destroy(gameObject);
        }
    }
}