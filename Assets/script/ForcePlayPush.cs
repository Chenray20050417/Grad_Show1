using UnityEngine;

public class ForcePlayPush : MonoBehaviour
{
    public Animator animator;

    void Start()
    {
        Debug.Log("強制播放 push");
        animator.Play("push", 0, 0f);
        animator.Update(0f);
    }
}