using System;
using UnityEngine;

public class CharacterBody : MonoBehaviour
{
    // Classe imbriqué

    [Serializable] public enum BodyType
    {
        BodyCyan,
        BodyDarkBlue,
        BodyFuschia,
        BodyGreen,
        BodyGreenBlue,
        BodyOrange,
        BodyOrangeClair,
        BodyPink,
        BodyPurple,
        BodyRed,
        BodyWhite,
        BodyYellow
    }


    // Attributs

    [SerializeField] private BodyType m_BodyType;

    private CharacterPlayer Root;


    // Life Cycle

    private void Start()
    {
        if (Root == null)
        {
            throw new Exception("Root non initialisé");
        }
    }


    // Requete

    public BodyType GetBodyType()
    {
        return m_BodyType;
    }


    // Méthode

    public void SetSlimeRoot(CharacterPlayer root)
    {
        Root = root;
    }
}
