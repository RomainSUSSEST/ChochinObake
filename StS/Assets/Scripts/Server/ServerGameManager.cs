namespace ServerManager
{
	using System.Collections;
	using UnityEngine;
	using SDD.Events;
    using System.Collections.Generic;

    public enum GameState { gameMenu, gamePlay, gameLobby, gamePause }

	public class ServerGameManager : ServerManager<ServerGameManager>
	{
		// Attributs

		private Dictionary<ulong, Player> CurrentPlayers;
		private AudioClip CurrentMusic;


		#region Request

		public IReadOnlyDictionary<ulong, Player> GetPlayers()
		{
			return new System.Collections.ObjectModel.ReadOnlyDictionary<ulong, Player>(CurrentPlayers);
		}

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

		#region Events' subscription
		public override void SubscribeEvents()
		{
			base.SubscribeEvents();
			
			//MainMenuManager
			EventManager.Instance.AddListener<EscapeButtonClickedEvent>(EscapeButtonClicked);
			EventManager.Instance.AddListener<PlayButtonClickedEvent>(PlayButtonClicked);
			EventManager.Instance.AddListener<OptionsButtonClickedEvent>(OptionsButtonClicked);
			EventManager.Instance.AddListener<CreditsButtonClickedEvent>(CreditsButtonClicked);
			EventManager.Instance.AddListener<QuitButtonClickedEvent>(QuitButtonClicked);

			EventManager.Instance.AddListener<RoomLeaveButtonClickedEvent>(RoomLeaveButtonClicked);
			EventManager.Instance.AddListener<RoomNextButtonClickedEvent>(RoomNextButtonClicked);

			EventManager.Instance.AddListener<MusicSelectionLeaveButtonClickedEvent>(MusicSelectionLeaveButtonClicked);

			// UI Resize
			EventManager.Instance.AddListener<ResizeUICompleteEvent>(ResizeUIComplete);

			// Network Event
			EventManager.Instance.AddListener<ServerDisconnectionSuccessEvent>(ClientDisconnected);

			// Level Manager
			EventManager.Instance.AddListener<GameReadyEvent>(GameReady);
		}

		public override void UnsubscribeEvents()
		{
			base.UnsubscribeEvents();

			//MainMenuManager
			EventManager.Instance.RemoveListener<EscapeButtonClickedEvent>(EscapeButtonClicked);
			EventManager.Instance.RemoveListener<PlayButtonClickedEvent>(PlayButtonClicked);
			EventManager.Instance.RemoveListener<OptionsButtonClickedEvent>(OptionsButtonClicked);
			EventManager.Instance.RemoveListener<CreditsButtonClickedEvent>(CreditsButtonClicked);
			EventManager.Instance.RemoveListener<QuitButtonClickedEvent>(QuitButtonClicked);

			EventManager.Instance.RemoveListener<RoomLeaveButtonClickedEvent>(RoomLeaveButtonClicked);
			EventManager.Instance.RemoveListener<RoomNextButtonClickedEvent>(RoomNextButtonClicked);

			EventManager.Instance.RemoveListener<MusicSelectionLeaveButtonClickedEvent>(MusicSelectionLeaveButtonClicked);

			// UI Resize
			EventManager.Instance.RemoveListener<ResizeUICompleteEvent>(ResizeUIComplete);

			// Network Event
			EventManager.Instance.RemoveListener<ServerDisconnectionSuccessEvent>(ClientDisconnected);

			// Level Manager
			EventManager.Instance.RemoveListener<GameReadyEvent>(GameReady);
		}
		#endregion

		#region Manager implementation
		protected override IEnumerator InitCoroutine()
		{
			// On demande le redimensionnement des UI.
			EventManager.Instance.Raise(new ResizeUIRequestEvent());

			yield break;
		}
		#endregion

		#region Callbacks to Events issued by MenuManager
		private void ResizeUIComplete(ResizeUICompleteEvent e)
		{
			MainMenu();
		}
		private void PlayButtonClicked(PlayButtonClickedEvent e)
		{
			RoomMenu();
		}

		private void OptionsButtonClicked(OptionsButtonClickedEvent e)
		{
			OptionsMenu();
		}

		private void CreditsButtonClicked(CreditsButtonClickedEvent e)
		{
			CreditsMenu();
		}

		private void RoomLeaveButtonClicked(RoomLeaveButtonClickedEvent e)
		{
			MainMenu();
		}

		private void RoomNextButtonClicked(RoomNextButtonClickedEvent e)
		{
			CurrentPlayers = e.PlayerList;
			MusicSelection();
		}

		private void MusicSelectionLeaveButtonClicked(MusicSelectionLeaveButtonClickedEvent e)
		{
			
		}

		private void EscapeButtonClicked(EscapeButtonClickedEvent e)
		{
			if (GetGameState == GameState.gamePlay) Pause();
		}

		private void QuitButtonClicked(QuitButtonClickedEvent e)
		{
			Application.Quit();
		}
        #endregion

        #region Callbacks to Event issued by LevelManager
		private void GameReady(GameReadyEvent e)
		{
			Play();
		}
        #endregion

        #region Callbacks to Network Event
        private void ClientDisconnected(ServerDisconnectionSuccessEvent e)
		{
			// Si nous somme en jeu et qu'un joueur se déconnecte.
			if (GetGameState == GameState.gamePlay)
			{
				CurrentPlayers[e.ClientID].PlayerState = PlayerState.Disconnected;
			}
		}
        #endregion

        #region GameState methods
        private void MainMenu()
		{
			SetTimeScale(1);
			m_GameState = GameState.gameMenu;
			EventManager.Instance.Raise(new GameMainMenuEvent());
		}

		private void RoomMenu()
		{
			SetTimeScale(1);
			m_GameState = GameState.gameLobby;
			EventManager.Instance.Raise(new GameRoomMenuEvent());
		}

		private void OptionsMenu()
		{
			SetTimeScale(1);
			m_GameState = GameState.gameMenu;
			EventManager.Instance.Raise(new GameOptionsMenuEvent());
		}

		private void CreditsMenu()
		{
			SetTimeScale(1);
			m_GameState = GameState.gameMenu;
			EventManager.Instance.Raise(new GameCreditsMenuEvent());
		}

		private void MusicSelection()
		{
			SetTimeScale(1);
			m_GameState = GameState.gamePlay;
			EventManager.Instance.Raise(new GameMusicSelectionMenuEvent()
			{
				players = GetPlayers()
			});
		}

		private void MusicResult()
		{
			SetTimeScale(1);
			m_GameState = GameState.gamePlay;
			EventManager.Instance.Raise(new GameMusicResultMenuEvent());
		}

		private void Play()
		{
			SetTimeScale(1);
			m_GameState = GameState.gamePlay;
			EventManager.Instance.Raise(new GamePlayEvent(CurrentPlayers, CurrentMusic));
		}

		private void Pause()
		{
			if (GetGameState != GameState.gamePlay) return;

			SetTimeScale(0);
			m_GameState = GameState.gamePause;
			EventManager.Instance.Raise(new GamePauseEvent());
		}

		private void Resume()
		{
			if (GetGameState == GameState.gamePlay) return;

			SetTimeScale(1);
			m_GameState = GameState.gamePlay;
			EventManager.Instance.Raise(new GameResumeEvent());
		}

		#endregion
	}
}