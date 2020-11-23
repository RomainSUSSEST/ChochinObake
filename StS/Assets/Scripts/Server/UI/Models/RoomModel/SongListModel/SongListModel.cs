using UnityEngine;
using ServerVisibleManager;
using System.IO;

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

    /// <summary>
    /// Actualise la liste des sons enregistré sur le serveur et les affiches.
    /// </summary>
    private void RefreshListSong()
    {
        string[] songs = ServerAccountManager.Instance.GetSongList();

        Song currentSong;
        for (int i = 0; i < songs.Length; ++i)
        {
            currentSong = Instantiate(SongPrefab, SongInfoSpawnTransform.position, Quaternion.identity, ContentNode);
            currentSong.SetSongTitle(Path.GetFileName(songs[i]));
        }
    }
}
