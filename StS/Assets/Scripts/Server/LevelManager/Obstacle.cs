using UnityEngine;

public abstract class Obstacle : MonoBehaviour
{
    #region Attributs

    [Header("Input to success")]
    [SerializeField] private InputActionValidArea.InputAction Type;
    [SerializeField] private Transform InputAction_Spawner;

    private SlimeServer AssociatedSlime;

    #endregion

    #region Life Cycle

    private void Update()
    {
        if (transform.position.z <= Ground.DESTROY_Z_POSITION)
        {
            Destroy(this.gameObject);
        }

        // On avance l'obstacle
        transform.Translate(new Vector3(0, 0, -Ground.MOVE_SPEED * Time.deltaTime));
    }

    private void Start()
    {
        // On décale l'obstacle d'un décalage pour centrer le point de pivot sur le spawner d'input (sur Z)
        transform.Translate(
            new Vector3(0,
                        0,
                        transform.position.z - InputAction_Spawner.transform.position.z)
            ); // Initialisation

        // On instancie l'input action associé, de la bonne couleur
        Instantiate(AssociatedSlime.GetInputAction(Type), InputAction_Spawner);
    }

    #endregion

    #region Methode

    public void SetAssociatedSlime(SlimeServer ss)
    {
        AssociatedSlime = ss;
    }

    #endregion
}