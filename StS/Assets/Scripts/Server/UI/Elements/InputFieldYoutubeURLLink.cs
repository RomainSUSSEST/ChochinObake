using UnityEngine;

public class InputFieldYoutubeURLLink : MonoBehaviour
{
    // Attributs

    private Animator animator;


    // Life Cycle

    private void Start()
    {
        animator = GetComponent<Animator>();
    }


    // Animation

    public void AnimationIncorrectYoutubeLink_End()
    {
        animator.SetBool("Start", false);
    }
}
