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

        if (m_AssociatedCharacterServer.IsAI)
        {
            SfxManager.Instance.AIPlaySfx(m_AssociatedCharacterServer.AssociatedAIManager.name, SfxManager.Instance.GongAppear);
        }
        else
        {
            SfxManager.Instance.PlayerPlaySfx(m_AssociatedCharacterServer.AssociedClientID, SfxManager.Instance.GongAppear);
        }
    }

    private void AnimationEnd()
    {
        transform.gameObject.SetActive(false);
    }

    private void GongHit()
    {
        if (m_AssociatedCharacterServer.IsAI)
        {
            SfxManager.Instance.AIPlaySfx(m_AssociatedCharacterServer.AssociatedAIManager.GetAssociatedProfil().Name, SfxManager.Instance.GongHit);
        }
        else
        {
            SfxManager.Instance.PlayerPlaySfx(m_AssociatedCharacterServer.AssociedClientID, SfxManager.Instance.GongHit);
        }

        m_AssociatedCharacterServer.PowerStart();
    }

    #endregion
}
