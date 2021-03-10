namespace ServerManager
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
        #region Enum

        public enum Bonus : int
        {
            SubstractCombo = -12,
            Shield = -6
        }

        private enum Malus : int
        {
            InvertKanji = 8,
            UncolorKanji = 16,
            FlashKanji = 24,
            InvertInput = 32,
            DisableOtherPlayers = 40
        }

        #endregion

        #region Constants

        public static readonly int MIN_NUMBER_WAVES = 12;

        public static readonly float DEFAULT_SPEED = 12f; // Vitesse par défaut
        public static readonly float MIN_SPEED = 20f;
        public static readonly float MAX_SPEED = 35f;

        public static readonly float ALGO_MIN_SENSITIVITY = 0.7f; // en % (gestion de la difficulté) 0 = D / 0.1 = M / 0.2 = F
        public static readonly float ALGO_MAX_SENSITIVITY = 0.225f;

        public static readonly float MAXIMUM_ADVANCE_DISTANCE = 20;

        public static readonly int MIN_SAFE_PLAYER = 1;

        #region GamePlay

        private static readonly float TIME_BETWEEN_IN_GAME_EVENTS = 25;

        #region Bonus


        #endregion

        #region Malus

        private static readonly float SLEEP_DELAI = 3;

        #endregion

        #endregion

        #endregion

        #region Attributes

        [SerializeField] private World WorldJapan;
        [SerializeField] private GameObject DepartureWorldJapan; // Départ

        [SerializeField] private Transform DepartureSpawn;

        [SerializeField] private List<InGameEvents> AllInGameEventsList;

        private GameObject CurrentWorld;
        private GameObject CurrentMenuBackground;

        private bool GenerateInGameEvents;

        private IReadOnlyCollection<CharacterPlayer> RoundPlayers; // Contient le charactere à la wave i ou null (si deconnexion par exemple)
        private int SafePlayerCount;

        #endregion

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

            // Player

            EventManager.Instance.AddListener<PowerDeclenchementEvent>(PowerDeclenchement);
        }

        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();

            EventManager.Instance.RemoveListener<RoundStartEvent>(RoundStart);
            EventManager.Instance.RemoveListener<MusicRoundEndEvent>(MusicRoundEnd);

            // Player

            EventManager.Instance.RemoveListener<PowerDeclenchementEvent>(PowerDeclenchement);
        }

        #endregion


        // Event Call Back

        #region Players

        private void PowerDeclenchement(PowerDeclenchementEvent e)
        {
            if (e.CmptCombo < 0)
            {
                if (UseBonus(e.CharacterServer, e.CmptCombo)) // Si un bonus est utilisé
                {
                    e.CharacterServer.ResetCombo(); // On reset les combos du joueur
                }
                    
            } else 
            {
                if (UseMalus(e.CharacterServer, e.CmptCombo)) // Si un malus est utilisé
                {
                    e.CharacterServer.ResetCombo(); // On reset les combos du joueur
                }
            }
        }

        /// <summary>
        /// Tente d'utliser un bonus sur le joueur "Player"
        /// renvoie true en cas de succès, false en cas d'échec.
        /// </summary>
        /// <param name="Player"></param>
        /// <param name="CmptCombo"></param>
        /// <returns></returns>
        private bool UseBonus(CharacterServer Player, int CmptCombo)
        {
            if (CmptCombo <= (int) Bonus.SubstractCombo)
            {
                ResetCombo();
                return true;

            } else if (CmptCombo <= (int) Bonus.Shield)
            {
                return Shield(Player);
            } else
            {
                return false;
            }

        }

        private bool UseMalus(CharacterServer Player, int CmptCombo)
        {
            if (CmptCombo >= (int) Malus.DisableOtherPlayers)
            {
                DisableOtherPlayers(Player);
                return true;
            } else if (CmptCombo >= (int) Malus.InvertInput)
            {
                //InvertInput(Player);
                return true;
            } else if (CmptCombo >= (int) Malus.FlashKanji)
            {
                //FlashKanji(Player);
                return true;
            } else if (CmptCombo >= (int) Malus.UncolorKanji)
            {
                //UncolorKanji(Player);
                return true;
            } else if (CmptCombo >= (int) Malus.InvertKanji)
            {
                //InvertKanji(Player);
                return true;
            } else
            {
                return false;
            }
        }

        #endregion

        #region Powers

        #region Bonus
        private void ResetCombo()
        {
            foreach (CharacterServer c in RoundPlayers)
            {
                if (c != null)
                {
                    c.ResetCombo();
                }
            }
        }

        /// <summary>
        /// Essai de mettre un shield sur le joueur appelant, renvois false
        /// si le joueur est déjà safe.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool Shield(CharacterServer target)
        {
            if (target.IsSafe())
            {
                return false;
            } else
            {
                AddSafePlayer(target);
                return true;
            }
        }

        #endregion

        #region Malus

        /// <summary>
        /// Endort tous les joueurs sauf Exception
        /// </summary>
        /// <param name="Exception"></param>
        private void DisableOtherPlayers(CharacterServer Exception)
        {
            foreach (CharacterServer c in RoundPlayers)
            {
                if (c != null && c.AssociedClientID != Exception.AssociedClientID)
                {
                    c.Sleep(SLEEP_DELAI);
                }
            }
        }
        

        #endregion

        #region Tools

        /// <summary>
        /// Ajoute le joueur "Target" à la liste des joueurs safe
        /// Si le nombre min de joueur safe est atteint,
        /// replace tout le monde en non safe.
        /// </summary>
        private void AddSafePlayer(CharacterServer Target)
        {
            // On calcul le nombre de joeuur effectif en jeu
            int cmpt = 0;

            foreach (CharacterServer c in RoundPlayers)
                if (c != null)
                    ++cmpt;

            if (++SafePlayerCount >= cmpt - MIN_SAFE_PLAYER)
            {
                foreach (CharacterServer c in RoundPlayers)
                {
                    if (c != null)
                        c.SetSafeStatus(false);
                }
            } else
            {
                Target.SetSafeStatus(true);
            }
        }

        #endregion

        #endregion

        #region GameManager Events

        protected override void GamePlay(GamePlayEvent e)
        {
            base.GamePlay(e);

            Destroy(CurrentMenuBackground);
            CurrentWorld = Instantiate(WorldJapan.gameObject);
        }

        protected override void GameMainMenu(GameMainMenuEvent e)
        {
            base.GameMainMenu(e);

            if (CurrentMenuBackground == null)
            {
                CurrentMenuBackground = Instantiate(DepartureWorldJapan, DepartureSpawn);
            }
        }

        protected override void GameRoomMenu(GameRoomMenuEvent e)
        {
            base.GameRoomMenu(e);

            if (CurrentMenuBackground == null)
            {
                CurrentMenuBackground = Instantiate(DepartureWorldJapan, DepartureSpawn);
            }
        }

        protected override void GameMusicSelectionMenu(GameMusicSelectionMenuEvent e)
        {
            base.GameMusicSelectionMenu(e);

            if (CurrentMenuBackground == null)
            {
                CurrentMenuBackground = Instantiate(DepartureWorldJapan, DepartureSpawn);
            }
        }

        protected override void GameMusicResultMenu(GameMusicResultMenuEvent e)
        {
            base.GameMusicResultMenu(e);

            if (CurrentMenuBackground == null)
            {
                CurrentMenuBackground = Instantiate(DepartureWorldJapan, DepartureSpawn);
            }
        }

        #endregion

        #region Tools

        #region CallBack Event

        private void RoundStart(RoundStartEvent e)
        {
            GenerateInGameEvents = true;
            RoundPlayers = e.RoundPlayers;

            StartCoroutine("InGameEventsManager");
        }

        private void MusicRoundEnd(MusicRoundEndEvent e)
        {
            GenerateInGameEvents = false;
        }

        #endregion

        #region Coroutines

        private IEnumerator InGameEventsManager()
        {
            while (GenerateInGameEvents)
            {
                yield return new WaitForSeconds(TIME_BETWEEN_IN_GAME_EVENTS);

                // On choisi un event
                int index = Random.Range(0, AllInGameEventsList.Count);

                foreach (CharacterServer c in RoundPlayers)
                {
                    if (c != null)
                    {
                        InGameEvents e = Instantiate(AllInGameEventsList[index], c.transform);
                        e.SetAssociatedCharacter(c);
                    }
                }

                yield return new WaitForSeconds(InGameEvents.EVENT_TIME);
            }
        }

        #endregion

        #endregion
    }
}