using TMPro;
using UnityEngine;

public class MusicSelectionServer_Song : MonoBehaviour
{
    // Attributs

    [Header("Song info")]

    [SerializeField] private TextMeshProUGUI SongInfo;
    [SerializeField] private TextMeshProUGUI VoteInfo;

    private int NbrVote;
    private string Title;


    // Requete

    public string GetTitle()
    {
        return Title;
    }

    public int getNbrVote()
    {
        return NbrVote;
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


    // Outils

    private void RefreshInfo()
    {
        SongInfo.text = Title;
        VoteInfo.text =  NbrVote.ToString();
    }
}
