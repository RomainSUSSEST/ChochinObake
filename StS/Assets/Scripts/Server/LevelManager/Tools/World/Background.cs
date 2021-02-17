using SDD.Events;
using UnityEngine;

public class Background : MonoBehaviour
{
    #region Constants

    public static float SIZE = 130;
    public static float MOVE_SPEED { get; set; } // Vitesse unit/s
    public static float DESTROY_Z_POSITION { get; set; } // Position de destruction

    #endregion

    #region Life cycle

    private void Update()
    {
        if (transform.position.z <= DESTROY_Z_POSITION)
        {
            EventManager.Instance.Raise(new BackgroundEndMapEvent());
            Destroy(this.gameObject);
        }

        transform.Translate(new Vector3(0, 0, -MOVE_SPEED * Time.deltaTime));
    }

    #endregion
}
