using System.Collections;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    // Enum

    public enum Elements { WATER, EARTH, FIRE }

    public enum Statut { EARLY, SUCCESS, ECHEC, MISS }

    #region Constants

    public static float DESTROY_Z_POSITION { get; set; } // Position de destruction

    #endregion

    #region Attributs

    [Header("Input to success")]
    [SerializeField] private Elements Type;

    [SerializeField] private GameObject Kanji;

    [SerializeField] private float ValidInputArea_Threshold;
    // Temps en plus après le perfect match
    [SerializeField] private float ValidInputArea_Delai; // min 0
    [SerializeField] private GameObject InternValidInput;
    [SerializeField] private GameObject ExternValidInput;

    [SerializeField] private Material ValidInputMaterial;
    [SerializeField] private Material InvalidInputMaterial;

    [Header("Gameplay")]

    [SerializeField] private Renderer LanternRenderer;
    [SerializeField] private Material LanternON;

    [Header("Malus")]
    [SerializeField] private Renderer KanjiRenderer;
    [SerializeField] private Material OutlinerMaterial;
    [SerializeField] private Material UncolorMaterial;

    private CharacterServer AssociatedCharacter;

    private float PositionStart; // Position du début

    private Statut m_Statut;

    private Coroutine m_IndicatorManagement;

    #endregion

    #region Life Cycle

    private void Start()
    {
        PositionStart = transform.position.z;

        m_IndicatorManagement = StartCoroutine("IndicatorManagement");
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
    /// Arrete la coroutine de gestion du timing.
    /// </summary>
    public void DestroyKanji()
    {
        Destroy(Kanji.gameObject);
        StopCoroutine(m_IndicatorManagement);
        LanternRenderer.material = LanternON;
    }

    public void SetKanjiRendererStatus(bool b)
    {
        KanjiRenderer.enabled = b;
    }

    public void UncolorKanji()
    {
        KanjiRenderer.materials = new Material[2];
        KanjiRenderer.materials[0] = OutlinerMaterial;
        KanjiRenderer.materials[1] = UncolorMaterial;
    }

    public void EchecObstacle()
    {
        m_Statut = Statut.ECHEC;
    }

    #endregion

    #region Coroutine

    private IEnumerator IndicatorManagement()
    {
        m_Statut = Statut.EARLY; // Early

        float tampon;
        do
        {
            // Gestion du curseur indiquant le timing pour appuyer.
            tampon = Mathf.InverseLerp(
                PositionStart,
                AssociatedCharacter.GetCharacterBody().GetValidArea().transform.position.z,
                transform.position.z);

            ExternValidInput.transform.localScale = Vector3.one * (3 - tampon * tampon * 2);

            yield return new CoroutineTools.WaitForFrames(1);
        }
        while (tampon < ValidInputArea_Threshold);

        Renderer Intern;
        Renderer Extern;

        // On peut appuyer

        Intern = InternValidInput.GetComponent<Renderer>();
        Extern = ExternValidInput.GetComponent<Renderer>();

        // On change la couleur

        Intern.material = ValidInputMaterial;
        Extern.material = ValidInputMaterial;

        m_Statut = Statut.SUCCESS;

        AssociatedCharacter.ObstacleSendSuccessTime(GetElement());

        // On continue de retrecir le cercle
        do
        {
            tampon = Mathf.InverseLerp(
                PositionStart,
                AssociatedCharacter.GetCharacterBody().GetValidArea().transform.position.z, 
                transform.position.z);

            ExternValidInput.transform.localScale = Vector3.one * (3 - tampon * tampon * 2);

            yield return new CoroutineTools.WaitForFrames(1);
        }
        while (tampon < 1);

        yield return new WaitForSeconds(ValidInputArea_Delai); // Temps rajouté en plus après le perfect match

        if (m_Statut != Statut.ECHEC)
        {
            Intern.material = InvalidInputMaterial;
            Extern.material = InvalidInputMaterial;

            m_Statut = Statut.MISS;
            AssociatedCharacter.ObstacleSendMissTime();
        }
    }

    #endregion
}