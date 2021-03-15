namespace ServerManager
{
	using UnityEngine;
	using System.Collections.Generic;

	/// <summary>
	/// Sfx manager.
	/// </summary>
	public class SfxManager : Singleton<SfxManager>
	{
		[Header("SfxManager")]

		#region AudioClip
		[Header("InGame")]
		public AudioClip Balloon;
		[SerializeField] private AudioClip CharacterHappy;
		[SerializeField] private AudioClip CharacterHappy2;
		[SerializeField] private AudioClip CharacterHit;
		[SerializeField] private AudioClip CharacterHit2;

		[Header("UI")]
		[SerializeField] private AudioClip UI_Sound1;
		[SerializeField] private AudioClip UI_Sound2;
		[SerializeField] private AudioClip UI_Sound3;
		#endregion

		[SerializeField] private AudioSource Source;

        #region Methods

        public void PlaySfx(AudioClip clip)
		{
			Source.Stop();
			Source.clip = clip;
			Source.Play();
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
	}

}