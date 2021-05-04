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

        [Header("SfxManager")]

		#region AudioClip
		[Header("InGame")]
		public AudioClip Balloon;
		[SerializeField] private AudioClip CharacterHappy;
		[SerializeField] private AudioClip CharacterHappy2;
		[SerializeField] private AudioClip CharacterHit;
		[SerializeField] private AudioClip CharacterHit2;

		public AudioClip GongAppear;
		public AudioClip GongHit;

		[Header("UI")]
		[SerializeField] private AudioClip UI_Sound1;
		[SerializeField] private AudioClip UI_Sound2;
		[SerializeField] private AudioClip UI_Sound3;
		#endregion

		[SerializeField] private AudioSource Source;

		private Dictionary<ulong, AudioSource> AudioSourcesPlayer;
		private Dictionary<string, AudioSource> AudioSourcesAI;

		#endregion

		#region Request

		public float GetVolume()
		{
			return Source.volume;
		}

        #endregion

        #region Methods

        public void PlaySfx(AudioClip clip)
		{
			Source.Stop();
			Source.clip = clip;
			Source.Play();
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
			Source.volume = v;
		}

        #endregion

        #region UI Events
        public void ButtonNextHasBeenClicked()
		{
			PlaySfx(UI_Sound1);
		}

		public void ButtonPanelHasBeenClicked()
		{
			PlaySfx(UI_Sound2);
		}

		public void ButtonLeaveHasBeenClicked()
		{
			PlaySfx(UI_Sound3);
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
				tampon.volume = Source.volume;
				AudioSourcesPlayer.Add(id, tampon);
			}

			AudioSourcesAI = new Dictionary<string, AudioSource>();

			foreach (AI_Player ai in ServerGameManager.Instance.GetAIList())
			{
				tampon = gameObject.AddComponent<AudioSource>();
				tampon.volume = Source.volume;
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