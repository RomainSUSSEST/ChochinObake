using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ServerManager;
using SDD.Events;
using CommonVisibleManager;
using System.IO;

public class MusicSelectionServerModel : MonoBehaviour
{
    // Attributs

    [Header("UI Elements")]

    [SerializeField] private TextMeshProUGUI Timer;

    [SerializeField] private TextMeshProUGUI PlayerList;

    [SerializeField] private MusicSelectionServer_Song SongPrefab;
    [SerializeField] private Transform SongSpawnTransform;
    [SerializeField] private Transform SongContentNode;

    [Header("Panel Config")]

    [SerializeField] private float TimerStartValue; // Temps initial du timer en seconde


    private float TimerCurrentValue; // Valeur courante du timer

    private IReadOnlyDictionary<ulong, Player> Players;

    private string[] SongListPath; // Liste des chansons récupéré d'accountManager
    private string[] SongListName;

    // Panel music
    private List<MusicSelectionServer_Song> SongListGameObject; // Liste des chansons affiché en UI
    private Vector3 CurrentSongSpawnerPosition;


    // Life cycle

    private void OnEnable()
    {
        SubscribeEvents();

        if (ServerGameManager.Instance.GetPlayers() != null)
        {
            // On récupéré la liste des joueurs
            Players = ServerGameManager.Instance.GetPlayers();

            // On intialise l'état des joueurs
            InitializePlayerDefaultState();

            // On récupére la liste des sons
            SongListPath = ServerAccountManager.Instance.GetSongList();
            SongListName = new string[SongListPath.Length];
            for (int i = 0; i < SongListPath.Length; ++i)
            {
                SongListName[i] = Path.GetFileName(SongListPath[i]);
            }

            // On initialise le timer
            TimerCurrentValue = TimerStartValue;

            // Initialisation de la page

            RefreshTimerUI(); // Affichage du timer
            RefreshPlayerListState(); // Affichage de la liste des joueurs et leurs états
            DestroyListSongGameObject(); // On reset le panel des chansons.
        }
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void Update()
    {
        if (TimerCurrentValue > 0)
        {
            TimerCurrentValue -= Time.deltaTime;
            RefreshTimerUI();

            if (TimerCurrentValue <= 0)
            {
                TimerEnd();
            }
        }
    }


    #region Event Call Back

    #region Networked Event

    private void ClientAskForMusicList(AskForMusicListEvent e)
    {
        if (e.PlayerID == null)
        {
            throw new System.Exception();
        }

        MessagingManager.Instance.RaiseNetworkedEventOnClient(new AnswerForMusicListRequestEvent(e.PlayerID.Value, SongListName));
    }

    private void VoteButtonHasBeenClicked(VoteButtonHasBeenClickedEvent e)
    {
        // On regarde si le joueur n'a pas déjà voté
        if (Players[e.PlayerID.Value].PlayerState == PlayerState.Voted)
        {
            return;
        }

        IncreaseVoteOnSong(e.AudioTitle); // On incrémente le nombre de vote sur la chanson

        Players[e.PlayerID.Value].PlayerState = PlayerState.Voted; // On change le state du joueur
        RefreshPlayerListState(); // On refresh les états

        // On indique au joueur que sont vote à bien été recu
        MessagingManager.Instance.RaiseNetworkedEventOnClient(new MusicVoteAcceptedEvent(e.PlayerID.Value));
    }

    #endregion

    #endregion


    // Outils

    #region Panel Music

    /// <summary>
    /// Detruit la liste des gameobject représentant les chanson voté proprement en libérant les
    /// ressources alloué. Et recréer une liste empty.
    /// Recadre également la position spawner à sa position par défaut
    /// </summary>
    private void DestroyListSongGameObject()
    {
        if (SongListGameObject != null)
        {
            // On détruit la liste des chansons
            foreach (MusicSelectionServer_Song song in SongListGameObject)
            {
                Destroy(song.gameObject);
            }
        }

        // On initialise la liste des sons
        SongListGameObject = new List<MusicSelectionServer_Song>();

        // On reset l'emplacement du spawner
        CurrentSongSpawnerPosition = SongSpawnTransform.position;
    }

    private void IncreaseVoteOnSong(string title)
    {
        // On regarde si le son n'a pas déjà été voté
        foreach (MusicSelectionServer_Song song in SongListGameObject)
        {
            if (song.GetTitle().Equals(title))
            {
                song.IncreaseNumberVote();
                return;
            }
        }

        // Sinon on ajoute le nouveau son
        MusicSelectionServer_Song tampon = Instantiate(SongPrefab, CurrentSongSpawnerPosition, Quaternion.identity, SongContentNode);
        tampon.SetTitle(title); // On set son titre
        tampon.IncreaseNumberVote(); // on set son nombre de vote

        SongListGameObject.Add(tampon); // On l'ajoute à la liste
    }

    #endregion

    #region Timer

    private void TimerEnd()
    {
        int cmptVote = GetMaxVote();

        List<string> PathMusicGagnante = new List<string>();
        
        // On récupére les musiques gagnante
        foreach (MusicSelectionServer_Song song in SongListGameObject)
        {
            if (song.getNbrVote() == cmptVote) // Si c'est une musique gagnante
            {
                // On récupére le path de la chanson
                foreach (string s in SongListPath)
                {
                    if (Path.GetFileName(s).Equals(song.GetTitle()))
                    { // C'est une gagnante, on l'ajoute
                        PathMusicGagnante.Add(s);
                        break;
                    }
                }
            }
        }

        // Si des personnes on voté
        if (PathMusicGagnante.Count > 0)
        {
            // On choisi une chanson parmi les gagnantes et on envoie son path du directory
            EventManager.Instance.Raise(new MusicSelectionTimerEndEvent()
            {
                PathDirectoryMusicSelected = PathMusicGagnante[Random.Range(0, PathMusicGagnante.Count)]
            });
        } else // Si personne n'a voté on prend une chanson aléatoire
        {
            EventManager.Instance.Raise(new MusicSelectionTimerEndEvent()
            {
                PathDirectoryMusicSelected = SongListPath[Random.Range(0, SongListPath.Length)]
            });
        }
    }

    /// <summary>
    /// Renvoie le nombre de vote le plus élevé parmi les chansons
    /// </summary>
    /// <returns> Renvoie le nombre de vote le plus élevé parmi les chansons </returns>
    private int GetMaxVote()
    {
        int vote = 0;
        foreach (MusicSelectionServer_Song song in SongListGameObject)
        {
            if (song.getNbrVote() > vote)
            {
                vote = song.getNbrVote();
            }
        }

        return vote;
    }

    private void RefreshTimerUI()
    {
        Timer.text = Mathf.Ceil(TimerCurrentValue).ToString() + 's';
    }

    #endregion

    #region Player Panel

    private void InitializePlayerDefaultState()
    {
        // On récupére la liste des joueurs
        IEnumerable<Player> value = Players.Values;

        foreach (Player p in value)
        {
            p.PlayerState = PlayerState.WaitingForTheVote;
        }
    }

    private void RefreshPlayerListState()
    {
        string text = "";

        // On récupére la liste des joueurs
        IEnumerable<Player> value = Players.Values;

        foreach (Player p in value)
        {
            // On écrit l'état actuel de chaque joueur
            text += p.Pseudo + " : " + GetTextFrom(p.PlayerState) + "\n";
        }

        PlayerList.text = text;
    }

    private string GetTextFrom(PlayerState p)
    {
        switch (p)
        {
            case PlayerState.WaitingForTheVote:
                return "Waiting for the vote";
            case PlayerState.Voted:
                return "Voted";
            default:
                throw new System.Exception();
        }
    }

    #endregion

    #region Event Subs

    private void SubscribeEvents()
    {
        // Networked Event

        EventManager.Instance.AddListener<AskForMusicListEvent>(ClientAskForMusicList);
        EventManager.Instance.AddListener<VoteButtonHasBeenClickedEvent>(VoteButtonHasBeenClicked);
    }

    private void UnsubscribeEvents()
    {
        // Networked Event

        EventManager.Instance.RemoveListener<AskForMusicListEvent>(ClientAskForMusicList);
        EventManager.Instance.RemoveListener<VoteButtonHasBeenClickedEvent>(VoteButtonHasBeenClicked);
    }

    #endregion
}
