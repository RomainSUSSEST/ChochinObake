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
        #region Power

        #region Attributes

        [SerializeField] private Sprite ResetAllCombo_Sprite;
        [SerializeField] private Sprite Shield_Sprite;
        [SerializeField] private Sprite InvertKanji_Sprite;
        [SerializeField] private Sprite UncolorKanji_Sprite;
        [SerializeField] private Sprite FlashKanji_Sprite;
        [SerializeField] private Sprite InvertInput_Sprite;
        [SerializeField] private Sprite DisableOtherPlayers_Sprite;
        [SerializeField] private Sprite NoPower_Sprite;

        #endregion

        public enum Power : int
        {
            ResetAllCombo = -12,
            Shield = -6,
            NoPower = 0,
            InvertKanji = 8,
            UncolorKanji = 16,
            FlashKanji = 24,
            InvertInput = 32,
            DisableOtherPlayers = 40
        }

        #region Tools

        /// <summary>
        /// Renvoie le sprite correspondant au pouvoir p
        /// null si default.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Sprite GetAssociatedSprite(Power p)
        {
            switch (p)
            {
                case Power.ResetAllCombo:
                    return ResetAllCombo_Sprite;
                case Power.Shield:
                    return Shield_Sprite;
                case Power.InvertKanji:
                    return InvertKanji_Sprite;
                case Power.UncolorKanji:
                    return UncolorKanji_Sprite;
                case Power.FlashKanji:
                    return FlashKanji_Sprite;
                case Power.InvertInput:
                    return InvertInput_Sprite;
                case Power.DisableOtherPlayers:
                    return DisableOtherPlayers_Sprite;
                default:
                    return NoPower_Sprite;
            }
        }

        #endregion

