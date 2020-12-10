using ClientManager;
using CommonVisibleManager;
using SDD.Events;
using System.Collections.Generic;
using UnityEngine;

public class MusicSelectionClientModel : MonoBehaviour
{
    // Attributs

    [Header("SongListPanel")]
    [SerializeField] private GameObject PanelMusicList;

    [Header("Song List Text Content")]

    [SerializeField] private MusicSelectionClient_Song SongPrefab; // La prefab de l'objet son à spawn
    [SerializeField] private Transform SongSpawnTransform;
    [SerializeField] private Transform ContentNode;

    private string[] MusicList; // La liste des musiques renvoyé par le serveur
    private List<MusicSelectionClient_Song> SongList; // La liste des objets UI affichant les musiques
    private Vector3 CurrentSpawnerPosition;


    #region Life cycle

    private void OnEnable()
    {
        SubscribeEvents(); // On s'abonne au différent event

        // On active le panel de musique
        PanelMusicList.SetActive(true);

        // On réinitialise le panel de musique
        DestroyListSong();
        AskServerForMusicList(); // On demande la liste des chansons au serveur
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    #endregion

    #region Event call back

    private void AnswerForMusicListRequest(AnswerForMusicListRequestEvent e)
    {
        MusicList = e.MusicList; // On enregistre la liste des musiques
        PrintPanelMusicList(); // On refresh la liste des musiques en conséquence.
    }

    private void MusicVoteAccepted(MusicVoteAcceptedEvent e)
    {
        // On désactive le PanelMusicList
        PanelMusicList.SetActive(false);
    }

    #endregion


    // Outils

    /// <summary>
    /// Permet de demander au serveur la liste des chansons
    /// </summary>
    private void AskServerForMusicList()
    {
        if (ClientNetworkManager.Instance.GetPlayerID() == null)
        {
            return;
        }

        MessagingManager.Instance.RaiseNetworkedEventOnServer(new AskForMusicListEvent(ClientNetworkManager.Instance.GetPlayerID().Value));
    }

    /// <summary>
    /// Print le panel de liste des musiques
    /// </summary>
    private void PrintPanelMusicList()
    {
        // On parcourt les chansons
        MusicSelectionClient_Song currentSong;
        for (int i = 0; i < MusicList.Length; ++i)
        {
            currentSong = Instantiate(SongPrefab, CurrentSpawnerPosition, Quaternion.identity, ContentNode);
            currentSong.SetTitle(MusicList[i]);

            CurrentSpawnerPosition = new Vector3(CurrentSpawnerPosition.x,
                CurrentSpawnerPosition.y - currentSong.GetComponent<RectTransform>().rect.height,
                CurrentSpawnerPosition.z);

            // On ajoute le son à la liste.
            SongList.Add(currentSong);
        }
    }

    /// <summary>
    /// Détruit la liste des chansons en nettoyant proprement les ressources alloué.
    /// Et initialise une empty liste
    /// Recadre également la position du spawner à sa position par défaut.
    /// </summary>
    private void DestroyListSong()
    {
        if (SongList != null)
        {
            // On détruit la liste des chansons
            foreach (MusicSelectionClient_Song song in SongList)
            {
                Destroy(song.gameObject);
            }
        }

        // On initialise la liste des sons
        SongList = new List<MusicSelectionClient_Song>();

        // On initialise la position du spawner.
        CurrentSpawnerPosition = SongSpawnTransform.position;
    }


    #region Event Subs

    private void SubscribeEvents()
    {
        // Networked Event

        EventManager.Instance.AddListener<AnswerForMusicListRequestEvent>(AnswerForMusicListRequest);
        EventManager.Instance.AddListener<MusicVoteAcceptedEvent>(MusicVoteAccepted);
    }

    private void UnsubscribeEvents()
    {
        // Networked Event

        EventManager.Instance.RemoveListener<AnswerForMusicListRequestEvent>(AnswerForMusicListRequest);
        EventManager.Instance.RemoveListener<MusicVoteAcceptedEvent>(MusicVoteAccepted);
    }

    #endregion
}
