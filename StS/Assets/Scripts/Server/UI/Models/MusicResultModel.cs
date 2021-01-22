using CommonVisibleManager;
using SDD.Events;
using ServerManager;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Affiche la musique gagnante et charge la carte associée.
/// 
/// Permet également aux joueurs de choisir une difficulté,
/// le résultat sera la moyenne des difficultés choisi ou medium par défaut.
/// </summary>
public class MusicResultModel : MonoBehaviour
{
    // Constante

    [SerializeField] private Color PROGRESS_BAR_DEFAULT_COLOR = Color.white;
    [SerializeField] private Color PROGRESS_BAR_ERROR_COLOR = Color.red;

    private static readonly float EASY_DIFFICULTY = 0; // 0%
    private static readonly float MEDIUM_DIFFICULTY = 0.5f; // 50%
    private static readonly float HARD_DIFFICULTY = 1; // 100%


    // Attributs

    [Header("UI Elements")]

    [SerializeField] private TextMeshProUGUI Timer;
    [SerializeField] private TextMeshProUGUI SongTitle;


    [Header("Panel Config")]

    [SerializeField] private float TimerStartValue; // Temps initial du timer en seconde

    [Header("Progress Bar")]

    [SerializeField] private Slider ProgressBar; // Progress Bar
    [SerializeField] private TextMeshProUGUI ProgressBar_State; // Texte indiquant les étapes au sein de la progress bar

    [Header("Difficulty")]
    [SerializeField] private Slider DifficultyBar;
    [SerializeField] private Image DifficultyBarImage;

    [SerializeField] private Color EasyColor;
    [SerializeField] private Color MediumColor;
    [SerializeField] private Color HardColor;


    private float TimerCurrentValue; // Valeur courante du timer
    private bool TimerIsEnd;

    private IReadOnlyDictionary<ulong, Player> Players;
    
    private float CurrentDifficulty;
    private float NbrVoteDifficulty;

    private bool MapIsLoaded;
    private AudioClip CurrentAudioClip;
    private List<SpectralFluxInfo> CurrentMapData;


    #region Life Cycle
    
    private void OnEnable()
    {
        if (ServerGameManager.Instance.GetCurrentMusicPath() != null)
        {
            SubscribeEvents();

            Players = ServerGameManager.Instance.GetPlayers();

            InitializePlayersDefaultStates();

            // On initialise le timer
            TimerCurrentValue = TimerStartValue;
            TimerIsEnd = false;


            // On intialise les données de chargement de carte
            ProgressBar_State.color = PROGRESS_BAR_DEFAULT_COLOR;
            MapIsLoaded = false;


            // On initialise la musique gagnante
            SongTitle.text = Path.GetFileName(ServerGameManager.Instance.GetCurrentMusicPath());


            // On initialise la difficulté
            DifficultyBar.value = 0.5f;
            DifficultyBarImage.color = MediumColor;

            CurrentDifficulty = 0;
            NbrVoteDifficulty = 0;


            // Initialisation de la page

            RefreshTimerUI(); // Affichage du timer
            LoadMapAsync(); // On charge la carte
        }
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

        if (GameReady())
        {
            EventManager.Instance.Raise(new MusicResultGameReadyEvent()
            {
                audio = CurrentAudioClip,
                map = CurrentMapData,
                difficulty = DifficultyBar.value
            });
        }
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    #endregion


    // Outils

    /// <summary>
    ///  true <==> MapIsLoaded && TimerIsEnd
    /// </summary>
    /// <returns> si tous est pret </returns>
    private bool GameReady()
    {
        return MapIsLoaded && TimerIsEnd;
    }

    #region Timer

    private void TimerEnd()
    {
        TimerIsEnd = true;
    }

    private void RefreshTimerUI()
    {
        Timer.text = Mathf.Ceil(TimerCurrentValue).ToString() + 's';
    }

    #endregion

    #region LoadMap

    private async void LoadMapAsync()
    {
        string path = ServerGameManager.Instance.GetCurrentMusicPath();
        try
        {
            CurrentAudioClip = await ServerAccountManager.Instance.GetAudioClipOfSongAsync(path);
            CurrentMapData = await ServerAccountManager.Instance.GetMapDataAsync(path);
        } catch (Exception e)
        {
            UpdateLoadingMapAnErrorOccured(e.Message);
            return;
        }

        UpdateLoadingMap(1, "Map loaded !");
        MapIsLoaded = true;
    }

    private void UpdateLoadingMap(float value, string state)
    {
        ProgressBar.value = value;
        ProgressBar_State.text = state;
    }

    private void UpdateLoadingMapAnErrorOccured(string msg)
    {
        ProgressBar_State.text = msg; // On affiche le message d'erreur
        ProgressBar_State.color = PROGRESS_BAR_ERROR_COLOR;
    }

    #endregion

    #region Event Subs

    private void SubscribeEvents()
    {
        EventManager.Instance.AddListener<UpdateLoadingProgressionAudioClipEvent>(UpdateLoadingProgressionAudioClip);
        EventManager.Instance.AddListener<UpdateLoadingMapDataEvent>(UpdateLoadingMapData);

        // NetworkedEvent

        EventManager.Instance.AddListener<EasyDifficultySelectedEvent>(EasyDifficultySelected);
        EventManager.Instance.AddListener<MediumDifficultySelectedEvent>(MediumDifficultySelected);
        EventManager.Instance.AddListener<HardDifficultySelectedEvent>(HardDifficultySelected);
    }

    private void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<UpdateLoadingProgressionAudioClipEvent>(UpdateLoadingProgressionAudioClip);
        EventManager.Instance.RemoveListener<UpdateLoadingMapDataEvent>(UpdateLoadingMapData);

        // NetworkedEvent

        EventManager.Instance.RemoveListener<EasyDifficultySelectedEvent>(EasyDifficultySelected);
        EventManager.Instance.RemoveListener<MediumDifficultySelectedEvent>(MediumDifficultySelected);
        EventManager.Instance.RemoveListener<HardDifficultySelectedEvent>(HardDifficultySelected);
    }

