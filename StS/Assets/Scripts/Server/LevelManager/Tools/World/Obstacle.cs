using System.Collections;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    // Enum

    public enum Elements : int { WATER, EARTH, AIR, FIRE }

    public enum Statut { EARLY, SUCCESS, MISS }

    #region Constants

    public static float DESTROY_Z_POSITION { get; set; } // Position de destruction

    #endregion

    #region Attributs

    [Header("Input to success")]
    [SerializeField] private Elements Type;

    [SerializeField] private GameObject Kanji;
    [SerializeField] private float ValidInputArea_Threshold;
    [SerializeField] private float ValidInputArea_Delai; // min 0
    [SerializeField] private GameObject InternValidInput;
    [SerializeField] private GameObject ExternValidInput;
    [SerializeField] private Material ValidInputMaterial;

    private CharacterServer AssociatedCharacter;

    private float PositionStart; // Position du début
    private float PositionEnd; // Position d'arrivé prévu.

    private Statut m_Statut;

    #endregion

    #region Life Cycle

    private void Start()
    {
        PositionStart = transform.position.z;
        PositionEnd = AssociatedCharacter.GetCharacterBody().GetValidArea().transform.position.z;

        StartCoroutine("IndicatorManagement");
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

    public Elements GetElement()
    {
        return Type;
    }

    public Statut GetStatut()
    {
        return m_Statut;
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

    /// <summary>
    /// Détruit le kanji de l'osbtacle
    /// </summary>
    public void DestroyKanji()
    {
        Destroy(Kanji.gameObject);
    }

    #endregion

    #region Coroutine

    private IEnumerator IndicatorManagement()
    {
        m_Statut = Statut.EARLY;

        float tampon;
        do
        {
            // Gestion du curseur indiquant le timing pour appuyer.
            tampon = Mathf.InverseLerp(PositionStart, PositionEnd, transform.position.z);

            ExternValidInput.transform.localScale = Vector3.one * (3 - tampon * tampon * 2);

            yield return new CoroutineTools.WaitForFrames(1);
        }
        while (tampon < ValidInputArea_Threshold);
        
        // On peut appuyer

        InternValidInput.GetComponent<Renderer>().material = ValidInputMaterial;
        ExternValidInput.GetComponent<Renderer>().material = ValidInputMaterial;

        m_Statut = Statut.SUCCESS;

        yield return new WaitForSeconds(ValidInputArea_Delai);

        m_Statut = Statut.MISS;
    }

    #endregion
}