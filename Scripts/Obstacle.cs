using UnityEngine;


public class Obstacle : MonoBehaviour
{
    private Animator animator;

    public void OnCollisionEnter2D(Collision2D collision)
    {
        SgLib.SoundManager.Instance.PlaySound(SgLib.SoundManager.Instance.tick);
        animator.Play("Contract");
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
}
