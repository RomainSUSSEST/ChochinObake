using UnityEngine;

public class Obstacle : MonoBehaviour
{
    // Enum

    public enum Elements : int { WATER, EARTH, AIR, FIRE }

    #region Constants

    public static float DESTROY_Z_POSITION { get; set; } // Position de destruction

    #endregion

    #region Attributs

    [Header("Input to success")]
    [SerializeField] private Elements Type;

    private CharacterServer AssociatedCharacter;

    private float SizePer2_Z; // Taille sur l'axe Z de l'obstacle divisé par 2.

    #endregion

    #region Life Cycle

    private void Start()
    {
        SizePer2_Z = GetComponent<Renderer>().bounds.size.z / 2;
    }

    private void Update()
    {
        if (transform.position.z <= DESTROY_Z_POSITION)
        {
            Destroy(this.gameObject);
        }

        // On avance l'obstacle
        transform.Translate(new Vector3(0, 0, -Ground.MOVE_SPEED * Time.deltaTime));
    }
    #endregion

    #region Requete

    public float GetSizePer2_Z()
    {
        return SizePer2_Z;
    }

    public Elements GetElement()
    {
        return Type;
    }

    #endregion

    #region Methode

    /// <summary>
    /// Enregistre le joueur référent et le notifie que l'on s'associe à lui en s'enregistrant
    /// </summary>
    /// <param name="ss"> Le joueur auquel s'associer </param>
    public void SetAssociatedCharacter(CharacterServer ss)
    {
        AssociatedCharacter = ss;
        ss.RegisterObstacle(this);
    }

    #endregion
}