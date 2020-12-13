using SDD.Events;
using UnityEngine;

public class Ground : MonoBehaviour
{
    // Attributs

    [SerializeField] private SlimeBody.BodyType AssociatedSlimeBody;
    
    public static float MOVE_SPEED { get; set; } // Vitesse unit/s
    public static float DESTROY_Z_POSITION { get; set; } // Position de destruction


    #region Life Cycle

    private void Update()
    {
        if (transform.position.z <= DESTROY_Z_POSITION)
        {
            EventManager.Instance.Raise(new GroundEndMapEvent());
            Destroy(this.gameObject);
        }

        transform.Translate(new Vector3(0, 0, -MOVE_SPEED * Time.deltaTime));
    }

    #endregion

    #region Request

    public SlimeBody.BodyType GetAssociatedSlimeBody()
    {
        return AssociatedSlimeBody;
    }

    #endregion
}
