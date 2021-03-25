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

namespace ServerManager
{
    /// <summary>
    /// Classe permettant de stocker les musiques, de les importer, ou de les exporter.
    /// </summary>
    public class ServerAccountManager : ServerManager<ServerAccountManager>
    {
        private enum STATE { ADDING, LOADING, NOTHING }


        // Constante

        private static readonly string WEBM_EXTENSION = ".webm";
        private static readonly string WAV_EXTENSION = ".wav";

        private static readonly string AUDIO_FILE = "Audio";
        private static readonly string DATA_FILE = "Data";

        // Song map data
        private static readonly float MIN_THRESHOLD_INTENSITY = 0.1f;
        private static readonly float MIN_TIME_BETWEEN_BEATS = 0.275f; // En seconde


        // Attributs

        private STATE CurrentState { get; set; }
        private AudioClip DownloadedSongAtAudioClip;
        private bool IsCancelled; // TODO


        #region Manager implementation
        protected override IEnumerator InitCoroutine()
        {
            // Initialisation de l'état de base
            CurrentState = STATE.NOTHING;

            yield break;
        }
        #endregion


        // Requetes

        /// <summary>
        /// Renvoi le path de la liste des chansons actuellement enregistré.
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
            await WaitToManagerReadyAsync();
            CurrentState = STATE.ADDING; // On dit que le manager add une chanson

            try
            {
                string WebmSongPath = await ExtractYoutubeAudioWebmAsync(Url); // On récupére l'audio de Youtube au format .Webm
                string Directory = Path.GetDirectoryName(WebmSongPath);
                string WavPath = await ConvertWebmToWavAsync(WebmSongPath, Directory); // On converti le fichier .Webm double en .Wav
                AudioClip clip = await LoadWavAudioFromAsync(WavPath);
                List<SpectralFluxInfo> realBeats = await AnalyzeAudioAsync(clip);
                await SaveMapAsync(realBeats, Directory);
                DeleteTemporaryFile(Directory);
            } catch (Exception e)
            {
                UpdatePrepareSongAnErrorOccurred(e.Message);
                return;
            } finally
            {
                ManagerIsReady(); // On libére le manager
            }

            // On averti que l'opération est terminé
            EventManager.Instance.Raise(new PrepareSongEndEvent());
        }

        /// <summary>
        /// Delete les données de l'audio contenu dans le directory pointé par path.
        /// Envoie un DataSongDeletedEvent une fois fini.
        /// </summary>
        /// <param name="path"> Le directory contenant les données de l'audio </param>
        public void RemoveSongWithDirectoryPath(string path)
        {
            Directory.Delete(path, true);
            EventManager.Instance.Raise(new DataSongDeletedEvent());
        }

        /// <summary>
        /// Renvoie l'audio clip associé au répertoire correspondant à la musique souhaité. (Asynchrone)
        /// Pour suivre la progression, s'abonner à "UpdateLoadingAudioClipFromSong" de type SDD.Events.Event
        /// </summary>
        /// <param name="directory"> Le répertoire contenant les informations de la musique </param>
        /// <returns> L'audio clip associé </returns>
        public async Task<AudioClip> GetAudioClipOfSongAsync(string directory)
        {
            await WaitToManagerReadyAsync();
            CurrentState = STATE.LOADING; // On dit que le manager load une chanson

            AudioClip clip = await GetAudioClipOfWavSongAsync(directory + '/' + AUDIO_FILE + WAV_EXTENSION);

            ManagerIsReady();
            return clip;
        }

        /// <summary>
        /// Renvoie les données de la carte stocké dans le repertoire 'directory' (Asynchrone)
        /// Pour suivre la progression, s'abonner à "UpdateLoadingMapDataEvent"
        /// </summary>
        /// <param name="directory"> Le répertoire contenant les données de la chansons </param>
        /// <returns> Une List<SpectralFluxInfo> correspondant au donnée de la carte. /returns>
        public async Task<List<SpectralFluxInfo>> GetMapDataAsync(string directory)
        {
            IProgress<double> progress = new Progress<double>(percent => UpdateLoadingMapData(percent));
            string file = directory + '/' + DATA_FILE;

            if (!File.Exists(file))
            {
                throw new Exception("Fichier manquant");
            }

            List<SpectralFluxInfo> result = await Task.Run(() =>
            {
                byte[] binaryInfo;
                using (FileStream reader = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    long fileLength = reader.Length;
                    binaryInfo = new byte[fileLength];

                    int count = 1024;
                    int index;

                    for (index = 0; index < binaryInfo.Length; index += count)
                    {
                        progress.Report((double)index / binaryInfo.Length);
                        reader.Read(binaryInfo, index, Math.Min(count, binaryInfo.Length - index));
                        Thread.Yield();
                    }
                }

                return (List<SpectralFluxInfo>) ByteArrayToObject(binaryInfo);
            });

            UpdateLoadingMapData(1);

            return result;
        }