    #endregion

    #region Event Call Back

    private void UpdateLoadingProgressionAudioClip(UpdateLoadingProgressionAudioClipEvent e)
    {
        UpdateLoadingMap(e.progression * 0.5f, "Loading Audio...");
    }

    private void UpdateLoadingMapData(UpdateLoadingMapDataEvent e)
    {
        UpdateLoadingMap(0.5f + (float) e.progression * 0.5f, "Loading Map...");
    }

    private void EasyDifficultySelected(EasyDifficultySelectedEvent e)
    {
        // On regarde si le joueur n'a pas déjà voté
        if (Players[e.PlayerID.Value].PlayerState == PlayerState.Voted)
        {
            return;
        }

        NbrVoteDifficulty++;
        CurrentDifficulty += EASY_DIFFICULTY;
        Players[e.PlayerID.Value].PlayerState = PlayerState.Voted;

        RefreshDifficultyBar();

        // On averti le client que le vote est bien recu.
        MessagingManager.Instance.RaiseNetworkedEventOnClient(
            new DifficultyVoteAcceptedEvent(e.PlayerID.Value));
    }

    private void MediumDifficultySelected(MediumDifficultySelectedEvent e)
    {
        // On regarde si le joueur n'a pas déjà voté
        if (Players[e.PlayerID.Value].PlayerState == PlayerState.Voted)
        {
            return;
        }

        NbrVoteDifficulty++;
        CurrentDifficulty += MEDIUM_DIFFICULTY;
        Players[e.PlayerID.Value].PlayerState = PlayerState.Voted;

        RefreshDifficultyBar();

        // On averti le client que le vote est bien recu.
        MessagingManager.Instance.RaiseNetworkedEventOnClient(
            new DifficultyVoteAcceptedEvent(e.PlayerID.Value));
    }

    private void HardDifficultySelected(HardDifficultySelectedEvent e)
    {
        // On regarde si le joueur n'a pas déjà voté
        if (Players[e.PlayerID.Value].PlayerState == PlayerState.Voted)
        {
            return;
        }

        NbrVoteDifficulty++;
        CurrentDifficulty += HARD_DIFFICULTY;
        Players[e.PlayerID.Value].PlayerState = PlayerState.Voted;

        RefreshDifficultyBar();

        // On averti le client que le vote est bien recu.
        MessagingManager.Instance.RaiseNetworkedEventOnClient(
            new DifficultyVoteAcceptedEvent(e.PlayerID.Value));
    }

    #endregion

    #region Tools

    private void InitializePlayersDefaultStates()
    {
        // On récupére la liste des joueurs
        IEnumerable<Player> value = Players.Values;

        foreach (Player p in value)
        {
            p.PlayerState = PlayerState.WaitingForTheVote;
        }
    }

    private void RefreshDifficultyBar()
    {
        DifficultyBar.value = CurrentDifficulty / NbrVoteDifficulty;

        if (DifficultyBar.value > 1f / 3f)
        {
            if (DifficultyBar.value > 2f / 3f)
            {
                DifficultyBarImage.color = HardColor;
            } else
            {
                DifficultyBarImage.color = MediumColor;
            }
        } else
        {
            DifficultyBarImage.color = EasyColor;
        }
    }

    #endregion
}
