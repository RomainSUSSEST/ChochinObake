using UnityEngine;
using TMPro;
using ServerVisibleManager;
using UnityEngine.UI;
using SDD.Events;

public class AddSongModel : MonoBehaviour
{
    // Constante

    private static readonly Color DEFAULT_COLOR = Color.white;
    private static readonly Color ERROR_COLOR = Color.red;


    // Attributs

    [Header("Panel Add Song")]

    [SerializeField] private GameObject PanelAddSong; // Référence du panel complet (View + Model)

    [Header("Input Field")]

    [SerializeField] private TMP_InputField URLInputField;
    [SerializeField] private Animator URLErrorAnimator;

    [Header("Button")]

    [SerializeField] private Button AddButton;

    [Header("Progress Bar")]

    [SerializeField] private GameObject ProgressBar_Content; // Ensemble du contenu de la progressBar.
    [SerializeField] private Slider ProgressBar; // Progress Bar
    [SerializeField] private TextMeshProUGUI ProgressBar_State; // Texte indiquant les étapes au sein de la progress bar
    [SerializeField] private Button ProgressBar_CloseButton;


    // Life Cycle

    private void Start()
    {
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void OnEnable()
    {
        // On initialise les éléments de la page.

        ProgressBar_Content.SetActive(false);
        AddButton.interactable = true;
    }


    // OnClick Button function

    public void AddButtonHasBeenClicked()
    {
        if (URLInputField.text == "")
        {
            URLErrorAnimator.SetBool("Start", true);
            return;
        }

        AddSong(URLInputField.text);
    }

    public void CancelButtonHasBeenClicked()
    {
        PanelAddSong.SetActive(false);
    }

    public void ProgressBar_CloseButtonHasBeenClicked()
    {
        ProgressBar_Content.SetActive(false);
        AddButton.interactable = true;
        URLInputField.text = "";
    }


    // Outils

    private void SubscribeEvents()
    {
        // ServerAccountManager Event

        EventManager.Instance.AddListener<ProgressBarPrepareSongHaveChangedEvent>(UpdateProgressBarPrepareSong);
        EventManager.Instance.AddListener<ProgressBarPrepareSongErrorEvent>(UpdateProgressBarPrepareSongError);
        EventManager.Instance.AddListener<PrepareSongEndEvent>(PrepareSongEnd);
    }

    private void UnsubscribeEvents()
    {
        // ServerAccountManager Event

        EventManager.Instance.RemoveListener<ProgressBarPrepareSongHaveChangedEvent>(UpdateProgressBarPrepareSong);
        EventManager.Instance.RemoveListener<ProgressBarPrepareSongErrorEvent>(UpdateProgressBarPrepareSongError);
        EventManager.Instance.RemoveListener<PrepareSongEndEvent>(PrepareSongEnd);
    }

    private void AddSong(string url)
    {
        // On désactive le AddButton pour éviter les multiples requetes
        AddButton.interactable = false;

        // On setup la ProgressBar
        InitializeProgressBar();

        ServerAccountManager.Instance.AddYoutubeSongAsync(url);
    }

    private void InitializeProgressBar()
    {
        ProgressBar_Content.SetActive(true); // On active le panel de progressBar
        ProgressBar_State.color = DEFAULT_COLOR; // On set la color à DEFAULT_COLOR
        ProgressBar_CloseButton.gameObject.SetActive(false); // On masque le boutton pour fermer la progressBar.
    }


    #region Event Call Back

    private void UpdateProgressBarPrepareSong(ProgressBarPrepareSongHaveChangedEvent e)
    {
        ProgressBar.value = (float) e.Value / 100f; // e.value 0 <= e.value <= 1
        ProgressBar_State.text = e.State;
    }

    private void UpdateProgressBarPrepareSongError(ProgressBarPrepareSongErrorEvent e)
    {
        ProgressBar_State.text = e.msg; // On affiche le message d'erreur
        ProgressBar_State.color = ERROR_COLOR; // On Change la couleur en ERROR_COLOR
    }

    /// <summary>
    /// L'ajout de la chanson est terminé, de manière normal ou non.
    /// </summary>
    /// <param name="e"></param>
    private void PrepareSongEnd(PrepareSongEndEvent e)
    {
        ProgressBar_CloseButton.gameObject.SetActive(true); // On affiche le bouton pour fermer la progressBar.
    }

    #endregion
}
