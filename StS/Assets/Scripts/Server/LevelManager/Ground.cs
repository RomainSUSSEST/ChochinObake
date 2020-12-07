using SDD.Events;
using UnityEngine;

public class Ground : MonoBehaviour
{
    // Attributs

    [SerializeField] private SlimeBody AssociatedSlimeBody;
    
    public static float MOVE_SPEED { get; set; }
    public static float DESTROY_Z_POSITION { get; set; }


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

    private void Start()
    {
        transform.Translate(new Vector3(0, 0, -MOVE_SPEED * Time.deltaTime)); // Initialisation
    }

    #endregion

    #region Request

    public SlimeBody GetAssociatedSlimeBody()
    {
        return AssociatedSlimeBody;
    }

    #endregion
}
