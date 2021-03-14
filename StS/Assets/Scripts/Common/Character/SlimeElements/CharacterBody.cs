using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class CharacterBody : MonoBehaviour
{
    // Classe imbriqué

    [Serializable] public enum BodyType
    {
        BodyBan,
        BodyByakusei,
        BodyDaisuke,
        BodyGento,
        BodyGinga,
        BodyMeguri,
        BodyOkuto,
        BodyPao,
        BodyPon,
        BodyRyuko,
        BodyWaruga,
        BodyYukimone
    }


    // Attributs

    [SerializeField] private BodyType m_BodyType;
    [TextArea][SerializeField] private string m_BodyStory;

    [SerializeField] private Transform ValidArea;

    [SerializeField] private ParticleSystem m_AttackSuccess;
    [SerializeField] private ParticleSystem m_AttackFailure;

    [SerializeField] private Image PowerLogo;

    private Animator Animator;

    private CharacterPlayer AssociatedCharacterPlayer;


    // Life cycle

    private void Awake()
    {
        Animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Animator == null)
        {
            Debug.Log("outch");
        }
    }

    // Requete

    public BodyType GetBodyType()
    {
        return m_BodyType;
    }

    public string GetStoryBody()
    {
        return m_BodyStory;
    }

    public Transform GetValidArea()
    {
        return ValidArea;
    }


    // Methods

    public void SetAssociatedCharacterPlayer(CharacterPlayer cp)
    {
        AssociatedCharacterPlayer = cp;
    }

    public void IsRunning(bool b)
    {
        Animator.SetBool("IsRunning", b);
    }

    public void SetPowerLogo(Sprite logo)
    {
        PowerLogo.sprite = logo;
    }

    #region Start attack

    public void StartAttackFire()
    {
        Animator.SetTrigger("StartAttackFire");
    }

    public void StartAttackEarth()
    {
        Animator.SetTrigger("StartAttackEarth");
    }

    public void StartAttackWater()
    {
        Animator.SetTrigger("StartAttackWater");
    }

    public void StartAttackPower()
    {
        Animator.SetTrigger("StartAttackPower");
    }

    #endregion

    #region Trigger attack

    public void TriggerAttackFire()
    {
        AssociatedCharacterPlayer.TriggeredAttackFire();
    }

    public void TriggerAttackWater()
    {
        AssociatedCharacterPlayer.TriggerAttackWater();
    }

    public void TriggerAttackEarth()
    {
        AssociatedCharacterPlayer.TriggeredAttackEarth();
    }

    public void TriggerAttackPower()
    {
        AssociatedCharacterPlayer.TriggerAttackPower();
    }

    #endregion

    #region Attack Effect

    public void Animation_AttackSuccess()
    {
        m_AttackSuccess.Play();
    }

    public void Animation_AttackFailure()
    {
        m_AttackFailure.Play();
    }

    #endregion
}
