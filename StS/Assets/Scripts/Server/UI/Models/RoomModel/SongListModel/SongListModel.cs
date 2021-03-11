using UnityEngine;
using System.Collections.Generic;
using SDD.Events;
using ServerManager;

public class SongListModel : MonoBehaviour
{
    // Constante

    private static readonly float MARGIN_HEIGHT = 5;


    // Attributs

    [SerializeField] private RectTransform ViewPanel;

    [Header("Panel Song List")]

    [SerializeField] private GameObject SongListPanel;

    [Header("Song List Text Content")]

    [SerializeField] private SongListModel_Song SongPrefab;
    [SerializeField] private Transform ContentNode;

    [Header("AddPanel")]

    [SerializeField] private GameObject PanelAddSong;

    private List<SongListModel_Song> SongList;


    // Life Cycle

    private void OnEnable()
    {
        // Initialisation

        SubscribeEvents();
        PanelAddSong.SetActive(false); // On s'assure que le PanelAddSong est désactivé.
        RefreshListSong(); // On charge la liste des sons enregistré.
    }

    public void OnDisable()
    {
        UnsubscribeEvents();
    }


    // Onclick button function

    public void CloseButtonHasBeenClicked()
    {
        SongListPanel.SetActive(false); // On désactive le panel courant
        EventManager.Instance.Raise(new SongListModelHasBeenClosedEvent());
    }

    public void AddButtonHasBeenClicked()
    {
        PanelAddSong.SetActive(true); // On active le panel pour ajouter des sons.
    }


    // Outils

    /// <summary>
    /// Reset puis actualise la liste des sons enregistré sur le serveur et les affiches.
    /// </summary>
    private void RefreshListSong()
    {
        // On detruit les anciennes données
        DestroyListSong();

        // On récupére la liste des sons
        string[] songs = ServerAccountManager.Instance.GetSongList();

        // Hauteur d'un bouton de sons & Margin
        float buttonHeight = SongPrefab.GetComponent<RectTransform>().rect.height;
        float currentMarginHeight = MARGIN_HEIGHT;

        // On tient compte du rescale de la vue
        buttonHeight *= ViewPanel.lossyScale.y;
        currentMarginHeight *= ViewPanel.lossyScale.y;

        // On estime la hauteur à allouer
        float height = (songs.Length + 1) * currentMarginHeight
            + songs.Length * buttonHeight;
        height /= ViewPanel.lossyScale.y;

        // On redimenssione le content
        RectTransform contentRectTransform = ContentNode.GetComponent<RectTransform>();
        contentRectTransform.sizeDelta = new Vector2
            (
                contentRectTransform.rect.width,
                height
            );

        // Position de départ

        Vector3 currentPositionButtonSpawn = new Vector3
            (
                contentRectTransform.position.x,
                (contentRectTransform.position.y
                    + height * ViewPanel.lossyScale.y / 2 - currentMarginHeight - buttonHeight / 2),
                contentRectTransform.position.z
            );

        // On les affiches
        SongListModel_Song currentSong;
        for (int i = 0; i < songs.Length; ++i)
        {
            currentSong = Instantiate(SongPrefab, currentPositionButtonSpawn, Quaternion.identity, ContentNode);
            currentSong.SetSongDirectory(songs[i]);
            currentPositionButtonSpawn -= new Vector3(0, buttonHeight + currentMarginHeight, 0);

            // On ajoute le son à la liste
            SongList.Add(currentSong);
        }

        // On décale le content pour afficher le premier en haut
        contentRectTransform.localPosition = new Vector3
            (
                contentRectTransform.localPosition.x,
                -height / 2 - contentRectTransform.parent.GetComponent<RectTransform>().rect.height,
                contentRectTransform.localPosition.z
            );
    }

    /// <summary>
    /// Detruit la liste des chansons en nettoyant proprement les ressources alloué.
    /// Puis recréer une liste empty
    /// </summary>
    private void DestroyListSong()
    {
        if (SongList != null)
        {
            // On détruit la liste des chansons.
            foreach (SongListModel_Song song in SongList)
            {
                Destroy(song.gameObject);
            }
        }

        // On Initialise la liste des sons.
        SongList = new List<SongListModel_Song>();
    }

    private void SubscribeEvents()
    {
        // ServerAccountManager Event

        EventManager.Instance.AddListener<PrepareSongEndEvent>(PrepareSongEnd);
        EventManager.Instance.AddListener<DataSongDeletedEvent>(DataSongDeleted);
    }

    private void UnsubscribeEvents()
    {
        // ServerAccountManager Event

        EventManager.Instance.RemoveListener<PrepareSongEndEvent>(PrepareSongEnd);
        EventManager.Instance.RemoveListener<DataSongDeletedEvent>(DataSongDeleted);
    }


    #region Event Call Back

    private void PrepareSongEnd(PrepareSongEndEvent e)
    {
        RefreshListSong();
    }

    private void DataSongDeleted(DataSongDeletedEvent e)
    {
        RefreshListSong();
    }

    #endregion
}
