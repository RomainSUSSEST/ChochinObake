using UnityEngine;
using ServerManager;

public class GongEffect : Effect
{
    #region Attributes

    [SerializeField] private ParticleSystem m_SmokeEffect;
    [SerializeField] private Animator m_Animator;

    [SerializeField] private LightProjectiles m_LightProjectilePrefab;

    [SerializeField] private CharacterServer m_AssociatedCharacterServer;

    #endregion

    #region Tools

    protected override void StartEffect()
    {
        m_Animator.SetTrigger("Play");
        m_SmokeEffect.Play();

        if (m_AssociatedCharacterServer.IsAI)
        {
            SfxManager.Instance.AIPlaySfx(m_AssociatedCharacterServer.AssociatedAIManager.GetAssociatedProfil().Name, SfxManager.Instance.GongAppear);
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

        foreach (CharacterServer c in m_AssociatedCharacterServer.GetCurrentTargets())
        {
            LightProjectiles projectile = Instantiate(m_LightProjectilePrefab, transform.position, Quaternion.identity);
            projectile.Target = c.GetLightProjectilesTarget();
            projectile.AssociatedCharacter = m_AssociatedCharacterServer;
        }
    }

    #endregion
}