#endregion

        #region Constants

        public static readonly float DEFAULT_SPEED = 12f; // Vitesse par défaut
        public static readonly float MIN_SPEED = 20f;
        public static readonly float MAX_SPEED = 30f;

        public static readonly float ALGO_MIN_SENSITIVITY = 0.7f; // en % (gestion de la difficulté) 0 = D / 0.1 = M / 0.2 = F
        public static readonly float ALGO_MAX_SENSITIVITY = 0.225f;

        public static readonly float MAXIMUM_ADVANCE_DISTANCE = 20;

        public static readonly int MIN_SAFE_PLAYER = 1;

        #region GamePlay

        private static readonly float MAX_SCORE = 1000;
        private static readonly float MIN_SCORE = 0;
        private static readonly float TIME_BETWEEN_IN_GAME_EVENTS = 25;

        #region Bonus


        #endregion

        #region Malus

        private static readonly float SLEEP_DELAI = 3;
        private static readonly float INVERT_KANJI_DELAI = 6;
        private static readonly float FLASH_KANJI_DELAI = 5;
        private static readonly float DELAI_INTER_FLASH = 0.4f;

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

        private IReadOnlyCollection<CharacterServer> RoundPlayers; // Contient le charactere à la wave i ou null (si deconnexion par exemple)
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
            EventManager.Instance.AddListener<RoundEndEvent>(RoundEnd);

            // Player

            EventManager.Instance.AddListener<PowerDeclenchementEvent>(PowerDeclenchement);
            EventManager.Instance.AddListener<PowerStartEvent>(PowerStart);
        }

        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();

            EventManager.Instance.RemoveListener<RoundStartEvent>(RoundStart);
            EventManager.Instance.RemoveListener<MusicRoundEndEvent>(MusicRoundEnd);
            EventManager.Instance.RemoveListener<RoundEndEvent>(RoundEnd);

            // Player

            EventManager.Instance.RemoveListener<PowerDeclenchementEvent>(PowerDeclenchement);
            EventManager.Instance.RemoveListener<PowerStartEvent>(PowerStart);
        }

        #endregion


        // Event Call Back

        #region Players

        private void PowerDeclenchement(PowerDeclenchementEvent e)
        {
            if (CanUsePower(e.CmptCombo))
            {
                e.CharacterServer.LockPowerInput();
                e.CharacterServer.ResetCombo(); // On reset les combos du joueur
                e.CharacterServer.UsePowerEffect(e.CmptCombo);
            }
        }

        private void PowerStart(PowerStartEvent e)
        {
            UsePower(e.CharacterServer, e.CmptCombo);
            e.CharacterServer.UnLockPowerInput();
        }

        private bool CanUsePower(int CmptCombo)
        {
            return CmptCombo <= (int)Power.Shield || CmptCombo >= (int)Power.InvertKanji;
        }

        /// <summary>
        /// Utlise un bonus sur le joueur "Player"
        /// </summary>
        /// <param name="Player"></param>
        /// <param name="CmptCombo"></param>
        /// <returns></returns>
        private void UsePower(CharacterServer Player, int CmptCombo)
        {
            if (CmptCombo <= (int)Power.ResetAllCombo)
            {
                ResetAllCombo();
            }
            else if (CmptCombo <= (int)Power.Shield)
            {
                Shield(Player);
            }
            else if (CmptCombo >= (int)Power.DisableOtherPlayers)
            {
                DisableOtherPlayers(Player); // A changer
            }
            else if (CmptCombo >= (int)Power.InvertInput)
            {
                InvertInput(Player);
            }
            else if (CmptCombo >= (int)Power.FlashKanji)
            {
                FlashKanji(Player);
            }
            else if (CmptCombo >= (int)Power.UncolorKanji)
            {
                UncolorKanji(Player);
            }
            else if (CmptCombo >= (int)Power.InvertKanji)
            {
                InvertKanji(Player);
            }
            else
            {
                return;
            }
        }

        #endregion

        #region Powers

        #region Bonus
        private void ResetAllCombo()
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
        /// Essai de mettre un shield sur le joueur appelant
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private void Shield(CharacterServer target)
        {
            AddSafePlayer(target);
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

        /// <summary>
        /// Inverse les touches de jusqu'à 2 joueurs
        /// Ne fais rien en cas d'échec
        /// </summary>
        /// <param name="Exception"></param>
        /// <returns></returns>
        private void InvertInput(CharacterServer Exception)
        {
            // On repere les victimes potentiel
            List<CharacterServer> PotentialTargets = new List<CharacterServer>();

            foreach (CharacterServer c in RoundPlayers)
            {
                if (c != null && c.AssociedClientID != Exception.AssociedClientID && !c.IsSafe())
                {
                    PotentialTargets.Add(c);
                }
            }

            // Si il n'y a aucune potentiel victime
            if (PotentialTargets.Count == 0)
                return;

            // On recherche une victime
            CharacterServer target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
            target.InvertInput(INVERT_KANJI_DELAI);
            AddSafePlayer(target);

            PotentialTargets.Remove(target);

            if (PotentialTargets.Count == 0)
            {
                return;
            }

            // On recherche une 2ème victime
            target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
            target.InvertInput(INVERT_KANJI_DELAI);
            AddSafePlayer(target);
        }
        
        /// <summary>
        /// Fait clignoter les kanji de jusqu'à 2 joueurs
        /// Les mets ensuite en safe
        /// Ne fait rien en cas d'échec
        /// </summary>
        /// <returns></returns>
        private void FlashKanji(CharacterServer Exception)
        {
            // On repère les victimes potentiel
            List<CharacterServer> PotentialTargets = new List<CharacterServer>();

            foreach (CharacterServer c in RoundPlayers)
            {
                if (c!= null && c.AssociedClientID != Exception.AssociedClientID && !c.IsSafe())
                {
                    PotentialTargets.Add(c);
                }
            }

            // Si il n'y a aucune potentiel victime
            if (PotentialTargets.Count == 0)
            {
                return;
            }

            // On recherche une victime
            CharacterServer target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
            target.FlashKanji(FLASH_KANJI_DELAI, DELAI_INTER_FLASH);
            AddSafePlayer(target);

            PotentialTargets.Remove(target);

            if (PotentialTargets.Count == 0)
            {
                return;
            }

            // On recherche une 2ème victime
            target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
            target.FlashKanji(FLASH_KANJI_DELAI, DELAI_INTER_FLASH);
            AddSafePlayer(target);
        }

        /// <summary>
        /// Les kanjis d'un joueur deviennent gris.
        /// Place ensuite le joueur en safe.
        /// Ne fais rien en cas d'échec
        /// </summary>
        /// <returns></returns>
        private void UncolorKanji(CharacterServer Exception)
        {
            // On repère les victimes potentiel
            List<CharacterServer> PotentialTargets = new List<CharacterServer>();

            foreach (CharacterServer c in RoundPlayers)
            {
                if (c != null && c.AssociedClientID != Exception.AssociedClientID && !c.IsSafe())
                {
                    PotentialTargets.Add(c);
                }
            }

            // Si il n'y a aucune potentiel victime
            if (PotentialTargets.Count == 0)
            {
                return;
            }

            // On recherche une victime
            CharacterServer target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
            target.UncolorKanji();
            AddSafePlayer(target);
        }

        /// <summary>
        /// Les kanji d'un joueur s'inverse.
        /// Place ensuite le joueur en safe.
        /// Ne fait rien en cas d'échec
        /// </summary>
        /// <param name="Exception"></param>
        private void InvertKanji(CharacterServer Exception)
        {
            // On repère les victimes potentiel
            List<CharacterServer> PotentialTargets = new List<CharacterServer>();

            foreach (CharacterServer c in RoundPlayers)
            {
                if (c != null && c.AssociedClientID != Exception.AssociedClientID && !c.IsSafe())
                {
                    PotentialTargets.Add(c);
                }
            }

            // Si il n'y a aucune potentiel victime
            if (PotentialTargets.Count == 0)
            {
                return;
            }

            // On recherche une victime
            CharacterServer target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];

            target.InvertKanji();  
            AddSafePlayer(target);
        }

        #endregion

        #region Tools

        /// <summary>
        /// Ajoute le joueur "Target" à la liste des joueurs safe
        /// Si le nombre min de joueur safe est atteint,
        /// replace tout le monde en non safe.
        /// 
        /// Ne marche que si nmbr de joueurs > MIN_SAFE_PLAYER
        /// 
        /// Ne fait rien en cas d'échec
        /// </summary>
        private void AddSafePlayer(CharacterServer Target)
        {
            // On calcul le nombre de joueur effectif en jeu ----
            int cmpt = 0;

            foreach (CharacterServer c in RoundPlayers)
                if (c != null)
                    ++cmpt;

            // Si le nombre de joueurs dans la partie est insuffisant au système de safe, on annule. ----
            if (cmpt <= MIN_SAFE_PLAYER + 1) 
                return;

            // Systeme de safe
            if (++SafePlayerCount >= cmpt - MIN_SAFE_PLAYER)
            {
                foreach (CharacterServer c in RoundPlayers)
                {
                    if (c != null)
                        c.SetSafeStatus(false);
                }
                SafePlayerCount = 0;
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

            if (CurrentWorld != null)
            {
                Destroy(CurrentWorld);
            }

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

        protected override void GameResult(GameResultEvent e)
        {
            base.GameResult(e);

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

        private void RoundEnd(RoundEndEvent e)
        {
            IReadOnlyDictionary<ulong, Player> Players = ServerGameManager.Instance.GetPlayers();

            foreach (CharacterServer p in RoundPlayers)
            {
                if (p != null)
                {
                    if (p.IsAI)
                    {
                        p.AssociatedAIManager.GetAssociatedProfil().Score +=
                            (int)Mathf.Lerp(
                            MIN_SCORE,
                            MAX_SCORE,
                            p.GetTotalObstacle() == 0 ? 1 : (float) p.GetTotalSuccess() / p.GetTotalObstacle());
                    } else
                    {
                        Players[p.AssociedClientID].Score +=
                            (int)Mathf.Lerp(
                            MIN_SCORE,
                            MAX_SCORE,
                            p.GetTotalObstacle() == 0 ? 1 : (float) p.GetTotalSuccess() / p.GetTotalObstacle());
                    }
                }
            }

            Destroy(CurrentWorld.gameObject);

            EventManager.Instance.Raise(new ScoreUpdatedEvent());
        }

        #endregion

        #region Coroutines

        private IEnumerator InGameEventsManager()
        {
            //while (GenerateInGameEvents)
            //{
            //    yield return new WaitForSeconds(TIME_BETWEEN_IN_GAME_EVENTS);

            //    // On choisi un event
            //    int index = Random.Range(0, AllInGameEventsList.Count);

            //    foreach (CharacterServer c in RoundPlayers)
            //    {
            //        if (c != null)
            //        {
            //            InGameEvents e = Instantiate(AllInGameEventsList[index], c.transform);
            //            e.SetAssociatedCharacter(c);
            //        }
            //    }

                yield return new WaitForSeconds(InGameEvents.EVENT_TIME);
            //}
        }

        #endregion

        #endregion
    }
}