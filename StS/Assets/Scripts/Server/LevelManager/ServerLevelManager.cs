﻿namespace ServerManager
{
    using SDD.Events;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Le ServerLevelManager instancie le monde choisi et s'occupe de la partie
    /// GAMEPLAY & SCORING.
    /// </summary>
    public class ServerLevelManager : ServerManager<ServerLevelManager>
    {
        #region Constants

        public static readonly int MIN_NUMBER_WAVES = 12;

        public static readonly float DEFAULT_SPEED = 8f; // Vitesse par défaut
        public static readonly float MIN_SPEED = 17f;
        public static readonly float MAX_SPEED = 24f;

        public static readonly float ALGO_MIN_SENSITIVITY = 0.2f; // en % (gestion de la difficulté) 0 = D / 0.1 = M / 0.2 = F
        public static readonly float ALGO_MAX_SENSITIVITY = 0f;

        public static readonly float DISTANCE_BETWEEN_LINE = 4f;

        public static readonly float NBR_LINE = 4; // Nombre de ligne immatérielle composant le jeu.
        public static readonly float LINE_SLIME_SPAWN = 3f / 4f; // Multiplicateur indiquant à quelle position initialement placé les joueurs par rapport aux nombres de lignes.

        private static readonly float TIME_BETWEEN_IN_GAME_EVENTS = 25;

        #endregion

        #region Attributes

        [SerializeField] private World WorldJapan;

        [SerializeField] private List<InGameEvents> AllInGameEventsList;

        private GameObject CurrentWorld;

        private bool GenerateInGameEvents;
        private IReadOnlyCollection<CharacterPlayer> RoundPlayers;

        #endregion

        // Requetes


        // Méthodes

        #region Manager implementation
        protected override IEnumerator InitCoroutine()
        {
            yield break;
        }
        #endregion

        #region Subs methods

        public override void SubscribeEvents()
        {
            base.SubscribeEvents();

            EventManager.Instance.AddListener<RoundStartEvent>(RoundStart);
            EventManager.Instance.AddListener<MusicRoundEndEvent>(MusicRoundEnd);
        }

        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();

            EventManager.Instance.RemoveListener<RoundStartEvent>(RoundStart);
            EventManager.Instance.RemoveListener<MusicRoundEndEvent>(MusicRoundEnd);
        }

        #endregion


        // Event Call Back

        #region GameManager Events

        protected override void GamePlay(GamePlayEvent e)
        {
            base.GamePlay(e);

            CurrentWorld = Instantiate(WorldJapan.gameObject);
        }

        #endregion


        // Outils

        #region CallBack Event

        private void RoundStart(RoundStartEvent e)
        {
            GenerateInGameEvents = true;
            RoundPlayers = e.RoundPlayers;

            StartCoroutine("InGameEvents");
        }

        private void MusicRoundEnd(MusicRoundEndEvent e)
        {
            GenerateInGameEvents = false;
        }

        #endregion

        #region Coroutines

        private IEnumerator InGameEvents()
        {
            while (GenerateInGameEvents)
            {
                yield return new WaitForSeconds(TIME_BETWEEN_IN_GAME_EVENTS);

                // On choisi un event
                int index = (int) ServerMusicManager.Instance.GetTimeLeftRoundMusic() % AllInGameEventsList.Count;

                foreach (CharacterServer c in RoundPlayers)
                {
                    if (c != null)
                    {
                        InGameEvents e = Instantiate(AllInGameEventsList[index], c.transform);
                        e.SetAssociatedCharacter(c);
                    }
                }
            }
        }

        #endregion
    }
}