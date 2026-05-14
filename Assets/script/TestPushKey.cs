using UnityEngine;

public class TestPushKey : MonoBehaviour
{
    public Animator animator;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.Play("squat2", 0, 0f);
        }
    }
}