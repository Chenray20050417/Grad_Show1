using UnityEngine;
using System.Collections;

public class PushHitBoxController : MonoBehaviour
{
    public float activeTime = 0.4f;

    [HideInInspector]
    public bool isAttacking = false;

    private Collider2D hitbox;

    void Start()
    {
        hitbox = GetComponent<Collider2D>();
        hitbox.enabled = false;
    }

    public void ActivateHitBox()
    {
        StopAllCoroutines();
        StartCoroutine(HitboxRoutine());
    }

    IEnumerator HitboxRoutine()
    {
        isAttacking = true;
        hitbox.enabled = true;

        yield return new WaitForSeconds(activeTime);

        hitbox.enabled = false;
        isAttacking = false;
    }
}