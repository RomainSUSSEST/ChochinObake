using UnityEngine;
using TMPro;

public class SongListModel : MonoBehaviour
{
    // Attributs

    [Header("Panel Song List")]

    [SerializeField] private GameObject SongListPanel;

    [Header("Song List Text Content")]

    [SerializeField] private TextMeshProUGUI SongListTextContent;

    [Header("AddPanel")]

    [SerializeField] private GameObject PanelAddSong;


    // Life Cycle

    private void OnEnable()
    {
        // Initialisation

        PanelAddSong.SetActive(false); // On s'assure que le PanelAddSong est désactivé.
        RefreshListSong(); // On charge la liste des sons enregistré.
    }


    // Onclick button function

    public void CloseButtonHasBeenClicked()
    {
        SongListPanel.SetActive(false); // On désactive le panel courant
    }

    public void AddButtonHasBeenClicked()
    {
        PanelAddSong.SetActive(true); // On active le panel pour ajouter des sons.
    }


    // Outils

    private void RefreshListSong()
    {

    }
}
