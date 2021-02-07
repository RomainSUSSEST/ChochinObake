namespace ServerManager
{
    using System.Collections;
    using UnityEngine;

    public class ServerLevelManager : ServerManager<ServerLevelManager>
    {
        // Constante

        public static readonly int MIN_NUMBER_WAVES = 12;

        public static readonly float DEFAULT_SPEED = 8f; // Vitesse par défaut
        public static readonly float MIN_SPEED = 17f;
        public static readonly float MAX_SPEED = 24f;

        public static readonly float ALGO_MIN_SENSITIVITY = 0.2f; // en % (gestion de la difficulté) 0 = D / 0.1 = M / 0.2 = F
        public static readonly float ALGO_MAX_SENSITIVITY = 0f;

        public static readonly float DISTANCE_BETWEEN_LINE = 4f;

        public static readonly float NBR_LINE = 4; // Nombre de ligne immatérielle composant le jeu.
        public static readonly float LINE_SLIME_SPAWN = 3f / 4f; // Multiplicateur indiquant à quelle position initialement placé les joueurs par rapport aux nombres de lignes.


        // Attributs

        [SerializeField] private World WorldJapan;

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

            CurrentWorld = Instantiate(WorldJapan.gameObject);
        }

        #endregion


        // Outils

    }
}
