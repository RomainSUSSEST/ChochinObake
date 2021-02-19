using UnityEngine;

public class EndMapArrival : MonoBehaviour
{
    #region Life Cycle

    private void Update()
    {
        // On avance
        transform.Translate(new Vector3(0, 0, -Ground.MOVE_SPEED * Time.deltaTime));

        if (transform.localPosition.z <= 0)
        {
            Ground.MOVE_SPEED = 0;
        }
    }

    #endregion
}