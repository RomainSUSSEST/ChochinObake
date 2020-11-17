using UnityEngine;
using TMPro;
using ServerVisibleManager;
using UnityEngine.UI;
using SDD.Events;

public class AddSongModel : MonoBehaviour
{
    // Attributs

    [Header("Panel Add Song")]

    [SerializeField] private GameObject PanelAddSong;

    [Header("Input Field")]

    [SerializeField] private TMP_InputField URLInputField;
    [SerializeField] private Animator URLErrorAnimator;

    [Header("Button")]

    [SerializeField] private Button AddButton;

    [Header("Progress Bar")]

    [SerializeField] private GameObject ProgressBar_Content; // Ensemble du contenu de la progressBar.
    [SerializeField] private Slider ProgressBar;
    [SerializeField] private TextMeshProUGUI ProgressBar_State;


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


    // Outils

    private void SubscribeEvents()
    {
        EventManager.Instance.AddListener<ProgressBarPrepareSongHaveChanged>(UpdateProgressBarPrepareSong);
    }

    private void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<ProgressBarPrepareSongHaveChanged>(UpdateProgressBarPrepareSong);
    }

    private void AddSong(string url)
    {
        // On désactive le AddButton pour éviter les multiples requetes
        AddButton.interactable = false;

        // On Affiche la progressBar
        ProgressBar_Content.SetActive(true);

        ServerAccountManager.Instance.AddYoutubeSong(url);
    }


    #region Event Call Back

    private void UpdateProgressBarPrepareSong(ProgressBarPrepareSongHaveChanged e)
    {
        ProgressBar.value = (float) e.Value / 100f;
        ProgressBar_State.text = e.State;
    }

    #endregion
}
