namespace ServerManager
{
    using System.Collections;
    using UnityEngine;

    public class ServerLevelManager : ServerManager<ServerLevelManager>
    {
        // Constante

        public static readonly int MIN_NUMBER_WAVES = 5;


        // Attributs

        [SerializeField] private WorldForest WorldForest;

        private GameObject CurrentWorld;


        // Requetes


        // Méthode

        #region Manager implementation
        protected override IEnumerator InitCoroutine()
        {
            yield break;
        }
        #endregion

        public override void SubscribeEvents()
        {
            base.SubscribeEvents();
        }

        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();
        }


        // Event Call Back

        #region GameManager Events

        protected override void GamePlay(GamePlayEvent e)
        {
            base.GamePlay(e);

            CurrentWorld = Instantiate(WorldForest.gameObject);
        }

        #endregion


        // Outils

    }
}
