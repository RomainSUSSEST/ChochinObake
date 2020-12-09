using UnityEngine;

public class Obstacle : MonoBehaviour
{
    #region Life Cycle

    private void Update()
    {
        if (transform.position.z <= Ground.DESTROY_Z_POSITION)
        {
            Destroy(this.gameObject);
        }

        transform.Translate(new Vector3(0, 0, -Ground.MOVE_SPEED * Time.deltaTime));
    }

    private void Start()
    {
        transform.Translate(new Vector3(0, 0, -Ground.MOVE_SPEED * Time.deltaTime)); // Initialisation
    }

    #endregion
}