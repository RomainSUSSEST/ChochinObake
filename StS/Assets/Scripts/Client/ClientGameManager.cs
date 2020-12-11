namespace ClientManager
{
    using SDD.Events;
    using System.Collections;
    using UnityEngine;

    public enum GameState { gameMenu, gameJoin, gamePlay, gamePause }

    public class ClientGameManager : ClientManager<ClientGameManager>
    {
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

        #region Events subscription
        public override void SubscribeEvents()
        {
            base.SubscribeEvents();

            // MainMenuManager
            EventManager.Instance.AddListener<JoinButtonClickedEvent>(JoinButtonClicked);
            EventManager.Instance.AddListener<LeaveButtonClickedEvent>(LeaveButtonClicked);
            EventManager.Instance.AddListener<PreviousCharacterSelectionButtonClickedEvent>(PreviousCharacterSelectionButtonClicked);
            EventManager.Instance.AddListener<ReadyCharacterSelectionButtonClickedEvent>(ReadyCharacterSelectionButtonClicked);

            // UI Resize
            EventManager.Instance.AddListener<ResizeUICompleteEvent>(ResizeUIComplete);

            // Network
            EventManager.Instance.AddListener<ServerConnectionSuccessEvent>(ServerConnectionSuccess);

            // Networked Event

            EventManager.Instance.AddListener<ServerClosedEvent>(ServerClosed);
            EventManager.Instance.AddListener<ServerEnterInGameMusicSelectionEvent>(ServerEnterInGameMusicSelection);
            EventManager.Instance.AddListener<GameStartedEvent>(GameStarted);
        }

        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();

            // MainMenuManager
            EventManager.Instance.RemoveListener<JoinButtonClickedEvent>(JoinButtonClicked);
            EventManager.Instance.RemoveListener<LeaveButtonClickedEvent>(LeaveButtonClicked);
            EventManager.Instance.RemoveListener<PreviousCharacterSelectionButtonClickedEvent>(PreviousCharacterSelectionButtonClicked);
            EventManager.Instance.RemoveListener<ReadyCharacterSelectionButtonClickedEvent>(ReadyCharacterSelectionButtonClicked);

            // UI Resize
            EventManager.Instance.RemoveListener<ResizeUICompleteEvent>(ResizeUIComplete);

            // Network
            EventManager.Instance.RemoveListener<ServerConnectionSuccessEvent>(ServerConnectionSuccess);

            // Networked Event
            EventManager.Instance.RemoveListener<ServerClosedEvent>(ServerClosed);
            EventManager.Instance.RemoveListener<ServerEnterInGameMusicSelectionEvent>(ServerEnterInGameMusicSelection);
            EventManager.Instance.RemoveListener<GameStartedEvent>(GameStarted);
        }
        #endregion

        #region Manager Implementation

        protected override IEnumerator InitCoroutine()
        {
            // On demande le redimenssionnement des UI.
            EventManager.Instance.Raise(new ResizeUIRequestEvent());

            yield break;
        }
        #endregion

        #region Callbacks to Events issued by MenuManager
        private void ResizeUIComplete(ResizeUICompleteEvent e)
        {
            MainMenu();
        }

        private void JoinButtonClicked(JoinButtonClickedEvent e)
        {
            JoinMenu();
        }

        private void LeaveButtonClicked(LeaveButtonClickedEvent e)
        {
            MainMenu();
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

        private void MainMenu()
        {
            SetTimeScale(1);
            m_GameState = GameState.gameMenu;
            EventManager.Instance.Raise(new MobileMainMenuEvent());
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
            m_GameState = GameState.gameJoin;
            EventManager.Instance.Raise(new MobileMusicSelectionEvent());
        }

        private void Play()
        {
            SetTimeScale(1);
            m_GameState = GameState.gamePlay;
            EventManager.Instance.Raise(new MobileGamePlayEvent());
        }

        #endregion

        #region Callbacks to Event issued by NetworksManager
        private void ServerConnectionSuccess(ServerConnectionSuccessEvent e)
        {
            SetTimeScale(1);
            m_GameState = GameState.gameMenu;
            EventManager.Instance.Raise(new MobileCharacterSelectionEvent());
        }
        #endregion

        #region Call back to Networked Event
        private void ServerClosed(ServerClosedEvent e)
        {
            MainMenu();
        }

        private void ServerEnterInGameMusicSelection(ServerEnterInGameMusicSelectionEvent e)
        {
            MusicSelection();
        }

        private void GameStarted(GameStartedEvent e)
        {
            Play();
        }
        #endregion
    }
}
