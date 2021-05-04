using UnityEngine;
using System.Collections;
using SDD.Events;

namespace ServerManager
{
	/// <summary>
	/// Music loops manager.
	/// Gestion de boucles musicales avec Fade-In/Fade-Out entre 2 boucles
	/// </summary>
	public class ServerMusicManager : ServerManager<ServerMusicManager>
	{
		// Constante

		// Vitesse de transition entre 2 clips ou temps de lancement d'un clip.
		private readonly float SPEED_INCREMENT_VOLUME = 0.05f;
		private readonly float SPEED_ACTUALISE_VOLUME = 0.1f;


		// Attributs

		[SerializeField] private AudioClip MenuMusic;

		private AudioSource AudioSource;

		#region Request

		/// <summary>
		/// Renvoie la durée de la musique de la manche courante en secondes
		/// </summary>
		/// <returns></returns>
		public float GetTotalDurationRoundMusic()
		{
			if (ServerGameManager.Instance.GetGameState != GameState.gamePlay)
				throw new System.Exception("Nous ne sommes pas en partie");


			return AudioSource.clip.length;
		}

		/// <summary>
		/// Renvoie le temps restant de la musique de la manche courante en secondes
		/// </summary>
		public float GetTimeLeftRoundMusic()
		{
			if (ServerGameManager.Instance.GetGameState != GameState.gamePlay)
				throw new System.Exception("Nous ne sommes pas en partie");

			return AudioSource.clip.length - AudioSource.time;
		}

		public float GetCurrentTimeRoundMusic()
		{
			return AudioSource.time;
		}

		public bool IsPlayingMusic()
		{
			return AudioSource.isPlaying;
		}

		public float GetVolume()
		{
			return AudioSource.volume;
		}

		#endregion

		#region Subs methods

		public override void SubscribeEvents()
		{
			base.SubscribeEvents();

			EventManager.Instance.AddListener<RoundStartEvent>(RoundStart);
		}

		public override void UnsubscribeEvents()
		{
			base.UnsubscribeEvents();

			EventManager.Instance.RemoveListener<RoundStartEvent>(RoundStart);
		}

        #endregion

        #region Methods

		public void SetVolume(float v)
		{
			AudioSource.volume = v;
		}

        #endregion

        #region Tools

        private void PlayMusic(AudioClip clip)
		{
			// Si la musique demandé est déjà lancé, ne fait rien.
			if (AudioSource.clip == clip)
			{
				return;
			}

			AudioSource.Stop();
			AudioSource.clip = clip;
			AudioSource.Play();
			StartCoroutine("PlaySongSmooth");
		}

		private IEnumerator PlaySongSmooth()
		{
			float tampon = AudioSource.volume;
			AudioSource.volume = 0;

			while (AudioSource.volume < tampon)
			{
				AudioSource.volume = AudioSource.volume + SPEED_INCREMENT_VOLUME;
				yield return new WaitForSeconds(SPEED_ACTUALISE_VOLUME);
			}

			AudioSource.volume = tampon;
		}

		private void StopCurrentMusic()
		{
			if (AudioSource.isPlaying)
			{
				AudioSource.clip = null;
				AudioSource.Stop();
			}
		}

		private void PauseCurrentMusic()
		{
			if (AudioSource.isPlaying)
			{
				AudioSource.Pause();
			}
		}

		private void ReplayCurrentMusic()
		{
			if (!AudioSource.isPlaying)
			{
				AudioSource.Play();
			}
		}

        #endregion

        #region Manager Implementation

        protected override IEnumerator InitCoroutine()
		{
			AudioSource = GetComponent<AudioSource>();

			yield break;
		}

		#endregion

		#region CallBack Event

		private void RoundStart(RoundStartEvent e)
		{
			AudioSource.Play();
			StartCoroutine("PlaySongSmooth");
		}

		#endregion

		#region GameEvent Function

		protected override void GamePlay(GamePlayEvent e)
		{
			base.GamePlay(e);

			AudioSource.Stop();
			AudioSource.loop = false;
			AudioSource.clip = ServerGameManager.Instance.GetCurrentAudioClip();
		}

		protected override void GameMainMenu(GameMainMenuEvent e)
		{
			base.GameMainMenu(e);

			AudioSource.loop = true;
			PlayMusic(MenuMusic);
		}

		protected override void GameRoomMenu(GameRoomMenuEvent e)
		{
			base.GameRoomMenu(e);

			AudioSource.loop = true;
			PlayMusic(MenuMusic);
		}

		protected override void GameOptionsMenu(GameOptionsMenuEvent e)
		{
			base.GameOptionsMenu(e);

			AudioSource.loop = true;
			PlayMusic(MenuMusic);
		}

		protected override void GameEnd(GameEndEvent e)
		{
			base.GameEnd(e);

			AudioSource.loop = true;
			PlayMusic(MenuMusic);
		}

		protected override void GameResult(GameResultEvent e)
		{
			base.GameResult(e);

			AudioSource.loop = true;
			PlayMusic(MenuMusic);
		}

		protected override void GameMusicSelectionMenu(GameMusicSelectionMenuEvent e)
		{
			base.GameMusicSelectionMenu(e);

			AudioSource.loop = true;
			PlayMusic(MenuMusic);
		}

		protected override void GameMusicResultMenu(GameMusicResultMenuEvent e)
		{
			base.GameMusicResultMenu(e);

			StopCurrentMusic();
		}

		protected override void GameContinue(GameContinueEvent e)
		{
			base.GameContinue(e);

			ReplayCurrentMusic();
		}

		protected override void GamePause(GamePauseEvent e)
		{
			base.GamePause(e);

			PauseCurrentMusic();
		}

		#endregion
	}
}
