using UnityEngine;

public class InvertKanji : Effect
{
    #region Attributes

    [SerializeField] private ParticleSystem m_SmokeEffect;
    [SerializeField] private Animator m_Animator;

    #endregion

    #region Tools

    protected override void StartEffect()
    {
        m_Animator.SetTrigger("Play");
        m_SmokeEffect.Play();
    }

    private void AnimationEnd()
    {
        transform.gameObject.SetActive(false);
    }

    #endregion
}
