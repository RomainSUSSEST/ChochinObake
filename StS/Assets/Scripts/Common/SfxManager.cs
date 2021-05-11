namespace ServerManager
{
	using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

	/// <summary>
	/// Sfx manager.
	/// </summary>
	public class SfxManager : ServerManager<SfxManager>
	{
        #region Attributes

		#region AudioClip
		[Header("InGame")]
		public AudioClip Balloon;

		[Header("Effects")]
		public AudioClip GongAppear;
		public AudioClip GongHit;

		public AudioClip FireworksExplosion;

		[Header("Character")]
		public AudioClip LeaderChange;

		[Header("UI")]
		[SerializeField] private AudioClip UI_Sound1;
		[SerializeField] private AudioClip UI_Sound2;
		[SerializeField] private AudioClip UI_Sound3;
		#endregion

		[SerializeField] private AudioSource DefaultSource;
		[SerializeField] private AudioSource CharacterAudioSource;

		private Dictionary<ulong, AudioSource> AudioSourcesPlayer;
		private Dictionary<string, AudioSource> AudioSourcesAI;

		#endregion

		#region Request

		public float GetVolume()
		{
			return DefaultSource.volume;
		}

        #endregion

        #region Methods

        public void PlayDefaultSfx(AudioClip clip)
		{
			DefaultSource.Stop();
			DefaultSource.clip = clip;
			DefaultSource.Play();
		}

		public void PlayCharacterSfx(AudioClip clip)
		{
			CharacterAudioSource.Stop();
			CharacterAudioSource.clip = clip;
			CharacterAudioSource.Play();
		}

		public void PlayerPlaySfx(ulong ID, AudioClip clip)
		{
			AudioSource audio = AudioSourcesPlayer[ID];
			audio.Stop();
			audio.clip = clip;
			audio.Play();
		}

		public void AIPlaySfx(string Name, AudioClip clip)
		{
			AudioSource audio = AudioSourcesAI[Name];
			audio.Stop();
			audio.clip = clip;
			audio.Play();
		}

		public void SetVolume(float v)
		{
			DefaultSource.volume = v;
			CharacterAudioSource.volume = v;
		}

        #endregion

        #region UI Events
        public void ButtonNextHasBeenClicked()
		{
			PlayDefaultSfx(UI_Sound1);
		}

		public void ButtonPanelHasBeenClicked()
		{
			PlayDefaultSfx(UI_Sound2);
		}

		public void ButtonLeaveHasBeenClicked()
		{
			PlayDefaultSfx(UI_Sound3);
		}

        #endregion

        #region Manager implementation

		protected override IEnumerator InitCoroutine()
		{
			yield break;
		}

		#endregion

		#region GameState

		protected override void GamePlay(GamePlayEvent e)
		{
			base.GamePlay(e);

			AudioSource tampon;

			AudioSourcesPlayer = new Dictionary<ulong, AudioSource>();

			foreach (ulong id in ServerGameManager.Instance.GetPlayers().Keys)
			{
				tampon = gameObject.AddComponent<AudioSource>();
				tampon.volume = DefaultSource.volume;
				AudioSourcesPlayer.Add(id, tampon);
			}

			AudioSourcesAI = new Dictionary<string, AudioSource>();

			foreach (AI_Player ai in ServerGameManager.Instance.GetAIList())
			{
				tampon = gameObject.AddComponent<AudioSource>();
				tampon.volume = DefaultSource.volume;
				AudioSourcesAI.Add(ai.Name, tampon);
			}
		}

		protected override void GameEnd(GameEndEvent e)
		{
			base.GameEnd(e);

			ClearAudioSources();
		}

		protected override void GameMainMenu(GameMainMenuEvent e)
		{
			base.GameMainMenu(e);

			ClearAudioSources();
		}

		protected override void GameRoomMenu(GameRoomMenuEvent e)
		{
			base.GameRoomMenu(e);

			ClearAudioSources();
		}

		protected override void GameMusicResultMenu(GameMusicResultMenuEvent e)
		{
			base.GameMusicResultMenu(e);

			ClearAudioSources();
		}

		protected override void GameResult(GameResultEvent e)
		{
			base.GameResult(e);

			ClearAudioSources();
		}

		protected override void GamePause(GamePauseEvent e)
		{
			base.GamePause(e);

			DefaultSource.Pause();
			CharacterAudioSource.Pause();
			foreach (AudioSource audio in AudioSourcesPlayer.Values)
			{
				audio.Pause();
			}
			foreach (AudioSource audio in AudioSourcesAI.Values)
			{
				audio.Pause();
			}
		}

		protected override void GameContinue(GameContinueEvent e)
		{
			base.GameContinue(e);

			DefaultSource.UnPause();
			CharacterAudioSource.UnPause();
			foreach (AudioSource audio in AudioSourcesPlayer.Values)
			{
				audio.UnPause();
			}
			foreach (AudioSource audio in AudioSourcesAI.Values)
			{
				audio.UnPause();
			}
		}

		protected override void GameMusicSelectionMenu(GameMusicSelectionMenuEvent e)
		{
			base.GameMusicSelectionMenu(e);

			ClearAudioSources();
		}

		protected override void GameOptionsMenu(GameOptionsMenuEvent e)
		{
			base.GameOptionsMenu(e);

			ClearAudioSources();
		}

		#endregion

		#region tools

		private void ClearAudioSources()
		{
			if (AudioSourcesPlayer != null)
			{
				foreach (AudioSource s in AudioSourcesPlayer.Values)
				{
					Destroy(s);
				}
				AudioSourcesPlayer = null;
			}

			if (AudioSourcesAI != null)
			{
				foreach (AudioSource s in AudioSourcesAI.Values)
				{
					Destroy(s);
				}
				AudioSourcesAI = null;
			}
		}

        #endregion
    }

}