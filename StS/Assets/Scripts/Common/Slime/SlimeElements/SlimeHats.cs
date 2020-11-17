using System;
using UnityEngine;

public class SlimeHats : MonoBehaviour
{
    // Type imbriquée

    [Serializable] public enum HatsType
    {
        Arrows,
        BaseBall,
        Billiard,
        Cactus,
        Chrismas,
        Cupidon,
        Demon,
        Dinosaure,
        Hachoir,
        Plonge,
        PoketBall,
        TV
    }


    // Attributs

    [SerializeField] private HatsType m_HatsType;

    private Slime Root;


    // Life Cycle

    private void Start()
    {
        if (Root == null)
        {
            throw new Exception("Root non initialisé");
        }
    }


    // Requete

    public HatsType GetHatsType()
    {
        return m_HatsType;
    }


    // Méthode

    public void SetSlimeRoot(Slime root)
    {
        Root = root;
    }
}
