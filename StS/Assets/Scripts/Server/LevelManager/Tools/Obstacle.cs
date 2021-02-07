using UnityEngine;

public class Obstacle : MonoBehaviour
{
    #region Constants

    public static float DESTROY_Z_POSITION { get; set; } // Position de destruction

    #endregion

    #region Attributs

    [Header("Input to success")]
    [SerializeField] private InputActionValidArea.InputAction Type;
    [SerializeField] private Transform InputAction_Spawner;

    private CharacterServer AssociatedSlime;

    #endregion

    #region Life Cycle

    private void Update()
    {
        if (transform.position.z <= DESTROY_Z_POSITION)
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

        // On avance l'obstacle
        transform.Translate(new Vector3(0, 0, -Ground.MOVE_SPEED * Time.deltaTime));
    }

    #endregion

    #region Requete

    public InputActionValidArea.InputAction GetInput()
    {
        return Type;
    }

    #endregion

    #region Methode

    /// <summary>
    /// Enregistre le slime référent et le notifie que l'on s'associe à lui en s'enregistrant
    /// </summary>
    /// <param name="ss"> Le slime auquel s'associer </param>
    public void SetAssociatedSlime(CharacterServer ss)
    {
        AssociatedSlime = ss;
        ss.RegisterObstacle(this);
    }

    public Vector3 GetCurrentInputActionAreaPosition()
    {
        return InputAction_Spawner.transform.position;
    }

    #endregion
}