using ServerManager;
using UnityEngine;
using UnityEngine.UI;

public class SoundsPanel_Model : MonoBehaviour
{
    #region Attributes

    [SerializeField] private Slider Music;
    [SerializeField] private Slider SoundEffect;

    #endregion

    #region LifeCycle

    private void Start()
    {
        Music.value = ServerMusicManager.Instance.GetVolume();
        SoundEffect.value = SfxManager.Instance.GetVolume();
    }

    #endregion

    #region OnValueChanged slider

    public void MusicVolumeChanged()
    {
        ServerMusicManager.Instance.SetVolume(Music.value);
    }

    public void SoundEffectsVolumeChanged()
    {
        SfxManager.Instance.SetVolume(SoundEffect.value);
    }

    #endregion
}
