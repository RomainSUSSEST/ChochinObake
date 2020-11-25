using UnityEngine;
using System.Collections.Generic;
using SDD.Events;
using ServerManager;

public class SongListModel : MonoBehaviour
{
    // Attributs

    [Header("Panel Song List")]

    [SerializeField] private GameObject SongListPanel;

    [Header("Song List Text Content")]

    [SerializeField] private Song SongPrefab;
    [SerializeField] private Transform SongInfoSpawnTransform;
    [SerializeField] private Transform ContentNode;

    [Header("AddPanel")]

    [SerializeField] private GameObject PanelAddSong;

    private List<Song> SongList;
    private Vector3 CurrentSpawnerPosition;


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
        // On detruit les anciennes données.
        DestroyListSong();

        // On Initialise la position du spawner.
        CurrentSpawnerPosition = SongInfoSpawnTransform.position;

        // On Initialise la liste des sons.
        SongList = new List<Song>();

        // On récupére la liste des sons
        string[] songs = ServerAccountManager.Instance.GetSongList();

        // On les affiches
        Song currentSong;
        for (int i = 0; i < songs.Length; ++i)
        {
            currentSong = Instantiate(SongPrefab, CurrentSpawnerPosition, Quaternion.identity, ContentNode);
            currentSong.SetSongDirectory(songs[i]);
            CurrentSpawnerPosition -= new Vector3(0, currentSong.GetComponent<RectTransform>().rect.height, 0);

            // On ajoute le son à la liste
            SongList.Add(currentSong);
        }
    }

    /// <summary>
    /// Detruit la liste des chansons en nettoyant proprement les ressources alloué.
    /// </summary>
    private void DestroyListSong()
    {
        if (SongList == null)
        {
            return;
        }

        // On détruit la liste des chansons.
        foreach (Song song in SongList)
        {
            Destroy(song.gameObject);
        }
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
