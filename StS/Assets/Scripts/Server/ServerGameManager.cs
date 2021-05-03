namespace ServerManager
{
	using System.Collections;
	using UnityEngine;
	using SDD.Events;
    using System.Collections.Generic;

    public enum GameState { gameMenu, gameMapSelect, gamePlay, gameLobby, gamePause }
	
	public class ServerGameManager : ServerManager<ServerGameManager>
	{
		// Attributs

		private Dictionary<ulong, Player> CurrentPlayers; // Les joueurs
		private List<AI_Player> CurrentAI;
		private string CurrentMusicPath; // Le path de la musique
		private AudioClip CurrentAudio; // La musique courante
		private List<SpectralFluxInfo> CurrentMapData; // Les données de carte
		private float CurrentDifficulty; // La difficulté actuelle en % de 0 à 1


		#region Request

		public IReadOnlyDictionary<ulong, Player> GetPlayers()
		{
			if (CurrentPlayers == null)
			{
				return null;
			}

			return new System.Collections.ObjectModel.ReadOnlyDictionary<ulong, Player>(CurrentPlayers);
		}

		public IReadOnlyList<AI_Player> GetAIList()
		{
			if (CurrentAI == null)
			{
				return null;
			}

			return new System.Collections.ObjectModel.ReadOnlyCollection<AI_Player>(CurrentAI);
		}

		/// <summary>
		/// Renvoie le chemin de la musique courante
		/// </summary>
		/// <returns> Le path </returns>
		public string GetCurrentMusicPath()
		{
			return CurrentMusicPath;
		}

		/// <summary>
		/// Renvoie l'AudioClip courant
		/// </summary>
		/// <returns> L'AudioClip </returns>
		public AudioClip GetCurrentAudioClip()
		{
			return CurrentAudio;
		}

		/// <summary>
		/// Renvoie la difficulté courante
		/// </summary>
		/// <returns> La difficulté de 0 à 1 (0 à 100%) </returns>
		public float GetCurrentDifficulty()
		{
			return CurrentDifficulty;
		}

		public System.Collections.ObjectModel.ReadOnlyCollection<SpectralFluxInfo> GetCurrentMapData()
		{
			if (CurrentMapData == null)
			{
				return null;
			}

			return CurrentMapData.AsReadOnly();
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
			EventManager.Instance.AddListener<PlayButtonClickedEvent>(PlayButtonClicked);
			EventManager.Instance.AddListener<OptionsButtonClickedEvent>(OptionsButtonClicked);
			EventManager.Instance.AddListener<QuitButtonClickedEvent>(QuitButtonClicked);

			EventManager.Instance.AddListener<RoomLeaveButtonClickedEvent>(RoomLeaveButtonClicked);
			EventManager.Instance.AddListener<RoomNextButtonClickedEvent>(RoomNextButtonClicked);

			EventManager.Instance.AddListener<MusicSelectionLeaveButtonClickedEvent>(MusicSelectionLeaveButtonClicked);
			EventManager.Instance.AddListener<MusicSelectionTimerEndEvent>(MusicSelectionTimerEnd);

			EventManager.Instance.AddListener<MusicResultGameReadyEvent>(MusicResultGameReady);

			EventManager.Instance.AddListener<ViewResultEndEvent>(ViewResultEnd);

			EventManager.Instance.AddListener<EscapeButtonHasBeenPressedEvent>(EscapeButtonHasBeenPressed);

			EventManager.Instance.AddListener<ContinueButtonClickedEvent>(ContinueButtonClicked);
			EventManager.Instance.AddListener<LeavePausePanelButtonClickedEvent>(LeavePausePanelButtonClicked);

			// ServerLevelManager

			EventManager.Instance.AddListener<ScoreUpdatedEvent>(ScoreUpdated);

			// Network Event
			EventManager.Instance.AddListener<ServerDisconnectionSuccessEvent>(ClientDisconnected);
		}

		public override void UnsubscribeEvents()
		{
			base.UnsubscribeEvents();

			//MainMenuManager
			EventManager.Instance.RemoveListener<PlayButtonClickedEvent>(PlayButtonClicked);
			EventManager.Instance.RemoveListener<OptionsButtonClickedEvent>(OptionsButtonClicked);
			EventManager.Instance.RemoveListener<QuitButtonClickedEvent>(QuitButtonClicked);

			EventManager.Instance.RemoveListener<RoomLeaveButtonClickedEvent>(RoomLeaveButtonClicked);
			EventManager.Instance.RemoveListener<RoomNextButtonClickedEvent>(RoomNextButtonClicked);

			EventManager.Instance.RemoveListener<MusicSelectionLeaveButtonClickedEvent>(MusicSelectionLeaveButtonClicked);
			EventManager.Instance.RemoveListener<MusicSelectionTimerEndEvent>(MusicSelectionTimerEnd);

			EventManager.Instance.RemoveListener<MusicResultGameReadyEvent>(MusicResultGameReady);

			EventManager.Instance.RemoveListener<ViewResultEndEvent>(ViewResultEnd);

			EventManager.Instance.RemoveListener<EscapeButtonHasBeenPressedEvent>(EscapeButtonHasBeenPressed);

			EventManager.Instance.RemoveListener<ContinueButtonClickedEvent>(ContinueButtonClicked);
			EventManager.Instance.RemoveListener<LeavePausePanelButtonClickedEvent>(LeavePausePanelButtonClicked);

			// ServerLevelManager

			EventManager.Instance.RemoveListener<ScoreUpdatedEvent>(ScoreUpdated);

			// Network Event
			EventManager.Instance.RemoveListener<ServerDisconnectionSuccessEvent>(ClientDisconnected);
		}
		#endregion

		#region Manager implementation

		protected override IEnumerator Start()
		{
			yield return base.Start();

			// On attend que tous les managers soit pret
			while (!ManagersStates.AllManagersReady())
				yield return new CoroutineTools.WaitForFrames(1);

			// On lance le menu
			MainMenu();
		}

		protected override IEnumerator InitCoroutine()
		{
			// Gérer la qualité

			yield break;
		}
		#endregion

		#region Callbacks to Events issued by MenuManager

		private void PlayButtonClicked(PlayButtonClickedEvent e)
		{
			// On s'assure que les joueurs sont reset
			CurrentAI = null;
			CurrentPlayers = null;
			CurrentMusicPath = null;
			CurrentAudio = null;
			CurrentMapData = null;
		
			RoomMenu();
		}

		private void OptionsButtonClicked(OptionsButtonClickedEvent e)
		{
			OptionsMenu();
		}

		private void RoomLeaveButtonClicked(RoomLeaveButtonClickedEvent e)
		{
			MainMenu();
		}

		private void RoomNextButtonClicked(RoomNextButtonClickedEvent e)
		{

			CurrentPlayers = e.PlayerList;
			CurrentAI = e.AI;
			MusicSelection();
		}

		private void MusicSelectionLeaveButtonClicked(MusicSelectionLeaveButtonClickedEvent e)
		{
			RoomMenu();
		}

		private void MusicSelectionTimerEnd(MusicSelectionTimerEndEvent e)
		{
			// On set la musique choisi
			CurrentMusicPath = e.PathDirectoryMusicSelected;
			MusicResult();
		}

		private void MusicResultGameReady(MusicResultGameReadyEvent e)
		{
			// On enregistre les données de carte
			CurrentAudio = e.audio;
			CurrentMapData = e.map;
			CurrentDifficulty = e.difficulty;

			Play();
		}

		private void ViewResultEnd(ViewResultEndEvent e)
		{
			RoomMenu();
		}

		private void QuitButtonClicked(QuitButtonClickedEvent e)
		{
			Application.Quit();
		}

		private void EscapeButtonHasBeenPressed(EscapeButtonHasBeenPressedEvent e)
		{
			if (m_GameState == GameState.gamePlay)
			{
				Pause();
			}
		}

		private void ContinueButtonClicked(ContinueButtonClickedEvent e)
		{
			Pause();
		}

		private void LeavePausePanelButtonClicked(LeavePausePanelButtonClickedEvent e)
		{
			RoomMenu();
		}

        #endregion

        #region Callbacks to Event issued by ServerLevelManager

		private void ScoreUpdated(ScoreUpdatedEvent e)
		{
			ResultMenu();
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

		private void MusicSelection()
		{
			SetTimeScale(1);
			m_GameState = GameState.gameMapSelect;
			EventManager.Instance.Raise(new GameMusicSelectionMenuEvent());
		}

		private void MusicResult()
		{
			SetTimeScale(1);
			m_GameState = GameState.gameMapSelect;
			EventManager.Instance.Raise(new GameMusicResultMenuEvent());
		}

		private void Play()
		{
			SetTimeScale(1);
			m_GameState = GameState.gamePlay;
			EventManager.Instance.Raise(new GamePlayEvent());
		}

		private void ResultMenu()
		{
			SetTimeScale(1);
			m_GameState = GameState.gameMenu;
			EventManager.Instance.Raise(new GameResultEvent());
		}

		private void Pause()
		{
			if (m_GameState == GameState.gamePause)
			{
				m_GameState = GameState.gamePlay;
				EventManager.Instance.Raise(new GameContinueEvent());
				SetTimeScale(1);
			} else
			{
				m_GameState = GameState.gamePause;
				EventManager.Instance.Raise(new GamePauseEvent());
				SetTimeScale(0);
			}
		}

		#endregion
	}
}