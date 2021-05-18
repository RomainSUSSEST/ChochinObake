using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ServerManager;
using SDD.Events;
using CommonVisibleManager;
using System.IO;

public class MusicSelectionServerModel : MonoBehaviour
{
    // Constantes

    private readonly int MARGIN_HEIGHT = 5; // en px

    // Attributs

    [Header("UI Elements")]

    [SerializeField] private TextMeshProUGUI Timer;

    [SerializeField] private TextMeshProUGUI PlayerList;

    [SerializeField] private RectTransform ViewPanel;

    [SerializeField] private MusicSelectionServer_Song SongPrefab;
    [SerializeField] private Transform SongContentNode;

    [Header("Panel Config")]

    [SerializeField] private float TimerStartValue; // Temps initial du timer en seconde


    private float TimerCurrentValue; // Valeur courante du timer

    private IReadOnlyDictionary<ulong, Player> Players;

    private string[] SongListPath; // Liste des chansons récupéré d'accountManager
    private string[] SongListName;

    // Panel music
    private List<MusicSelectionServer_Song> SongListGameObject; // Liste des chansons affiché en UI


    // Life cycle

    private void OnEnable()
    {
        SubscribeEvents();

        // On récupéré la liste des joueurs
        Players = ServerGameManager.Instance.GetPlayers();

        if (Players != null)
        {
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

        // On regarde si tout les joueurs on voté
        if (AllPlayersVoted())
        {
            ChooseAndSendWinnerSong();
        }

        // On indique au joueur que sont vote à bien été recu
        MessagingManager.Instance.RaiseNetworkedEventOnClient(new MusicVoteAcceptedEvent(e.PlayerID.Value));
    }

    #endregion

    #endregion


    // Outils

    #region ChooseAndSendWinnerSong

    private void ChooseAndSendWinnerSong()
    {
        int cmptVote = GetMaxVote();

        if (cmptVote > 0)
        {
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

            // On choisi une chanson parmi les gagnantes et on envoie son path du directory
            EventManager.Instance.Raise(new MusicSelectionTimerEndEvent()
            {
                PathDirectoryMusicSelected = PathMusicGagnante[Random.Range(0, PathMusicGagnante.Count)]
            });
        }
        else // Si personne n'a voté on prend une chanson aléatoire
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

    #endregion

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
    }

    private void IncreaseVoteOnSong(string title)
    {
        bool instantiate = true;

        // On regarde si le son n'a pas déjà été voté
        foreach (MusicSelectionServer_Song song in SongListGameObject)
        {
            if (song.GetTitle().Equals(title))
            {
                song.IncreaseNumberVote();
                instantiate = false;
            }
        }

        if (instantiate)
        {
            // Sinon on ajoute le nouveau son
            MusicSelectionServer_Song tampon = Instantiate(SongPrefab, SongContentNode);
            tampon.SetTitle(title); // On set son titre
            tampon.IncreaseNumberVote(); // on set son nombre de vote

            SongListGameObject.Add(tampon); // On l'ajoute à la liste
        }

        #region Sort

        SongListGameObject.Sort((MusicSelectionServer_Song x, MusicSelectionServer_Song y) =>
            y.getNbrVote() - x.getNbrVote());

        #endregion

        #region Placement

        float areaHeight = SongPrefab.GetComponent<RectTransform>().rect.height;
        float currentMarginHeight = MARGIN_HEIGHT;

        // On tiens compte du rescale de la vue
        areaHeight *= ViewPanel.lossyScale.y;
        currentMarginHeight *= ViewPanel.lossyScale.y;

        // On estime la hauteur à allouer
        float height = (SongListGameObject.Count + 1) * currentMarginHeight
            + SongListGameObject.Count * areaHeight;

        height /= ViewPanel.lossyScale.y;

        // On redimenssionne le content
        RectTransform contentRectTransform = SongContentNode.GetComponent<RectTransform>();
        contentRectTransform.sizeDelta = new Vector2
            (
               contentRectTransform.rect.width,
               height
            );

        // Position de départ

        Vector3 currentPositionSong = new Vector3
            (
                contentRectTransform.position.x,
                (contentRectTransform.position.y
                    + height * ViewPanel.lossyScale.y / 2 - currentMarginHeight - areaHeight / 2),
                contentRectTransform.position.z
            );

        // On les affiches

        foreach (MusicSelectionServer_Song msss in SongListGameObject)
        {
            msss.transform.position = currentPositionSong;
            currentPositionSong -= new Vector3(0, areaHeight + currentMarginHeight, 0);
        }

        // On affiche le premier en haut
        contentRectTransform.localPosition = new Vector3
            (
                contentRectTransform.localPosition.x,
                -height / 2 - contentRectTransform.parent.GetComponent<RectTransform>().rect.height,
                contentRectTransform.localPosition.z
            );

        #endregion
    }

    #endregion

    #region Timer

    private void TimerEnd()
    {
        ChooseAndSendWinnerSong();
    }

    private void RefreshTimerUI()
    {
        Timer.text = Mathf.Ceil(TimerCurrentValue).ToString() + 's';
    }

    #endregion

    #region Player

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

    private bool AllPlayersVoted()
    {
        IEnumerable<Player> value = Players.Values;
        foreach (Player p in value)
        {
            if (p.PlayerState != PlayerState.Voted)
            {
                return false;
            }
        }

        return true;
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
