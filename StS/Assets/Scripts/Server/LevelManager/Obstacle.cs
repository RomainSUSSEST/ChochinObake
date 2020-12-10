using UnityEngine;

public abstract class Obstacle : MonoBehaviour
{
    #region Enum

    public enum InputAction { SWIPE_UP, SWIPE_RIGHT, SWIPE_BOTTOM, SWIPE_LEFT, DOUBLE_CLICK }

    #endregion

    #region Attributs

    [Header("Input to success")]

    [SerializeField] private InputAction InputActionToDodge;

    private SlimeServer AssociatedSlime;

    #endregion

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

    #region Methode

    public void SetAssociatedSlime(SlimeServer ss)
    {
        AssociatedSlime = ss;
    }

    #endregion
}