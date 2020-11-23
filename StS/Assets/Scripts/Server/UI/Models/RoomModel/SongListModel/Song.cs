using UnityEngine;
using TMPro;

public class Song : MonoBehaviour
{
    // Attributs

    [Header("Song info")]

    [SerializeField] private TextMeshProUGUI SongTitle;


    // Méthode

    public void SetSongTitle(string title)
    {
        SongTitle.text = title;
    }


    // Onclick Button Event

    public void DeleteButtonHasBeenClicked()
    {

    }
}
