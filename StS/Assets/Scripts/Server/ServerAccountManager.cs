using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using NAudio.FileFormats.Mp3;
using NAudio.Wave;
using SDD.Events;
using System;
using System.Collections;
using System.IO;
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
        // Constante

        private static readonly string WEBM_EXTENSION = ".webm";
        private static readonly string MP3_EXTENSION = ".mp3";
        private static readonly string WAV_EXTENSION = ".wav";
        private static readonly string DEFAULT_NAME = "Audio";


        // Attributs

        private volatile AudioClip DownloadedAudioClip;


        #region Manager implementation
        protected override IEnumerator InitCoroutine()
        {
            LoadSongData();
            yield break;
        }

        private void LoadSongData()
        {
            
        }
        #endregion


        // Requetes



        // Méthodes

        public async void AddYoutubeSong(string Url)
        {
            string SongPath = await ExtractYoutubeAudio(Url);

            ConvertWebmToWav(SongPath, Path.GetDirectoryName(SongPath));
            //AudioClip clip = await LoadAudioFrom(SongPath);
            //Debug.Log("reussis");
        }


        // Outils

        #region async function

        /// <summary>
        /// 
        /// Extract the youtube audio of the url and return the path of the created file.
        /// 
        /// </summary>
        /// <returns> The path of the file created in DEFAULT_EXTENSION with DEFAULT_NAME </returns>
        private async Task<string> ExtractYoutubeAudio(string Url)
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

            string directory = Application.persistentDataPath + "/" + video.Title.Replace(Path.GetInvalidPathChars().ToString(), "");
            Directory.CreateDirectory(directory);

            string resultPath = directory + "/" + DEFAULT_NAME + WEBM_EXTENSION;
            if (streamInfo != null)
            {
                UpdatePrepareSongState("Downloading...", 5);

                // Get the actual stream
                Stream stream = await youtube.Videos.Streams.GetAsync(streamInfo);

                // Donwload the stream to file
                await youtube.Videos.Streams.DownloadAsync(streamInfo, resultPath, new Progress<double>(percent => UpdatePrepareSongState("Downloading...", 5 + percent * 55f)));

                return resultPath;
            } else
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Converti le fichier mp3 en wav à destination de target
        /// </summary>
        /// <param name="path"></param>
        /// <param name="target"></param>
        /// <returns> target </returns>
        private string ConvertWebmToWav(string pathWebm, string targetDirectory)
        {
            //// On converti en Mp3
            //Debug.Log("Convertion en .mp3");
            //string mp3Path = targetDirectory + "/" + DEFAULT_NAME + MP3_EXTENSION;
            //cs_ffmpeg_mp3_converter.FFMpeg.Convert2Mp3(pathWebm, mp3Path);

            //// On convertit en .wav
            //using (Mp3FileReader mp3 = new Mp3FileReader(mp3Path))
            //{
            //    using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
            //    {
            //        Debug.Log("Convertion en .wav");
            //        string wavPath = targetDirectory + DEFAULT_NAME + WAV_EXTENSION;
            //        WaveFileWriter.CreateWaveFile(wavPath, pcm);
            //        Debug.Log("Fin");
            //        Debug.Log(wavPath);
            //        return wavPath;
            //    }
            //}

            // On converti en .wav
            Debug.Log("Convertion en .wav");
            string wavPath = targetDirectory + DEFAULT_NAME + WAV_EXTENSION;

            cs_ffmpeg_mp3_converter.FFMpeg ffmpeg = new cs_ffmpeg_mp3_converter.FFMpeg();
            ffmpeg.ConvertWebmToWav(pathWebm, wavPath, new Progress<double>(percent => UpdatePrepareSongState("Convert To Wav...", 60 + percent * 40f)));

            return pathWebm;
        }

        /// <summary>
        /// Charge le mp3 indiquer par path et renvoie un AudioClip associé.
        /// </summary>
        /// <param name="path"></param>
        /// <returns> L'audioClip associé au mp3 pointé par Path. </returns>
        private async Task<AudioClip> LoadAudioFrom(string path)
        {
            StartCoroutine("_UnityWebRequestDownloadFile", path);
            await Task.Run(() =>
            {
                while (DownloadedAudioClip == null)
                {
                    Thread.Sleep(1);
                }
            });

            return DownloadedAudioClip;
        }

        #endregion

        #region Coroutines

        private IEnumerator _UnityWebRequestDownloadFile(string path)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG))
            {
                www.SendWebRequest();

                while (!www.isDone)
                {
                    UpdatePrepareSongState("Loading Song...", www.downloadProgress);
                    yield return new WaitForSeconds(0.2f); ;
                }

                if (www.isNetworkError || www.isHttpError)
                {
                    throw new Exception();
                } else
                {
                    DownloadedAudioClip = DownloadHandlerAudioClip.GetContent(www);
                }
            }
        }

        #endregion

        private void UpdatePrepareSongState(string state, double value)
        {
            EventManager.Instance.Raise(new ProgressBarPrepareSongHaveChanged()
            {
                State = state,
                Value = value
            });
        }
    }
}