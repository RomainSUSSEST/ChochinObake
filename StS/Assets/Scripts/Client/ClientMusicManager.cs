namespace ClientManager
{
	using System.Collections;
	using UnityEngine;

	public class ClientMusicManager : ClientManager<ClientMusicManager>
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
			AudioSource.Stop();
		}


		// InitCoroutine

		protected override IEnumerator InitCoroutine()
		{
			yield break;
		}


		// Event function

		#region CallBack to GameManager Event

		protected override void MobileMainMenu(MobileMainMenuEvent e)
		{
			base.MobileMainMenu(e);

			PlayMusic(MenuMusic);
		}

		protected override void MobileJoinRoom(MobileJoinRoomEvent e)
		{
			base.MobileJoinRoom(e);

			PlayMusic(MenuMusic);
		}

		protected override void MobileCharacterSelection(MobileCharacterSelectionEvent e)
		{
			base.MobileCharacterSelection(e);

			PlayMusic(MenuMusic);
		}

		protected override void MobileMusicSelection(MobileMusicSelectionEvent e)
		{
			base.MobileMusicSelection(e);

			PlayMusic(MenuMusic);
		}

		#endregion
	}

}