        // Outils

        #region GetAudioClipOfSongAsync

        /// <summary>
        /// Charge le fichier audio .Wav indiquer par path et renvoie un AudioClip associé.
        /// Pour suivre la progression, s'abonner à "UpdateLoadingAudioClipFromSong" de type SDD.Events.Event
        /// </summary>
        /// <param name="path"> Le chemin du fichier audio au format .Wav </param>
        /// <returns> L'audioClip associé au .Wav pointé par Path. </returns>
        private async Task<AudioClip> GetAudioClipOfWavSongAsync(string path)
        {
            DownloadedSongAtAudioClip = null;
            StartCoroutine("_GetAudioClipOfWavSongAsync", path);
            await Task.Run(() =>
            {
                while (DownloadedSongAtAudioClip == null)
                {
                    Thread.Sleep(100);
                }
            });

            return DownloadedSongAtAudioClip;
        }

        private IEnumerator _GetAudioClipOfWavSongAsync(string path)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.WAV))
            {
                www.SendWebRequest();

                while (!www.isDone)
                {
                    UpdateLoadingAudioClipFromSong(www.downloadProgress);
                    yield return new WaitForSeconds(0.2f); ;
                }

                if (www.isNetworkError || www.isHttpError)
                {
                    throw new Exception("Error when loading audio");
                }
                else
                {
                    DownloadedSongAtAudioClip = DownloadHandlerAudioClip.GetContent(www);

                    if (DownloadedSongAtAudioClip == null)
                    {
                        throw new Exception("An error occurred");
                    }
                }
            }
        }

        /// <summary>
        /// Permet de suivre la progression du chargement
        /// </summary>
        /// <param name="value"> Valeur [0f, 1f]</param>
        private void UpdateLoadingAudioClipFromSong(float value)
        {
            EventManager.Instance.Raise(new UpdateLoadingProgressionAudioClipEvent()
            {
                progression = value
            });
        }

        #endregion

        #region GetMapDataAsync

        private void UpdateLoadingMapData(double value)
        {
            EventManager.Instance.Raise(new UpdateLoadingMapDataEvent()
            {
                progression = value
            });
        }

        #endregion

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

                // On récupére les charactères interdit
            char[] tampon = Path.GetInvalidFileNameChars();
            char[] invalidChar = new char[tampon.Length];
            int i;
            for (i = 0; i < tampon.Length; ++i)
            {
                invalidChar[i] = tampon[i];
            }


            string newFolder = Replace(video.Title, invalidChar, ""); // Nouveau dossier à créer
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

            string resultPath = directory + "/" + AUDIO_FILE + WEBM_EXTENSION;

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
        /// Clean l'audio en fonction des constante MIN_THRESHOLD_INTENSITY & MIN_TIME_BETWEEN_BEATS
        /// </summary>
        /// <param name="audio"> L'audioClip à analyser </param>
        /// <returns> La liste des moments fort au format List<SpectralFluxInfo> </returns>
        private async Task<List<SpectralFluxInfo>> AnalyzeAudioAsync(AudioClip audio)
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
            SpectralFluxAnalyzer spectralFluxAnalyzer = await GetFullSpectrumThreadedAsync(NumTotalSamples, MultiChannelsSamples, NumChannels, SampleRate);

            // Clean Up Data
            UpdatePrepareSongState("Clean up data...", 91);

            List<SpectralFluxInfo> result = new List<SpectralFluxInfo>(); // On initialise la liste 'résultat'
            float LastBeatTime = -MIN_TIME_BETWEEN_BEATS; // On initialise au plus bas le lastBeatTime

            foreach (SpectralFluxInfo sfi in spectralFluxAnalyzer.trueBeats)
            {
                // On regarde si la valeur eest écoutable
                if (sfi.prunedSpectralFlux >= MIN_THRESHOLD_INTENSITY)
                {
                    // On regarde si le delai entre le current et le précédent est au dessus du seuil minimum
                    if (sfi.time - LastBeatTime >= MIN_TIME_BETWEEN_BEATS)
                    {
                        result.Add(sfi);
                        LastBeatTime = sfi.time;
                    }
                }
            }

            UpdatePrepareSongState("Clean up end", 95);
            return result;
        }

        /// <summary>
        /// Analyse l'audio et renvoie le SpectralFluxAnalyzer associé.
        /// </summary>
        /// <param name="numTotalSamples"></param>
        /// <param name="multiChannelsSamples"></param>
        /// <param name="numChannels"></param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        private async Task<SpectralFluxAnalyzer> GetFullSpectrumThreadedAsync(int numTotalSamples, float[] multiChannelsSamples, int numChannels, int sampleRate)
        {
            SpectralFluxAnalyzer PreProcessedSpectralFluxAnalyzer = new SpectralFluxAnalyzer();
            IProgress<double> progress = new Progress<double>(percent => UpdatePrepareSongState("Analyze Audio...", 68f + percent * 22f));

            // Lancement de la tache lourde sur un autre thread avec suivi de progression
            await Task.Run(() =>
            {
                // We only need to retain the samples for combined channels over the time domain
                float[] preProcessedSamples = new float[numTotalSamples];

                int numProcessed = 0;
                float combinedChannelAverage = 0f;

                int refresh = Math.Max(1, multiChannelsSamples.Length / 25); // On refreshera la bar de chargement 25 fois maximum, 1 fois mini.
                for (int i = 0; i < multiChannelsSamples.Length; ++i)
                {
                    combinedChannelAverage += multiChannelsSamples[i];

                    // Each time we have processed all channels samples for a point in time, we will store the average of the channels combined
                    if ((i + 1) % numChannels == 0)
                    {
                        preProcessedSamples[numProcessed] = combinedChannelAverage / numChannels;
                        numProcessed++;
                        combinedChannelAverage = 0f;

                        Thread.Yield(); // On rend la main à d'autre Thread de temps à autre pour ne pas bloquer
                    }

                    if (i % refresh == 0) // On limite l'opération couteuse
                    {
                        progress.Report(0.5f * i / multiChannelsSamples.Length); // On met à jours la progression (valeur de 50% max ici)
                    }
                }

                // Once we have our audio sample data prepared, we can execute an FFT to return the spectrum data over the time domain
                int spectrumSampleSize = 1024;
                int iterations = preProcessedSamples.Length / spectrumSampleSize;

                FFT fft = new FFT();
                fft.Initialize((UInt32)spectrumSampleSize);

                double[] sampleChunk = new double[spectrumSampleSize];
                refresh = Math.Max(1, iterations / 25); // On refreshera la bar de chargement 25 fois maximum, 1 fois mini.
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

                    if (i % refresh == 0) // On limite l'opération couteuse
                    {
                        progress.Report(0.5f + 0.5f * i / iterations); // On met à jours la progression (valeur de 50% max ici)
                        Thread.Yield(); // On rend la main à d'autres Thread
                    }
                }
            });

            UpdatePrepareSongState("Analyze audio end", 90);
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
                        writer.Write(binaryInfo, index, Math.Min(count, binaryInfo.Length - index));
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
            File.Delete(Directory + '/' + AUDIO_FILE + WEBM_EXTENSION);
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
                    UpdatePrepareSongAnErrorOccurred("Error when loading audio");
                }
                else
                {
                    UpdatePrepareSongState("Loading end...", 65);
                    DownloadedSongAtAudioClip = DownloadHandlerAudioClip.GetContent(www);

                    if (DownloadedSongAtAudioClip == null)
                    {
                        UpdatePrepareSongAnErrorOccurred("An error occurred");
                    }
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
        private void UpdatePrepareSongAnErrorOccurred(string msg)
        {
            EventManager.Instance.Raise(new ProgressBarPrepareSongErrorEvent()
            {
                msg = msg
            });
            EventManager.Instance.Raise(new PrepareSongEndEvent());
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

        /// <summary>
        /// Permet d'attendre jusqu'à ce que le manager soit pret.
        /// </summary>
        /// <returns></returns>
        private async Task WaitToManagerReadyAsync()
        {
            // Si une opération est déjà en cours, on attend.
            if (CurrentState != STATE.NOTHING)
            {
                await Task.Run(() =>
                {
                    while (CurrentState != STATE.NOTHING)
                    {
                        Thread.Sleep(200);
                    }
                });
            }
        }

        /// <summary>
        /// Indique que le manager est pret
        /// </summary>
        private void ManagerIsReady()
        {
            // On indique que l'opération est terminé.
            CurrentState = STATE.NOTHING;
        }

        private string Replace(string s, char[] separators, string newVal)
        {
            string[] temp;

            temp = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            return String.Join(newVal, temp);
        }
    }
}