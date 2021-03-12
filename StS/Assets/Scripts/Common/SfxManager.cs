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

		public AudioClip Balloon;

        #endregion

		[SerializeField] private AudioSource Source;

		public void PlaySfx(AudioClip clip)
		{
			Source.Stop();
			Source.clip = clip;
			Source.Play();
		}
	}

}