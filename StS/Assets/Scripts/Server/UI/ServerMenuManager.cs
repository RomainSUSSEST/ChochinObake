
namespace ServerManager
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using SDD.Events;

	public class ServerMenuManager : ServerManager<ServerMenuManager>
	{
        #region Panels
        [Header("Panels")]
		[SerializeField] private GameObject m_PanelMainMenu;
		[SerializeField] private GameObject m_PanelRoom;
		[SerializeField] private GameObject m_PanelMusicSelection;
		[SerializeField] private GameObject m_PanelMusicResult;
		[SerializeField] private GameObject m_PanelOptions;
		[SerializeField] private GameObject m_PanelResult;
		[SerializeField] private GameObject m_PanelPause;

		private List<GameObject> m_AllPanels;
		#endregion

		#region Manager implementation
		protected override IEnumerator InitCoroutine()
		{
			yield break;
		}
		#endregion

		#region Monobehaviour lifecycle
		protected override void Awake()
		{
			base.Awake();
			RegisterPanels();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				EventManager.Instance.Raise(new EscapeButtonHasBeenPressedEvent());
			}
		}

		#endregion

		#region Panel Methods
		private void RegisterPanels()
		{
			m_AllPanels = new List<GameObject>();

			m_AllPanels.Add(m_PanelMainMenu);
			m_AllPanels.Add(m_PanelRoom);
			m_AllPanels.Add(m_PanelMusicSelection);
			m_AllPanels.Add(m_PanelMusicResult);
			m_AllPanels.Add(m_PanelOptions);
			m_AllPanels.Add(m_PanelResult);
			m_AllPanels.Add(m_PanelPause);
	}

		private void OpenPanel(GameObject panel)
		{
			foreach (var item in m_AllPanels)
				if (item) item.SetActive(item == panel);
		}

		private void OpenAllPanel()
		{
			foreach (var item in m_AllPanels)
			{
				item.SetActive(true);
			}
		}

		private void CloseAllPanel()
		{
			OpenPanel(null);
		}
        #endregion

        #region UI OnClick Events

		public void PlayButtonHasBeenClicked()
		{
			EventManager.Instance.Raise(new PlayButtonClickedEvent());
		}

		public void OptionsButtonHasBeenClicked()
		{
			EventManager.Instance.Raise(new OptionsButtonClickedEvent());
		}

		public void QuitButtonHasBeenClicked()
		{
			EventManager.Instance.Raise(new QuitButtonClickedEvent());
		}

		public void RoomLeaveButtonHasBeenClicked()
		{
			EventManager.Instance.Raise(new RoomLeaveButtonClickedEvent());
		}

		public void OptionsLeaveButtonHasBeenClicked()
		{
			EventManager.Instance.Raise(new RoomLeaveButtonClickedEvent());
		}

		public void MusicSelectionLeaveButtonClicked()
		{
			EventManager.Instance.Raise(new MusicSelectionLeaveButtonClickedEvent());
		}

		public void ContinueButtonClicked()
		{
			EventManager.Instance.Raise(new ContinueButtonClickedEvent());
		}

		public void LeavePausePanelButtonClicked()
		{
			EventManager.Instance.Raise(new LeavePausePanelButtonClickedEvent());
		}

		#endregion

		#region Callbacks to GameManager events

		protected override void GameMainMenu(GameMainMenuEvent e)
		{
			base.GameMainMenu(e);

			OpenPanel(m_PanelMainMenu);
		}

		protected override void GameRoomMenu(GameRoomMenuEvent e)
		{
			base.GameRoomMenu(e);

			OpenPanel(m_PanelRoom);
		}

		protected override void GameOptionsMenu(GameOptionsMenuEvent e)
		{
			base.GameOptionsMenu(e);

			OpenPanel(m_PanelOptions);
		}
		
		protected override void GameMusicSelectionMenu(GameMusicSelectionMenuEvent e)
		{
			base.GameMusicSelectionMenu(e);

			OpenPanel(m_PanelMusicSelection);
		}

		protected override void GameMusicResultMenu(GameMusicResultMenuEvent e)
		{
			base.GameMusicResultMenu(e);

			OpenPanel(m_PanelMusicResult);
		}

		protected override void GamePlay(GamePlayEvent e)
		{
			base.GamePlay(e);

			CloseAllPanel();
		}

		protected override void GameResult(GameResultEvent e)
		{
			base.GameResult(e);

			OpenPanel(m_PanelResult);
		}

		protected override void GameContinue(GameContinueEvent e)
		{
			base.GameContinue(e);

			CloseAllPanel();
		}

		protected override void GamePause(GamePauseEvent e)
		{
			base.GamePause(e);

			OpenPanel(m_PanelPause);
		}
		#endregion
	}
}
