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

    private void OnEnable()
    {
        ChooseRandomSong();
    }


    // Outils

    private void ChooseRandomSong()
    {
        MusicSelected = randomSong[Random.Range(0, randomSong.Count)];
    }
}
