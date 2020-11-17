namespace ServerMaskedManager
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ServerCameraManager : ServerManager<ServerCameraManager>
    {
        // Constante

        public static readonly float MENU_CAMERA_ORTHOGRAPHIQUE_SIZE = 100;


        // Attributs

        [Header("CameraManager")]

        #region Camera
        [Header("Camera")]

        [SerializeField] private GameObject m_InGameCamera;
        [SerializeField] private GameObject m_MenuCamera;
        [SerializeField] private GameObject m_PauseCamera;

        private List<GameObject> m_AllCamera;
        #endregion

        #region Events sub

        public override void SubscribeEvents()
        {
            base.SubscribeEvents();
        }

        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();
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

            RegisterCamera();
            ConfigCamera();
        }

        #endregion

        #region Camera Methods

        private void RegisterCamera()
        {
            m_AllCamera = new List<GameObject>();

            m_AllCamera.Add(m_InGameCamera);
            m_AllCamera.Add(m_PauseCamera);
            m_AllCamera.Add(m_MenuCamera);
        }

        private void ConfigCamera()
        {
            // Configuration de la caméra menu

            m_MenuCamera.GetComponent<Camera>().orthographicSize = MENU_CAMERA_ORTHOGRAPHIQUE_SIZE;
        }

        private void OpenCamera(GameObject camera)
        {
            foreach (GameObject item in m_AllCamera)
            {
                if (item) item.SetActive(item == camera);
            }
        }

        #endregion

        #region Callbacks to GameManager events

        protected override void GameMainMenu(GameMainMenuEvent e)
        {
            base.GameMainMenu(e);

            OpenCamera(m_MenuCamera);
        }

        protected override void GameRoomMenu(GameRoomMenuEvent e)
        {
            base.GameRoomMenu(e);

            OpenCamera(m_MenuCamera);
        }

        protected override void GameOptionsMenu(GameOptionsMenuEvent e)
        {
            base.GameOptionsMenu(e);

            OpenCamera(m_MenuCamera);
        }

        protected override void GameCreditsMenu(GameCreditsMenuEvent e)
        {
            base.GameCreditsMenu(e);

            OpenCamera(m_MenuCamera);
        }

        protected override void GameMusicSelectionMenu(GameMusicSelectionMenuEvent e)
        {
            base.GameMusicSelectionMenu(e);

            OpenCamera(m_MenuCamera);
        }

        protected override void GameMusicResultMenu(GameMusicResultMenuEvent e)
        {
            base.GameMusicResultMenu(e);

            OpenCamera(m_MenuCamera);
        }

        protected override void GamePlay(GamePlayEvent e)
        {
            base.GamePlay(e);

            OpenCamera(m_InGameCamera);
        }
        #endregion
    }

}
