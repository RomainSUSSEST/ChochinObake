using UnityEngine;

public class InputActionValidArea : MonoBehaviour
{
    // Attributs

    [SerializeField] private SlimeBody.BodyType AssociatedBody;


    // Requete

    public SlimeBody.BodyType GetAssociatedBody()
    {
        return AssociatedBody;
    }
}
