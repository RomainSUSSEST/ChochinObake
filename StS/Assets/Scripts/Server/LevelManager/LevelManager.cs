

namespace ServerManager
{
    using DSPLib;
    using SDD.Events;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using CommonVisibleManager;

    public class LevelManager : ServerManager<LevelManager>
    {
        // Constante

        private static readonly float AdaptSensitivity = 0.2f; // en %
        private static readonly float MaxDistanceLevel = 62.5f; // Distance maximal de la carte en longueur


        // Attributs

        [Header("WorldForestAssets")]

        [SerializeField] private Obstacle GroundForest;
        [SerializeField] private List<Obstacle> ObstacleForest;

        [Header("LevelManager")]

        // TO DO : CHANGER LA MANIERE DE FAIRE
        [SerializeField] private AudioSource CurrentAudioSource;

        [SerializeField] private SlimeServer SlimeServer;
        [SerializeField] private GameObject SpawnerSlime;
        [SerializeField] private GameObject SpawnerMap;

        private int NumChannels;
        private int NumTotalSamples;
        private int SampleRate;
        private float[] MultiChannelSamples;
        private SpectralFluxAnalyzer PreProcessedSpectralFluxAnalyzer;

        private float MaximumBeatsPrunedSpectralFlux;
        private float MoyenneBeatsPrunedSpectralFlux;

        private System.Random CurRandom;

        private float ThresholdOfObstacleSpawn; // Sensibilité de l'algorithme. Plus c'est petit, plus il y a d'obstacle. (min : 0, max : MaximumBeatsPrunedSpectralFlux)

        private List<SpectralFluxInfo> RealBeats;

        // Parti en jeu

        private AudioClip CurrentClip;

        private Obstacle LastObstacle;
        private Obstacle NextObstacle;


        // Requetes

        public float GetTimeFromIndex(int index)
        {
            return ((1f / (float)this.SampleRate) * index);
        }

        public void GetFullSpectrumThreaded()
        {
            try
            {
                // We only need to retain the samples for combined channels over the time domain
                float[] preProcessedSamples = new float[this.NumTotalSamples];

                int numProcessed = 0;
                float combinedChannelAverage = 0f;
                for (int i = 0; i < MultiChannelSamples.Length; i++)
                {
                    combinedChannelAverage += MultiChannelSamples[i];

                    // Each time we have processed all channels samples for a point in time, we will store the average of the channels combined
                    if ((i + 1) % this.NumChannels == 0)
                    {
                        preProcessedSamples[numProcessed] = combinedChannelAverage / this.NumChannels;
                        numProcessed++;
                        combinedChannelAverage = 0f;
                    }
                }

                // Once we have our audio sample data prepared, we can execute an FFT to return the spectrum data over the time domain
                int spectrumSampleSize = 1024;
                int iterations = preProcessedSamples.Length / spectrumSampleSize;

                FFT fft = new FFT();
                fft.Initialize((UInt32)spectrumSampleSize);

                double[] sampleChunk = new double[spectrumSampleSize];
                for (int i = 0; i < iterations; i++)
                {
                    // Grab the current 1024 chunk of audio sample data
                    Array.Copy(preProcessedSamples, i * spectrumSampleSize, sampleChunk, 0, spectrumSampleSize);

                    // Apply our chosen FFT Window
                    double[] windowCoefs = DSP.Window.Coefficients(DSP.Window.Type.Hanning, (uint)spectrumSampleSize);
                    double[] scaledSpectrumChunk = DSP.Math.Multiply(sampleChunk, windowCoefs);
                    double scaleFactor = DSP.Window.ScaleFactor.Signal(windowCoefs);

                    // Perform the FFT and convert output (complex numbers) to Magnitude
                    System.Numerics.Complex[] fftSpectrum = fft.Execute(scaledSpectrumChunk);
                    double[] scaledFFTSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(fftSpectrum);
                    scaledFFTSpectrum = DSP.Math.Multiply(scaledFFTSpectrum, scaleFactor);

                    // These 1024 magnitude values correspond (roughly) to a single point in the audio timeline
                    float curSongTime = GetTimeFromIndex(i) * spectrumSampleSize;

                    // Send our magnitude data off to our Spectral Flux Analyzer to be analyzed for peaks
                    PreProcessedSpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime);
                }

            }
            catch (Exception e)
            {
                // Catch exceptions here since the background thread won't always surface the exception to the main thread
                Debug.Log(e.ToString());
            }
        }


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

