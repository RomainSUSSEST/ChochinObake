using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ServerManager;
using SDD.Events;
using CommonVisibleManager;

public class MusicSelectionServerModel : MonoBehaviour
{
    // Attributs

    [Header("UI Elements")]

    [SerializeField] private TextMeshProUGUI Timer;
    [SerializeField] private TextMeshProUGUI PlayerList;

    [Header("Panel Config")]

    [SerializeField] private float TimerStartValue; // Temps initial du timer en seconde


    private float TimerCurrentValue; // Valeur courante du timer
    private IReadOnlyDictionary<ulong, Player> Players;
    private string[] SongList;


    // Life cycle

    private void OnEnable()
    {
        SubscribeEvents();

        if (ServerGameManager.Instance.GetPlayers() != null)
        {
            // On récupéré la liste des joueurs
            Players = ServerGameManager.Instance.GetPlayers();

            // On récupére la liste des sons
            SongList = ServerAccountManager.Instance.GetSongList();

            // On initialise le timer
            TimerCurrentValue = TimerStartValue;

            // Initialisation de la page

            RefreshTimerUI();
            RefreshPlayerListState();
        }
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void Update()
    {
        TimerCurrentValue -= Time.deltaTime;
        RefreshTimerUI();

        if (TimerCurrentValue <= 0)
        {
            TimerEnd();
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

        MessagingManager.Instance.RaiseNetworkedEventOnClient(new AnswerForMusicListRequestEvent(e.PlayerID.Value, SongList));
    }

    #endregion

    #endregion


    // Outils

    private void RefreshTimerUI()
    {
        Timer.text = Mathf.Ceil(TimerCurrentValue).ToString() + 's';
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

    private void TimerEnd()
    {

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

    #region Event Subs

    private void SubscribeEvents()
    {
        // Networked Event

        EventManager.Instance.AddListener<AskForMusicListEvent>(ClientAskForMusicList);
    }

    private void UnsubscribeEvents()
    {
        // Networked Event

        EventManager.Instance.RemoveListener<AskForMusicListEvent>(ClientAskForMusicList);
    }

    #endregion
}
