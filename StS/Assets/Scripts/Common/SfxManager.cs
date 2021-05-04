namespace ServerManager
{
	using UnityEngine;

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

		public AudioClip GongAppear;
		public AudioClip GongHit;

		[Header("UI")]
		[SerializeField] private AudioClip UI_Sound1;
		[SerializeField] private AudioClip UI_Sound2;
		[SerializeField] private AudioClip UI_Sound3;
		#endregion

		[SerializeField] private AudioSource Source;

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
	}

}