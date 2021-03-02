using System;
using UnityEngine;

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


    // Constants

    private static readonly int ATTACK_COUNT = 1;


    // Attributs

    [SerializeField] private BodyType m_BodyType;
    [TextArea][SerializeField] private string m_BodyStory;

    private Animator Animator;


    // Life cycle

    private void Awake()
    {
        Animator = GetComponent<Animator>();
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


    // Methods

    public void IsRunning(bool b)
    {
        Animator.SetBool("IsRunning", b);
    }

    public void Attack()
    {
        Animator.SetInteger("AttackIndex", UnityEngine.Random.Range(0, ATTACK_COUNT));
        Animator.SetTrigger("Attack");
    }
}
