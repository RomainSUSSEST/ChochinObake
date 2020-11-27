using SDD.Events;
using ServerManager;
using System.IO;
using TMPro;
using UnityEngine;

public class MusicResultModel : MonoBehaviour
{
    // Attributs
 
    [Header("UI Elements")]

    [SerializeField] private TextMeshProUGUI Timer;
    [SerializeField] private TextMeshProUGUI SongTitle;


    [Header("Panel Config")]

    [SerializeField] private float TimerStartValue; // Temps initial du timer en seconde


    private float TimerCurrentValue; // Valeur courante du timer


    #region Life Cycle
    
    private void OnEnable()
    {
        if (ServerGameManager.Instance.GetCurrentMusicPath() != null)
        {
            // On initialise le timer
            TimerCurrentValue = TimerStartValue;
            SongTitle.text = Path.GetFileName(ServerGameManager.Instance.GetCurrentMusicPath());

            // Initialisation de la page

            RefreshTimerUI(); // Affichage du timer
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
    }

    #endregion


    // Outils

    #region Timer

    private void TimerEnd()
    {
        EventManager.Instance.Raise(new MusicResultTimerEndEvent());
    }

    private void RefreshTimerUI()
    {
        Timer.text = Mathf.Ceil(TimerCurrentValue).ToString() + 's';
    }

    #endregion
}
