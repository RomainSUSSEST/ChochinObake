using UnityEngine;
using TMPro;
using System.IO;
using ServerManager;

public class SongListModel_Song : MonoBehaviour
{
    // Attributs

    [Header("Song info")]

    [SerializeField] private TextMeshProUGUI SongTitle;

    private string DirectoryPath;


    // Méthode

    public void SetSongDirectory(string directory)
    {
        DirectoryPath = directory;
        SongTitle.text = Path.GetFileName(directory);
    }


    // Onclick Button Event

    public void DeleteButtonHasBeenClicked()
    {
        ServerAccountManager.Instance.RemoveSongWithDirectoryPath(DirectoryPath);
    }
}
