using UnityEngine;

public class SquatHitBox : MonoBehaviour
{
    public Collider2D hitBox;
    public float activeTime = 0.2f;

    private float timer;

    void Awake()
    {
        if (hitBox == null)
            hitBox = GetComponent<Collider2D>();

        hitBox.enabled = false;
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                hitBox.enabled = false;

                Debug.Log("HitBox 關閉");
            }
        }
    }

    public void OpenHitBox()
    {
        hitBox.enabled = true;
        timer = activeTime;

        Debug.Log("HitBox 開啟");
    }
}