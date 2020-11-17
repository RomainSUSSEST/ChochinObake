using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicResultModel : MonoBehaviour
{
    // Attributs

    [Header("MusicResult")]

    [SerializeField] private List<AudioClip> randomSong;
    private AudioClip MusicSelected;


    // Life Cycle

    private void Awake()
    {
        SubscribeEvent();
    }

    private void OnDestroy()
    {
        UnsubscribeEvent();
    }

    private void OnEnable()
    {
        ChooseRandomSong();
    }


    // Event Subscription

    private void SubscribeEvent()
    {
        // GameManager
        EventManager.Instance.AddListener<AskForNewRoundEvent>(SendMusicSelected);
    }

    private void UnsubscribeEvent()
    {
        // GameManager
        EventManager.Instance.RemoveListener<AskForNewRoundEvent>(SendMusicSelected);
    }


    // GameManager Event Call

    private void SendMusicSelected(AskForNewRoundEvent e)
    {
        EventManager.Instance.Raise(new SetMusicRoundEvent(MusicSelected));
    }


    // Outils

    private void ChooseRandomSong()
    {
        MusicSelected = randomSong[Random.Range(0, randomSong.Count)];
    }
}
