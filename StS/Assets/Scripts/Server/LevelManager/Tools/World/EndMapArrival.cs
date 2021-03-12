using SDD.Events;
using UnityEngine;

public class EndMapArrival : MonoBehaviour
{
    #region Life Cycle

    private void Update()
    {
        // On avance
        transform.Translate(new Vector3(0, 0, -Ground.MOVE_SPEED * Time.deltaTime));

        if (transform.localPosition.z <= 0) // Car enfant du world
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
            // On arrete les déplacements
            Ground.MOVE_SPEED = 0;
            Background.MOVE_SPEED = 0;

            EventManager.Instance.Raise(new RoundEndEvent());
        }
    }

    #endregion
}