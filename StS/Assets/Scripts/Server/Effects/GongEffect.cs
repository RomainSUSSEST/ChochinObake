using UnityEngine;
using ServerManager;

public class GongEffect : Effect
{
    #region Attributes

    [SerializeField] private ParticleSystem m_SmokeEffect;
    [SerializeField] private Animator m_Animator;

    [SerializeField] private CharacterServer m_AssociatedCharacterServer;

    #endregion

    #region Tools

    protected override void StartEffect()
    {
        m_Animator.SetTrigger("Play");
        m_SmokeEffect.Play();
        SfxManager.Instance.PlaySfx(SfxManager.Instance.GongAppear);
    }

    private void AnimationEnd()
    {
        transform.gameObject.SetActive(false);
    }

    private void GongHit()
    {
        SfxManager.Instance.PlaySfx(SfxManager.Instance.GongHit);
        m_AssociatedCharacterServer.PowerStart();
    }

    #endregion
}
