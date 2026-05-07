using System.Collections;
using UnityEngine;

public class PushHitBoxController : MonoBehaviour
{
    public float activeTime = 0.25f;

    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.enabled = false;
    }

    public void ActivateHitBox()
    {
        StopAllCoroutines();
        StartCoroutine(OpenHitBox());
    }

    IEnumerator OpenHitBox()
    {
        col.enabled = true;
        yield return new WaitForSeconds(activeTime);
        col.enabled = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        BoxCollider2D box = GetComponent<BoxCollider2D>();

        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.offset, box.size);
        }
    }
}