using TMPro;
using UnityEngine;

public class MusicSelectionServer_Song : MonoBehaviour
{
    // Attributs

    [Header("Song info")]

    [SerializeField] private TextMeshProUGUI SongInfo;

    private int NbrVote;
    private string Title;
    private string DirectoryPath;


    // Requete

    public string GetTitle()
    {
        return Title;
    }

    public int getNbrVote()
    {
        return NbrVote;
    }

    public string GetDirectoryPath()
    {
        return DirectoryPath;
    }


    // Méthode

    public void IncreaseNumberVote()
    {
        NbrVote += 1;
        RefreshInfo();
    }

    public void DecreaseNumberVote()
    {
        NbrVote -= 1;
        RefreshInfo();
    }

    public void SetTitle(string title)
    {
        Title = title;
        RefreshInfo();
    }

    public void SetDirectoryPath(string path)
    {
        DirectoryPath = path;
    }


    // Outils

    private void RefreshInfo()
    {
        SongInfo.text = Title + " |Vote : " + NbrVote;
    }
}
