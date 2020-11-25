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
        [SerializeField] private GameObject m_PanelChooseMusic;
        [SerializeField] private GameObject m_PanelInGame;

        private List<GameObject> m_AllPanels;
        #endregion

        #region Events Subscription

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
            m_AllPanels.Add(m_PanelChooseMusic);
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

        #region Canvas Methods

        private void ResizeUIRequest(ResizeUIRequestEvent e)
        {
            StartCoroutine("ResizeUI");
        }

        private IEnumerator ResizeUI()
        {
            // redimensionne les canvas selon la dimension de l'écran. ---
            RectTransform rectTransform = m_MainCanvas.GetComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);

            // Reduction des canvas pour la taille des cameras ---

            m_MainCanvas.GetComponent<RectTransform>();
            // calcul du nouveau scale
            float scale = ClientCameraManager.MENU_CAMERA_ORTHOGRAPHIQUE_SIZE * 2 / Screen.height;
            rectTransform.localScale = new Vector3(scale, scale, scale);

            // On ouvre tout les panels
            OpenAllPanel();

            // On attend une frame le temps de les activer
            yield return new WaitForFrames(1);

            // Envoie un event pour demander aux sous composant de se redimensionner
            EventManager.Instance.Raise(new ResizeUIEvent());

            // On ferme les panels.
            CloseAllPanel();

            // On attend que tous les panels se désactive
            yield return new WaitForFrames(1);

            // On indiquee que la redimension est terminé.
            EventManager.Instance.Raise(new ResizeUICompleteEvent());
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

        public void PreviousMusicSelectionButtonHasBeenClicked()
        {
            EventManager.Instance.Raise(new PreviousMusicSelectionButtonClickedEvent());
        }

        public void RespawnButtonClicked()
        {
            EventManager.Instance.Raise(new RespawnButtonClickedEvent());
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

        protected override void MobileChooseMusic(MobileChooseMusicEvent e)
        {
            base.MobileChooseMusic(e);
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

