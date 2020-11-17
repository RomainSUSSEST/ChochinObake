using System;
using UnityEngine;

public class SlimeBody : MonoBehaviour
{
    // Classe imbriqué

    [Serializable] public enum BodyType
    {
        BlackBody,
        BlueBody,
        BrownBody,
        DarkYellowBody,
        FluoGreenBody,
        GreenBody,
        GreyBody,
        KhakiBody,
        LightBlueBody,
        LightGreenBody,
        LightYellowBody,
        OrangeBody,
        PinkBody,
        PurpleBody,
        RedBody,
        WhiteBody
    }


    // Attributs

    [SerializeField] private BodyType m_BodyType;
    [SerializeField] private GameObject Tentacles;

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

    public BodyType GetBodyType()
    {
        return m_BodyType;
    }


    // Méthode

    public void SetSlimeRoot(Slime root)
    {
        Root = root;
    }

    public void TentaclesRotateLeft()
    {
        Tentacles.transform.Rotate(90, 0, 0);
    }

    public void TentaclesRotateRight()
    {
        Tentacles.transform.Rotate(-90, 0, 0);
    }


    #region Animation Event
    public void AnimationEvent_JumpStart()
    {
        Root.StartJumpPhysic();
    }

    public void AnimationEvent_LiquefactionEnd()
    {
        Root.LiquefactionEnd();
    }

    public void AnimationEvent_AttackEnd()
    {
        Root.AttackEnd();
    }
    #endregion
}
