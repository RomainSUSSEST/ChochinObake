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
            NoPower = 0,
            InvertKanji = 8,
            UncolorKanji = 16,
            FlashKanji = 24,
            InvertInput = 32,
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
                case Power.InvertKanji:
                    return InvertKanji_Sprite;
                case Power.UncolorKanji:
                    return UncolorKanji_Sprite;
                case Power.FlashKanji:
                    return FlashKanji_Sprite;
                case Power.InvertInput:
                    return InvertInput_Sprite;
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

        public static readonly float MAXIMUM_ADVANCE_DISTANCE = 30; // Distance maximal d'avancé des joueurs

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

                e.CharacterServer.UsePowerEffect(e.CmptCombo, GetTargets(e.CharacterServer, e.CmptCombo));
            }
        }

        private void PowerStart(PowerStartEvent e)
        {
            UsePower(e.Targets, e.CmptCombo);
            e.Player.UnLockPowerInput();
        }

        private bool CanUsePower(int CmptCombo)
        {
            return CmptCombo <= (int)Power.ResetAllCombo || CmptCombo >= (int)Power.InvertKanji;
        }

        /// <summary>
        /// Utlise un bonus sur le joueur "Player"
        /// </summary>
        /// <param name="Player"></param>
        /// <param name="CmptCombo"></param>
        /// <returns></returns>
        private void UsePower(List<CharacterServer> targets, int CmptCombo)
        {
            if (CmptCombo <= (int)Power.ResetAllCombo)
            {
                ResetAllCombo(targets);
            }
            else if (CmptCombo >= (int)Power.InvertInput)
            {
                InvertInput(targets);
            }
            else if (CmptCombo >= (int)Power.FlashKanji)
            {
                FlashKanji(targets);
            }
            else if (CmptCombo >= (int)Power.UncolorKanji)
            {
                UncolorKanji(targets);
            }
            else if (CmptCombo >= (int)Power.InvertKanji)
            {
                InvertKanji(targets);
            }
            else
            {
                return;
            }
        }

        private List<CharacterServer> GetTargets(CharacterServer Player, int CmptCombo)
        {
            List<CharacterServer> tampon = new List<CharacterServer>();
            if (CmptCombo <= (int)Power.ResetAllCombo)
            {
                foreach (CharacterServer c in RoundPlayers)
                {
                    if (c != null)
                    {
                        tampon.Add(c);
                    }
                }
                return tampon;
            }
            else if (CmptCombo >= (int)Power.InvertInput || CmptCombo >= (int)Power.FlashKanji) // 2 Targets
            {
                // On repere les victimes potentiel
                List<CharacterServer> PotentialTargets = new List<CharacterServer>();

                foreach (CharacterServer c in RoundPlayers)
                {
                    if (c != null && c != Player && !c.IsSafe())
                    {
                        PotentialTargets.Add(c);
                    }
                }

                // Si il n'y a aucune potentiel victime
                if (PotentialTargets.Count == 0)
                    return tampon;

                CharacterServer target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
                PotentialTargets.Remove(target);
                tampon.Add(target);

                if (PotentialTargets.Count == 0)
                {
                    return tampon;
                }

                target = PotentialTargets[Random.Range(0, PotentialTargets.Count)];
                tampon.Add(target);

                return tampon;

            }
            else if (CmptCombo >= (int)Power.UncolorKanji || CmptCombo >= (int)Power.InvertKanji) // 1 target
            {
                // On repère les victimes potentiel
                List<CharacterServer> PotentialTargets = new List<CharacterServer>();

                foreach (CharacterServer c in RoundPlayers)
                {
                    if (c != null && c != Player && !c.IsSafe())
                    {
                        PotentialTargets.Add(c);
                    }
                }

                // Si il n'y a aucune potentiel victime
                if (PotentialTargets.Count == 0)
                {
                    return tampon;
                }

                // On recherche une victime
                tampon.Add(PotentialTargets[Random.Range(0, PotentialTargets.Count)]);
                return tampon;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Powers

        #region Bonus
        private void ResetAllCombo(List<CharacterServer> targets)
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
        //private void Shield(List<CharacterServer> targets)
        //{
        //    foreach (CharacterServer c in targets)
        //    {
        //        AddSafePlayer(c);
        //    }
        //}

        #endregion

        #region Malus

        /// <summary>
        /// Endort tous les joueurs sauf Exception
        /// </summary>
        /// <param name="Exception"></param>
        //private void DisableOtherPlayers(CharacterServer Exception)
        //{
        //    foreach (CharacterServer c in RoundPlayers)
        //    {
        //        if (c != null && c.AssociedClientID != Exception.AssociedClientID)
        //        {
        //            c.Sleep(SLEEP_DELAI);
        //        }
        //    }
        //}

        /// <summary>
        /// Inverse les touches de jusqu'à 2 joueurs
        /// Ne fais rien en cas d'échec
        /// </summary>
        /// <param name="Exception"></param>
        /// <returns></returns>
        private void InvertInput(List<CharacterServer> targets)
        {
            foreach(CharacterServer c in targets)
            {
                c.InvertInput(INVERT_KANJI_DELAI);
                AddSafePlayer(c);
            }
        }
        
        /// <summary>
        /// Fait clignoter les kanji de jusqu'à 2 joueurs
        /// Les mets ensuite en safe
        /// Ne fait rien en cas d'échec
        /// </summary>
        /// <returns></returns>
        private void FlashKanji(List<CharacterServer> targets)
        {
            foreach (CharacterServer c in targets)
            {
                c.FlashKanji(FLASH_KANJI_DELAI, DELAI_INTER_FLASH);
                AddSafePlayer(c);
            }
        }

        /// <summary>
        /// Les kanjis d'un joueur deviennent gris.
        /// Place ensuite le joueur en safe.
        /// Ne fais rien en cas d'échec
        /// </summary>
        /// <returns></returns>
        private void UncolorKanji(List<CharacterServer> targets)
        {
            foreach (CharacterServer c in targets)
            {
                c.UncolorKanji();
                AddSafePlayer(c);
            }

        }

        /// <summary>
        /// Les kanji d'un joueur s'inverse.
        /// Place ensuite le joueur en safe.
        /// Ne fait rien en cas d'échec
        /// </summary>
        /// <param name="Exception"></param>
        private void InvertKanji(List<CharacterServer> targets)
        {
            foreach (CharacterServer c in targets)
            {
                c.InvertKanji();
                AddSafePlayer(c);
            }
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
            List<AI_Player> aiWinners = new List<AI_Player>(); // Les AI gagnantes
            List<Player> playerWinners = new List<Player>(); // Les joueurs gagnants

            int CurrentScore;
            int bestCurrentScore = 0;

            IReadOnlyDictionary<ulong, Player> Players = ServerGameManager.Instance.GetPlayers();

            foreach (CharacterServer p in RoundPlayers)
            {
                if (p != null)
                {
                    if (p.IsAI)
                    {
                        AI_Player aiProfil = p.AssociatedAIManager.GetAssociatedProfil();

                        CurrentScore = (int)Mathf.Lerp(
                            MIN_SCORE,
                            MAX_SCORE,
                            p.GetTotalObstacle() == 0 ? 1 : (float)p.GetTotalSuccess() / p.GetTotalObstacle());

                        aiProfil.Score += CurrentScore;
                        aiProfil.LastGameBestCombo = p.GetBestCombo();
                        aiProfil.LastGameLanternSuccess = p.GetTotalSuccess();
                        aiProfil.LastGamePowerUse = p.GetPowerUse();
                        aiProfil.lastGameTotalLantern = p.GetTotalObstacle();

                        if (CurrentScore > bestCurrentScore) // Si le gagnant est battu, on clear
                        {
                            aiWinners.Clear();
                            aiWinners.Add(aiProfil); // On ajoute le gagnant
                        } else if (CurrentScore == bestCurrentScore) // Si égalité
                        {
                            aiWinners.Add(aiProfil);
                        }
                    } else
                    {
                        Player playerProfil = Players[p.AssociedClientID];

                        CurrentScore = (int)Mathf.Lerp(
                            MIN_SCORE,
                            MAX_SCORE,
                            p.GetTotalObstacle() == 0 ? 1 : (float)p.GetTotalSuccess() / p.GetTotalObstacle());

                        playerProfil.Score += CurrentScore;
                        playerProfil.LastGameBestCombo = p.GetBestCombo();
                        playerProfil.LastGameLanternSuccess = p.GetTotalSuccess();
                        playerProfil.LastGamePowerUse = p.GetPowerUse();
                        playerProfil.lastGameTotalLantern = p.GetTotalObstacle();

                        if (CurrentScore > bestCurrentScore) // Si le gagnant est battu, on clear
                        {
                            aiWinners.Clear();
                            playerWinners.Clear();
                            playerWinners.Add(playerProfil); // On ajoute le gagnant
                        }
                        else if (CurrentScore == bestCurrentScore) // Si égalité
                        {
                            playerWinners.Add(playerProfil);
                        }
                    }
                }
            }

            // On incrémente le nombre de victoire du/des gagnant(s)

            foreach (AI_Player ai in aiWinners)
            {
                ai.Victory++;
            }

            foreach (Player p in playerWinners)
            {
                p.Victory++;
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