
namespace ServerMaskedManager
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
		[SerializeField] private GameObject m_PanelCredits;
		[SerializeField] private GameObject m_PanelOptions;
		[SerializeField] private GameObject m_PanelInGameMenu;
		[SerializeField] private GameObject m_PanelResult;

		private List<GameObject> m_AllPanels;
		#endregion

		#region Events' subscription
		public override void SubscribeEvents()
		{
			base.SubscribeEvents();

			// UI
			EventManager.Instance.AddListener<ResizeUIRequestEvent>(ResizeUIRequest);
		}

		public override void UnsubscribeEvents()
		{
			base.UnsubscribeEvents();

			// UI
			EventManager.Instance.RemoveListener<ResizeUIRequestEvent>(ResizeUIRequest);
		}
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
			if (Input.GetButtonDown("Cancel"))
			{
				EscapeButtonHasBeenClicked();
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
			m_AllPanels.Add(m_PanelCredits);
			m_AllPanels.Add(m_PanelOptions);
			m_AllPanels.Add(m_PanelInGameMenu);
			m_AllPanels.Add(m_PanelResult);
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

        #region Canvas Methods
		private void ResizeUIRequest(ResizeUIRequestEvent e)
		{
			StartCoroutine("ResizeUI");
		}

		private IEnumerator ResizeUI()
		{
			// On ouvre tout les panels
			OpenAllPanel();

			// On attend une frame le temps de les activer.
			yield return new CoroutineTools.WaitForFrames(1);

			// Envoie un event pour demander aux sous composant de se redimensionner
			EventManager.Instance.Raise(new ResizeUIEvent());

			// On ferme les panels.
			CloseAllPanel();

			// On attend que tous les panels se désactive
			yield return new CoroutineTools.WaitForFrames(1);

			// On indique que la redimension est terminé.
			EventManager.Instance.Raise(new ResizeUICompleteEvent());
		}

        #endregion

        #region UI OnClick Events
        private void EscapeButtonHasBeenClicked()
		{
			EventManager.Instance.Raise(new EscapeButtonClickedEvent());
		}

		public void PlayButtonHasBeenClicked()
		{
			EventManager.Instance.Raise(new PlayButtonClickedEvent());
		}

		public void OptionsButtonHasBeenClicked()
		{
			EventManager.Instance.Raise(new OptionsButtonClickedEvent());
		}

		public void CreditsButtonHasBeenClicked()
		{
			EventManager.Instance.Raise(new CreditsButtonClickedEvent());
		}

		public void QuitButtonHasBeenClicked()
		{
			EventManager.Instance.Raise(new QuitButtonClickedEvent());
		}

		public void RoomLeaveButtonHasBeenClicked()
		{
			EventManager.Instance.Raise(new RoomLeaveButtonClickedEvent());
		}

		public void RoomNextButtonHasBeenClicked()
		{
			EventManager.Instance.Raise(new RoomNextButtonClickedEvent());
		}

		public void MusicSelectionLeaveButtonClicked()
		{
			EventManager.Instance.Raise(new MusicSelectionLeaveButtonClickedEvent());
		}

		public void MusicSelectionNextButtonClicked()
		{
			EventManager.Instance.Raise(new MusicSelectionNextButtonClickedEvent());
		}

		public void MusicResultNextButtonClicked()
		{
			EventManager.Instance.Raise(new MusicResultNextButtonClickedEvent());
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

		protected override void GameCreditsMenu(GameCreditsMenuEvent e)
		{
			base.GameCreditsMenu(e);

			OpenPanel(m_PanelCredits);
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
		#endregion
	}
}
