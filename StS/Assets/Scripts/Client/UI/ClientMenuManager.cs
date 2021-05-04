namespace ClientManager
{
    using SDD.Events;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ClientMenuManager : ClientManager<ClientMenuManager>
    {
        [Header("MenuManager")]

        #region Canvas

        [Header("Canvas")]

        [SerializeField] private GameObject m_MainCanvas;
        #endregion

        #region Panels
        [Header("Panels")]

        [SerializeField] private GameObject m_PanelMainMenu;
        [SerializeField] private GameObject m_PanelJoinRoom;
        [SerializeField] private GameObject m_PanelCharacterSelection;
        [SerializeField] private GameObject m_PanelMusicSelection;
        [SerializeField] private GameObject m_PanelMusicResult;
        [SerializeField] private GameObject m_PanelInGame;

        private List<GameObject> m_AllPanels;
        #endregion

        #region Manager Implementation

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
        #endregion

        #region PanelMethods

        private void RegisterPanels()
        {
            m_AllPanels = new List<GameObject>();

            m_AllPanels.Add(m_PanelMainMenu);
            m_AllPanels.Add(m_PanelJoinRoom);
            m_AllPanels.Add(m_PanelCharacterSelection);
            m_AllPanels.Add(m_PanelMusicSelection);
            m_AllPanels.Add(m_PanelMusicResult);
            m_AllPanels.Add(m_PanelInGame);
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

        #region UI OnclickEvents
        public void JoinButtonHasBeenClicked()
        {
            EventManager.Instance.Raise(new JoinButtonClickedEvent());
        }

        public void LeaveButtonHasBeenClicked()
        {
            EventManager.Instance.Raise(new LeaveButtonClickedEvent());
        }

        public void PreviousCharacterSelectionButtonHasBeenClicked()
        {
            EventManager.Instance.Raise(new PreviousCharacterSelectionButtonClickedEvent());
        }

        public void ReadyCharacterSelectionButtonHasBeenClicked()
        {
            EventManager.Instance.Raise(new ReadyCharacterSelectionButtonClickedEvent());
        }

        public void ExitButtonHasBeenClicked()
        {
            EventManager.Instance.Raise(new ExitButtonClickedEvent());
        }

        #endregion

        #region CallBacks to GameManager Events

        protected override void MobileMainMenu(MobileMainMenuEvent e)
        {
            base.MobileMainMenu(e);

            OpenPanel(m_PanelMainMenu);
        }

        protected override void MobileJoinRoom(MobileJoinRoomEvent e)
        {
            base.MobileJoinRoom(e);

            OpenPanel(m_PanelJoinRoom);
        }

        protected override void MobileCharacterSelection(MobileCharacterSelectionEvent e)
        {
            base.MobileCharacterSelection(e);

            OpenPanel(m_PanelCharacterSelection);
        }

        protected override void MobileMusicSelection(MobileMusicSelectionEvent e)
        {
            base.MobileMusicSelection(e);

            OpenPanel(m_PanelMusicSelection);
        }

        protected override void MobileMusicResult(MobileMusicResultEvent e)
        {
            base.MobileMusicResult(e);

            OpenPanel(m_PanelMusicResult);
        }

        protected override void MobileGamePlay(MobileGamePlayEvent e)
        {
            base.MobileGamePlay(e);

            OpenPanel(m_PanelInGame);
        }

        #endregion


        // Outils

        public class WaitForFrames : CustomYieldInstruction
        {
            private int _targetFrameCount;

            public WaitForFrames(int numberOfFrames)
            {
                _targetFrameCount = Time.frameCount + numberOfFrames;
            }

            public override bool keepWaiting
            {
                get
                {
                    return Time.frameCount < _targetFrameCount;
                }
            }
        }
    }

}

