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

		#region Tools

		private void PlayMusic(AudioClip clip)
		{
			// Si la musique demandé est déjà lancé, ne fait rien.
			if (AudioSource.clip == clip)
			{
				return;
			}

			AudioSource.Stop();
			AudioSource.volume = 0;
			AudioSource.clip = clip;
			AudioSource.Play();
			StartCoroutine("PlaySongSmooth");
		}

		private IEnumerator PlaySongSmooth()
		{

			while (AudioSource.volume < 1)
			{
				AudioSource.volume = AudioSource.volume + SPEED_INCREMENT_VOLUME;
				yield return new WaitForSeconds(SPEED_ACTUALISE_VOLUME);
			}
		}

		private void StopCurrentMusic()
		{
			if (AudioSource.isPlaying)
			{
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
			AudioClip clip = ServerGameManager.Instance.GetCurrentAudioClip();
			AudioSource.loop = false;
			PlayMusic(clip);
		}

		#endregion

		#region GameEvent Function

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

		#endregion
	}
}
