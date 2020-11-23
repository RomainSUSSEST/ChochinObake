using DSPLib;
using SDD.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace ServerVisibleManager
{
    /// <summary>
    /// Classe permettant de stocker les musiques, de les importer, ou de les exporter.
    /// </summary>
    public class ServerAccountManager : ServerManager<ServerAccountManager>
    {
        // State

        private enum STATE { Nothing, AddingSong, LoadingSong }

        // Constante

        private static readonly string WEBM_EXTENSION = ".webm";
        private static readonly string WAV_EXTENSION = ".wav";

        private static readonly string DEFAULT_NAME = "Audio";
        private static readonly string DATA_FILE = "Data";


        // Attributs

        private STATE ServerAccountManager_State;
        private AudioClip DownloadedSongAtAudioClip;


        #region Manager implementation
        protected override IEnumerator InitCoroutine()
        {
            ServerAccountManager_State = STATE.Nothing;
            yield break;
        }
        #endregion


        // Requetes

        /// <summary>
        /// Renvoi la liste des chansons actuellement enregistré.
        /// </summary>
        /// <returns> Renvoi la liste des chansons actuellement enregistré dans "Application.persistentDataPath" </returns>
        public string[] GetSongList()
        {
            return Directory.GetDirectories(Application.persistentDataPath);
        }


        // Méthodes

        /// <summary>
        /// Ajoute le son référencé par l'Url Youtube à la bibliothéque du jeu.
        /// Durant l'execution, ServerAccountManager_State = STATE.AddingSong
        /// 
        /// Une fois terminé, envoie un event PrepareSongEndEvent() et ServerAccountManager_State = STATE.Nothing.
        /// </summary>
        /// <param name="Url"> L'Url du lien youtube à ajouter </param>
        public async void AddYoutubeSongAsync(string Url)
        {
            ServerAccountManager_State = STATE.AddingSong;

            try
            {
                string WebmSongPath = await ExtractYoutubeAudioWebmAsync(Url); // On récupére l'audio de Youtube au format .Webm
                string Directory = Path.GetDirectoryName(WebmSongPath);
                string WavPath = await ConvertWebmToWavAsync(WebmSongPath, Directory); // On converti le fichier .Webm double en .Wav
                AudioClip clip = await LoadWavAudioFromAsync(WavPath);
                List<SpectralFluxInfo> realBeats = await AnalyzeAudio(clip);
                await SaveMapAsync(realBeats, Directory);
                DeleteTemporaryFile(Directory);
            } catch (Exception e)
            {
                AnErrorOccurred(e.Message);
                return;
            }

            // On averti que l'opération est terminé
            EventManager.Instance.Raise(new PrepareSongEndEvent());
            // On change l'état du manager en conséquence.
            ServerAccountManager_State = STATE.Nothing;
        }

        /// <summary>
        /// Delete les données de l'audio contenu dans le directory pointé par path.
        /// </summary>
        /// <param name="path"> Le directory contenant les données de l'audio </param>
        public void RemoveSongWithDirectoryPath(string path)
        {
            Directory.Delete(path, true);
        }


        // Outils

        #region AddingSong

        #region async function

        /// <summary>
        /// 
        /// Extract the youtube audio of the url and return the path of the created file at .Webm format with async function.
        /// Save at default repository with Unity in the folder with name of youtube video title (Application.persistentDataPath).
        /// 
        /// </summary>
        /// <returns> The path of the file created in .Webm with (cons) DEFAULT_NAME </returns>
        private async Task<string> ExtractYoutubeAudioWebmAsync(string Url)
        {
            // Preparation
            UpdatePrepareSongState("Initialization", 0);

            // On initialise le youtubeclient
            YoutubeClient youtube = new YoutubeClient();

            // On récupére les Metadatas

            UpdatePrepareSongState("Get MetaData", 2);
            YoutubeExplode.Videos.Video video = await youtube.Videos.GetAsync(Url);

            // On récupére les différents channels de qualité

            UpdatePrepareSongState("Search for the best quality", 4);
            StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(Url);
          
            // On sélectionne la plus haute qualité audio
            IStreamInfo streamInfo = streamManifest.GetAudioOnly().WithHighestBitrate();

            // On crée le répertoire stockant les données

            string newFolder = video.Title.Replace(Path.GetInvalidPathChars().ToString(), ""); // Nouveau dossier à créer
            if (newFolder == "") // Si le nom ne contient que des characteres non lisible
            {
                throw new Exception("The video's name is unreadable");
            }

            string directory = Application.persistentDataPath + "/" + newFolder;

            // On regarde si le répertoire n'existe pas déjà.
            if (Directory.Exists(directory))
            {
                throw new Exception("The song is already added !");
            }

            Directory.CreateDirectory(directory); // Création du répertoire

            string resultPath = directory + "/" + DEFAULT_NAME + WEBM_EXTENSION;

            // Lancement du téléchargement

            UpdatePrepareSongState("Downloading...", 5);

            // Donwload the stream to file
            await youtube.Videos.Streams.DownloadAsync(streamInfo, resultPath, new Progress<double>(percent => UpdatePrepareSongState("Downloading...", 5 + percent * 45f)));

            return resultPath;
        }

        /// <summary>
        /// 
        /// Converti le fichier .Webm référencé par @pathWebm en .Wav à destination du répertoire @destinationFolder de manière asynchrone.
        /// Le nom est conservé.
        /// 
        /// </summary>
        /// <param name="pathWebm"> Le path référancant le fichier .webm (extension incluse) </param>
        /// <param name="destinationFolder"> path du répértoire cible pour l'exportation </param>
        /// <returns> The path of the new file created (.Wav) in the @destinationFolder with the same name and with async function </returns>
        private async Task<string> ConvertWebmToWavAsync(string pathWebm, string destinationFolder)
        {
            // On initialise la progression

            UpdatePrepareSongState("Convert file Initialisation", 51);

            // On calcul le chemin final de destination du fichier futurement converti
            string wavPath = destinationFolder + '/' + Path.GetFileNameWithoutExtension(pathWebm) + WAV_EXTENSION;

            cs_ffmpeg_mp3_converter.FFMpeg ffmpeg = new cs_ffmpeg_mp3_converter.FFMpeg();
            await ffmpeg.ConvertWebmDoubleToWavAsync(pathWebm, wavPath, new Progress<double>(percent => UpdatePrepareSongState("Convert To Wav...", 51f + percent * 9f)));

            return wavPath;
        }

        /// <summary>
        /// Charge le fichier audio .Wav indiquer par path et renvoie un AudioClip associé.
        /// </summary>
        /// <param name="path"> Le chemin du fichier audio au format .Wav </param>
        /// <returns> L'audioClip associé au .Wav pointé par Path. </returns>
        private async Task<AudioClip> LoadWavAudioFromAsync(string path)
        {
            StartCoroutine("_UnityWebRequestDownloadFile", path);
            await Task.Run(() =>
            {
                while (DownloadedSongAtAudioClip == null)
                {
                    Thread.Sleep(100);
                }
            });

            return DownloadedSongAtAudioClip;
        }

        /// <summary>
        /// Analyze l'audio et renvoie la liste des moments forts associé.
        /// </summary>
        /// <param name="audio"> L'audioClip à analyser </param>
        /// <returns> La liste des moments fort au format List<SpectralFluxInfo> </returns>
        private async Task<List<SpectralFluxInfo>> AnalyzeAudio(AudioClip audio)
        {
            UpdatePrepareSongState("Begin map generation...", 66);
            // On récupére les infos sur la musique

            float[] MultiChannelsSamples = new float[audio.samples * audio.channels];
            int NumChannels = audio.channels;
            int NumTotalSamples = audio.samples;
            // We are not evaluationg the audio as it is being played by Unity, so we need the clip's sampling rate
            int SampleRate = audio.frequency;

            audio.GetData(MultiChannelsSamples, 0);

            UpdatePrepareSongState("Analyze audio for generation...", 68);
            SpectralFluxAnalyzer spectralFluxAnalyzer = await GetFullSpectrumThreaded(NumTotalSamples, MultiChannelsSamples, NumChannels, SampleRate);
            return spectralFluxAnalyzer.trueBeats;
        }

        /// <summary>
        /// Analyse l'audio et renvoie le SpectralFluxAnalyzer associé.
        /// </summary>
        /// <param name="numTotalSamples"></param>
        /// <param name="multiChannelsSamples"></param>
        /// <param name="numChannels"></param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        private async Task<SpectralFluxAnalyzer> GetFullSpectrumThreaded(int numTotalSamples, float[] multiChannelsSamples, int numChannels, int sampleRate)
        {
            SpectralFluxAnalyzer PreProcessedSpectralFluxAnalyzer = new SpectralFluxAnalyzer();
            IProgress<double> progress = new Progress<double>(percent => UpdatePrepareSongState("Analyze Audio...", 68f + percent * 27f));

            // Lancement de la tache lourde sur un autre thread avec suivi de progression
            await Task.Run(() =>
            {
                // We only need to retain the samples for combined channels over the time domain
                float[] preProcessedSamples = new float[numTotalSamples];

                int numProcessed = 0;
                float combinedChannelAverage = 0f;
                for (int i = 0; i < multiChannelsSamples.Length; ++i)
                {
                    combinedChannelAverage += multiChannelsSamples[i];

                    // Each time we have processed all channels samples for a point in time, we will store the average of the channels combined
                    if ((i + 1) % numChannels == 0)
                    {
                        preProcessedSamples[numProcessed] = combinedChannelAverage / numChannels;
                        numProcessed++;
                        combinedChannelAverage = 0f;
                    }

                    progress.Report(0.5f * i / multiChannelsSamples.Length); // On met à jours la progression (valeur de 50% max ici)
                    Thread.Yield();
                }

                // Once we have our audio sample data prepared, we can execute an FFT to return the spectrum data over the time domain
                int spectrumSampleSize = 1024;
                int iterations = preProcessedSamples.Length / spectrumSampleSize;

                FFT fft = new FFT();
                fft.Initialize((UInt32)spectrumSampleSize);

                double[] sampleChunk = new double[spectrumSampleSize];
                for (int i = 0; i < iterations; ++i)
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
                    float curSongTime = ((1f / (float)sampleRate) * i) * spectrumSampleSize;

                    // Send our magnitude data off to our Spectral Flux Analyzer to be analyzed for peaks
                    PreProcessedSpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime);
                    progress.Report(0.5f * i / iterations); // On met à jours la progression (valeur de 50% max ici)
                    Thread.Yield();
                }
            });

            UpdatePrepareSongState("Analyze audio end", 95);
            return PreProcessedSpectralFluxAnalyzer;
        }

        /// <summary>
        /// Enregistre les données de l'audio au format binaire sous le nom DATA_FILE de manière async.
        /// </summary>
        /// <param name="realBeats"> Les données de l'audio </param>
        /// <param name="mapRepository"> Le repertoire où enregistrer les données </param>
        /// <returns></returns>
        private async Task SaveMapAsync(List<SpectralFluxInfo> realBeats, string mapRepository)
        {
            IProgress<double> progress = new Progress<double>(percent => UpdatePrepareSongState("Save map data...", 95f + percent * 0.04f));
            
            await Task.Run(() =>
            {
                byte[] binaryInfo = ObjectToByteArray(realBeats); // On converti les infos en données binaire
                using (BinaryWriter writer = new BinaryWriter(File.Open(mapRepository + '/' + DATA_FILE, FileMode.Create)))
                {
                    int count = 1024;
                    int index;

                    for (index = 0; index < binaryInfo.Length; index += count)
                    {
                        progress.Report((double)index / binaryInfo.Length);
                        writer.Write(binaryInfo, index, Math.Min(count, binaryInfo.Length - (index + 1)));
                        Thread.Yield();
                    }

                    writer.Close();
                }
            });

            UpdatePrepareSongState("Save map end", 99);
        }

        /// <summary>
        /// Supprime le fichier .Webm désormais inutile
        /// </summary>
        /// <param name="Directory"> Le répertoire de la chanson </param>
        /// <returns></returns>
        private void DeleteTemporaryFile(string Directory)
        {
            File.Delete(Directory + '/' + DEFAULT_NAME + WEBM_EXTENSION);
            UpdatePrepareSongState("Map saved !", 100);
        }
        #endregion

        #region Coroutines

        private IEnumerator _UnityWebRequestDownloadFile(string path)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.WAV))
            {
                www.SendWebRequest();

                while (!www.isDone)
                {
                    UpdatePrepareSongState("Loading Song...", 60 + www.downloadProgress * 5f);
                    yield return new WaitForSeconds(0.2f); ;
                }

                if (www.isNetworkError || www.isHttpError)
                {
                    throw new Exception("Error when loading audio");
                }
                else
                {
                    UpdatePrepareSongState("Loading end...", 65);
                    DownloadedSongAtAudioClip = DownloadHandlerAudioClip.GetContent(www);
                }
            }
        }

        #endregion

        private void UpdatePrepareSongState(string state, double value)
        {
            EventManager.Instance.Raise(new ProgressBarPrepareSongHaveChangedEvent()
            {
                State = state,
                Value = value
            });
        }

        /// <summary>
        /// - Envoi le message d'erreur
        /// - Envoi un event PrepareSongEndEvent
        /// - Change l'état du manager à STATE.Nothing
        /// </summary>
        /// <param name="msg"></param>
        private void AnErrorOccurred(string msg)
        {
            EventManager.Instance.Raise(new ProgressBarPrepareSongErrorEvent()
            {
                msg = msg
            });
            EventManager.Instance.Raise(new PrepareSongEndEvent());
            ServerAccountManager_State = STATE.Nothing;
        }

        #endregion

        #region Binary Tools

        private byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            object obj = binForm.Deserialize(memStream);

            return obj;
        }

        #endregion
    }
}