            // Obstacle

            EventManager.Instance.AddListener<ObstacleEndMapEvent>(InstantiateObstacle);
        }

        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();

            // Obstacle

            EventManager.Instance.RemoveListener<ObstacleEndMapEvent>(InstantiateObstacle);
        }


        // Event Call Back

        #region GameManager Events

        protected override void GamePlay(GamePlayEvent e)
        {
            base.GamePlay(e);

            // Initialisation des obstacles
            Obstacle.SetCurrentMoveSpeed(RealBeats.Count / CurrentClip.length * Obstacle.DEFAULT_SPEED);
            Obstacle.SetMapStart(SpawnerMap.transform);

            // On initialise la carte

            float totalSize = 0;
            while (totalSize < MaxDistanceLevel)
            {

                LastObstacle = Instantiate(GroundForest,
                    new Vector3(SpawnerMap.transform.position.x, SpawnerMap.transform.position.y, SpawnerMap.transform.position.z + totalSize),
                    Quaternion.identity);

                totalSize += LastObstacle.GetComponent<BoxCollider>().bounds.size.z; ;
            }

            //InstantiatePlayers(e.GetPlayers());

            StartCoroutine("ProceduralGenerator");
        }

        #endregion


        // Outils

        /**
		 * @pre beats.prunedSpectralFlux > ThresholdOfObstacleSpawn &&
		 *	Sum(ListObstacleToSpawn[i].GetComponent<Obstacle>().GetNbrCase_X() < NBR_WAVE
		 * 
		 * @post ObstacleToSpawn.Count == old ObstacleToSpawn.Count + 1
		 *		 TotalSizeListObtacleToSpawn == old TotalSizeListObstacleToSpawn + Obstacle[index].GetComponent<Obstacle>().GetNbrCase_X()
		 *		 
		 * @send CanNotAddObstacle si l'obstacle ne rentre pas dans la place restante sur la ligne en cours.
		 */
        private void AddObstacle(SpectralFluxInfo beats)
        {
            // Si le beats n'est pas ignoré
            if (beats.prunedSpectralFlux > ThresholdOfObstacleSpawn)
            {
                SfxManager.Instance.PlaySfx2D("BalloonPop");
                // On cherche à combien de pourcentage du maximum correspond le beats.
                float curPercentOfMax = 100 * beats.prunedSpectralFlux / MaximumBeatsPrunedSpectralFlux;

                // On prend l'index en fonction du pourcentage du maximum du beats
                int index = (int)Mathf.Floor(curPercentOfMax % ObstacleForest.Count);
                NextObstacle = ObstacleForest[index];
            }
        }


        /**
		 * @post : NbrObstacleSpawn.Clear()
		 *		   TotalSizeListObstacleToSpawn == 0
		 *		   Update PreviousLine
		 */
        private void InstantiateObstacle(ObstacleEndMapEvent e)
        {
            if (NextObstacle == null)
            {
                NextObstacle = GroundForest;
            }

            LastObstacle = Instantiate(NextObstacle,
                new Vector3(LastObstacle.transform.position.x,
                    LastObstacle.transform.position.y,
                    LastObstacle.transform.position.z + LastObstacle.GetComponent<BoxCollider>().bounds.size.z),
                Quaternion.identity);

            NextObstacle = null;
        }

        private void InstantiatePlayers(Dictionary<ulong, Player> players)
        {
            Dictionary<ulong, Player>.KeyCollection keys = players.Keys;

            foreach (ulong k in keys)
            {
                SlimeServer curSlime = Instantiate(SlimeServer, SpawnerSlime.transform.position, SlimeServer.transform.rotation);

                curSlime.SetHat(players[k].Hat);
                curSlime.SetBody(players[k].Body);
                curSlime.AssociedClientID = k;
            }
        }


        // Coroutine

        private IEnumerator ProceduralGenerator()
        {
            int cpt = 0;
            while (cpt < RealBeats.Count)
            {
                if (RealBeats[cpt].time <= CurrentAudioSource.time && cpt < RealBeats.Count)
                {
                    AddObstacle(RealBeats[cpt]);

                    ++cpt;
                }

                yield return new CoroutineTools.WaitForFrames(1);
            }
        }
    }
}
