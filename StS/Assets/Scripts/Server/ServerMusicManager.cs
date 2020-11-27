using UnityEngine;
using System.Collections;

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

		[Header("MusicManager")]

		[SerializeField] private AudioClip MenuMusic;

		private AudioSource AudioSource;


		// Life cycle

		protected override void Awake()
		{
			base.Awake();

			AudioSource = GetComponent<AudioSource>();
		}


		// Outils

		private void PlayMusic(AudioClip clip)
		{
			// Si la musique demandé est déjà lancé, ne fait rien. Ou si le clip demandé vaut null
			if (AudioSource.clip == clip || clip == null)
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


		// InitCoroutine

		protected override IEnumerator InitCoroutine()
		{
			yield break;
		}


		// Event GameState function

		#region Event Function

		protected override void GameMainMenu(GameMainMenuEvent e)
		{
			base.GameMainMenu(e);

			PlayMusic(MenuMusic);
		}

		protected override void GameRoomMenu(GameRoomMenuEvent e)
		{
			base.GameRoomMenu(e);

			PlayMusic(MenuMusic);
		}

		protected override void GameOptionsMenu(GameOptionsMenuEvent e)
		{
			base.GameOptionsMenu(e);

			PlayMusic(MenuMusic);
		}

		protected override void GameCreditsMenu(GameCreditsMenuEvent e)
		{
			base.GameCreditsMenu(e);

			PlayMusic(MenuMusic);
		}

		protected override void GamePlay(GamePlayEvent e)
		{
			base.GamePlay(e);


		}

		protected override void GamePause(GamePauseEvent e)
		{
			base.GamePause(e);

			PauseCurrentMusic();
		}

		protected override void GameResume(GameResumeEvent e)
		{
			base.GameResume(e);

			ReplayCurrentMusic();
		}

		protected override void GameEnd(GameEndEvent e)
		{
			base.GameEnd(e);

			PlayMusic(MenuMusic);
		}

		protected override void GameResult(GameResultEvent e)
		{
			base.GameResult(e);

			PlayMusic(MenuMusic);
		}

		protected override void GameMusicSelectionMenu(GameMusicSelectionMenuEvent e)
		{
			base.GameMusicSelectionMenu(e);

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
