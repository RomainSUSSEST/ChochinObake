namespace ClientManager
{
    using SDD.Events;
    using System.Collections;
    using UnityEngine;

    public enum GameState { gameMenu, gamePlay, gamePause }

    public class ClientGameManager : ClientManager<ClientGameManager>
    {
        #region Constant

        private readonly string ERROR_SERVER_MESSAGE = "Connection lost, verify that both wifi client and server are the same";

        #endregion

        #region Attributs
        private CharacterBody currentBody;
        private int LastScore;
        private int LastPowerUse;
        private int LastBestCombo;
        private int LastLanternSuccess;
        private int LastTotalLantern;
        #endregion
        
        #region Game State
        private GameState m_GameState;

        public GameState GetGameState { get { return m_GameState; } }
        #endregion

        #region Time
        private void SetTimeScale(float newTimeScale)
        {
            Time.timeScale = newTimeScale;
        }
        #endregion

        #region Requests

        public CharacterBody GetCurrentBody()
        {
            return currentBody;
        }

        public int GetLastScore()
        {
            return LastScore;
        }

        public int GetLastBestCombo()
        {
            return LastBestCombo;
        }

        public int GetLastPowerUse()
        {
            return LastPowerUse;
        }

        public int GetLastLanternSuccess()
        {
            return LastLanternSuccess;
        }

        public int GetLastTotalLantern()
        {
            return LastTotalLantern;
        }

        #endregion

        #region Events subscription
        public override void SubscribeEvents()
        {
            base.SubscribeEvents();

            // MainMenuManager
            EventManager.Instance.AddListener<JoinButtonClickedEvent>(JoinButtonClicked);
            EventManager.Instance.AddListener<LeaveButtonClickedEvent>(LeaveButtonClicked);
            EventManager.Instance.AddListener<PreviousCharacterSelectionButtonClickedEvent>(PreviousCharacterSelectionButtonClicked);
            EventManager.Instance.AddListener<ReadyCharacterSelectionButtonClickedEvent>(ReadyCharacterSelectionButtonClicked);
            EventManager.Instance.AddListener<RefreshCharacterInformationEvent>(RefreshSlimeInformation);
            EventManager.Instance.AddListener<ExitButtonClickedEvent>(ExitButtonClicked);

            // Network
            EventManager.Instance.AddListener<ServerConnectionSuccessEvent>(ServerConnectionSuccess);

            // Networked Event

            EventManager.Instance.AddListener<ServerClosedEvent>(ServerClosed);
            EventManager.Instance.AddListener<ServerEnterInGameMusicSelectionEvent>(ServerEnterInGameMusicSelection);
            EventManager.Instance.AddListener<ServerEnterInGameMusicResultEvent>(ServerEnterInGameMusicResult);
            EventManager.Instance.AddListener<GameStartedEvent>(GameStarted);
            EventManager.Instance.AddListener<ServerEnterInLobbyEvent>(ServerEnterInLobby);
            EventManager.Instance.AddListener<ServerEnterInPostGameEvent>(ServerEnterInPostGame);
        }

        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();

            // MainMenuManager
            EventManager.Instance.RemoveListener<JoinButtonClickedEvent>(JoinButtonClicked);
            EventManager.Instance.RemoveListener<LeaveButtonClickedEvent>(LeaveButtonClicked);
            EventManager.Instance.RemoveListener<PreviousCharacterSelectionButtonClickedEvent>(PreviousCharacterSelectionButtonClicked);
            EventManager.Instance.RemoveListener<ReadyCharacterSelectionButtonClickedEvent>(ReadyCharacterSelectionButtonClicked);
            EventManager.Instance.RemoveListener<RefreshCharacterInformationEvent>(RefreshSlimeInformation);
            EventManager.Instance.RemoveListener<ExitButtonClickedEvent>(ExitButtonClicked);

            // Network
            EventManager.Instance.RemoveListener<ServerConnectionSuccessEvent>(ServerConnectionSuccess);

            // Networked Event
            EventManager.Instance.RemoveListener<ServerClosedEvent>(ServerClosed);
            EventManager.Instance.RemoveListener<ServerEnterInGameMusicSelectionEvent>(ServerEnterInGameMusicSelection);
            EventManager.Instance.RemoveListener<ServerEnterInGameMusicResultEvent>(ServerEnterInGameMusicResult);
            EventManager.Instance.RemoveListener<GameStartedEvent>(GameStarted);
            EventManager.Instance.RemoveListener<ServerEnterInLobbyEvent>(ServerEnterInLobby);
            EventManager.Instance.RemoveListener<ServerEnterInPostGameEvent>(ServerEnterInPostGame);
        }
        #endregion

        #region Manager Implementation

        protected override IEnumerator Start()
        {
            yield return base.Start();

            // On attend que tous les managers soit pret
            while (!ManagersStates.AllManagersReady())
                yield return new CoroutineTools.WaitForFrames(1);

            // On lance le menu
            MainMenu("");
        }

        protected override IEnumerator InitCoroutine()
        {

            yield break;
        }
        #endregion

        #region Callbacks to Events issued by MenuManager

        private void ExitButtonClicked(ExitButtonClickedEvent e)
        {
            Application.Quit();
        }

        private void JoinButtonClicked(JoinButtonClickedEvent e)
        {
            JoinMenu();
        }

        private void LeaveButtonClicked(LeaveButtonClickedEvent e)
        {
            MainMenu("");
        }

        private void PreviousCharacterSelectionButtonClicked(PreviousCharacterSelectionButtonClickedEvent e)
        {
            JoinMenu();
        }

        private void ReadyCharacterSelectionButtonClicked(ReadyCharacterSelectionButtonClickedEvent e)
        {
            EventManager.Instance.Raise(new ReadyCharacterSelectionEvent());
        }
        #endregion

        #region GameState Methods

        private void MainMenu(string message)
        {
            SetTimeScale(1);
            m_GameState = GameState.gameMenu;
            EventManager.Instance.Raise(new MobileMainMenuEvent()
            {
                Message = message
            });
        }

        private void JoinMenu()
        {
            SetTimeScale(1);
            m_GameState = GameState.gameMenu;
            EventManager.Instance.Raise(new MobileJoinRoomEvent());
        }

        private void MusicSelection()
        {
            SetTimeScale(1);
            m_GameState = GameState.gameMenu;
            EventManager.Instance.Raise(new MobileMusicSelectionEvent());
        }

        private void MusicResult()
        {
            SetTimeScale(1);
            m_GameState = GameState.gameMenu;
            EventManager.Instance.Raise(new MobileMusicResultEvent());
        }

        private void Play()
        {
            SetTimeScale(1);
            m_GameState = GameState.gamePlay;
            EventManager.Instance.Raise(new MobileGamePlayEvent());
        }

        private void CharacterSelection()
        {
            SetTimeScale(1);
            m_GameState = GameState.gameMenu;
            EventManager.Instance.Raise(new MobileCharacterSelectionEvent());
        }

        private void PostGameMenu()
        {
            SetTimeScale(1);
            m_GameState = GameState.gameMenu;
            EventManager.Instance.Raise(new MobilePostGameEvent());
        }

        #endregion

        #region Callbacks to Event issued by NetworksManager
        private void ServerConnectionSuccess(ServerConnectionSuccessEvent e)
        {
            CharacterSelection();
        }
        #endregion

        #region Call back to Networked Event
        private void ServerClosed(ServerClosedEvent e)
        {
            MainMenu(ERROR_SERVER_MESSAGE);
        }

        private void ServerEnterInGameMusicSelection(ServerEnterInGameMusicSelectionEvent e)
        {
            MusicSelection();
        }

        private void ServerEnterInGameMusicResult(ServerEnterInGameMusicResultEvent e)
        {
            MusicResult();
        }

        private void GameStarted(GameStartedEvent e)
        {
            Play();
        }

        private void ServerEnterInLobby(ServerEnterInLobbyEvent e)
        {
            CharacterSelection();
        }

        private void ServerEnterInPostGame(ServerEnterInPostGameEvent e)
        {
            LastBestCombo = e.MaxCombo;
            LastLanternSuccess = e.TotalSuccess;
            LastPowerUse = e.PowerUse;
            LastScore = e.Score;
            LastTotalLantern = e.TotalLantern;

            PostGameMenu();
        }

        #endregion

        #region EventCallbackCharacterSelectionMenu
        private void RefreshSlimeInformation(RefreshCharacterInformationEvent e)
        {
            currentBody = e.body;
        }
        #endregion
    }
}
