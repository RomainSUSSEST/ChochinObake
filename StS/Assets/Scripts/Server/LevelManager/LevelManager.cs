

namespace ServerManager
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class LevelManager : ServerManager<LevelManager>
    {
        // Constante

        private static readonly int MIN_NUMBER_WAVES = 5;


        // Attributs

        private System.Collections.ObjectModel.ReadOnlyCollection<SpectralFluxInfo> CurrentMap;


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

            // On récupére les informations de la carte.
            CurrentMap = ServerGameManager.Instance.GetCurrentMapData();
            AudioClip clip = ServerGameManager.Instance.GetCurrentAudioClip();

            if (CurrentMap == null || clip == null)
            {
                throw new System.Exception("Donnée de carte invalide");
            }

            // Initialisation des obstacles
            Obstacle.SetCurrentMoveSpeed((CurrentMap.Count / clip.length) * Obstacle.DEFAULT_SPEED);
            //InitializationMap();
        }

        #endregion


        // Outils

    }
}
