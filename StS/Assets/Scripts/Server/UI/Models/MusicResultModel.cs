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
/// </summary>
public class MusicResultModel : MonoBehaviour
{
    // Constante

    [SerializeField] private Color PROGRESS_BAR_DEFAULT_COLOR = Color.white;
    [SerializeField] private Color PROGRESS_BAR_ERROR_COLOR = Color.red;


    // Attributs

    [Header("UI Elements")]

    [SerializeField] private TextMeshProUGUI Timer;
    [SerializeField] private TextMeshProUGUI SongTitle;


    [Header("Panel Config")]

    [SerializeField] private float TimerStartValue; // Temps initial du timer en seconde

    [Header("Progress Bar")]

    [SerializeField] private Slider ProgressBar; // Progress Bar
    [SerializeField] private TextMeshProUGUI ProgressBar_State; // Texte indiquant les étapes au sein de la progress bar


    private float TimerCurrentValue; // Valeur courante du timer
    private bool TimerIsEnd;

    private bool MapIsLoaded;
    private AudioClip CurrentAudioClip;
    private List<SpectralFluxInfo> CurrentMapData;


    #region Life Cycle
    
    private void OnEnable()
    {
        if (ServerGameManager.Instance.GetCurrentMusicPath() != null)
        {
            SubscribeEvents();

            // On initialise le timer
            TimerCurrentValue = TimerStartValue;
            TimerIsEnd = false;

            // On intialise les données de chargement de carte
            ProgressBar_State.color = PROGRESS_BAR_DEFAULT_COLOR;
            MapIsLoaded = false;

            // On initialise la musique gagnante
            SongTitle.text = Path.GetFileName(ServerGameManager.Instance.GetCurrentMusicPath());

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
                map = CurrentMapData
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
    }

    private void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<UpdateLoadingProgressionAudioClipEvent>(UpdateLoadingProgressionAudioClip);
        EventManager.Instance.RemoveListener<UpdateLoadingMapDataEvent>(UpdateLoadingMapData);
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

    #endregion
}
