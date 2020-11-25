namespace ClientManager
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ClientCameraManager : ClientManager<ClientCameraManager>
    {
        // Constante

        public static readonly float MENU_CAMERA_ORTHOGRAPHIQUE_SIZE = 100;


        // Attributs

        [Header("CameraManager")]

        #region Camera
        [Header("Camera")]

        [SerializeField] private GameObject m_MainCamera;

        private List<GameObject> m_AllCamera;
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

            m_AllCamera.Add(m_MainCamera);
        }

        private void ConfigCamera()
        {
            // Configuration de la MainCamera

            m_MainCamera.GetComponent<Camera>().orthographicSize = MENU_CAMERA_ORTHOGRAPHIQUE_SIZE;
        }

        private void OpenCamera(GameObject camera)
        {
            foreach (var item in m_AllCamera)
            {
                if (item) item.SetActive(item == camera);
            }
        }
        #endregion

        #region Callbacks to GameManager events

        protected override void MobileMainMenu(MobileMainMenuEvent e)
        {
            base.MobileMainMenu(e);

            OpenCamera(m_MainCamera);
        }

        protected override void MobileJoinRoom(MobileJoinRoomEvent e)
        {
            base.MobileJoinRoom(e);

            OpenCamera(m_MainCamera);
        }

        protected override void MobileCharacterSelection(MobileCharacterSelectionEvent e)
        {
            base.MobileCharacterSelection(e);

            OpenCamera(m_MainCamera);
        }

        protected override void MobileMusicSelection(MobileMusicSelectionEvent e)
        {
            base.MobileMusicSelection(e);

            OpenCamera(m_MainCamera);
        }

        protected override void MobileChooseMusic(MobileChooseMusicEvent e)
        {
            base.MobileChooseMusic(e);

            OpenCamera(m_MainCamera);
        }

        #endregion
    }

}

