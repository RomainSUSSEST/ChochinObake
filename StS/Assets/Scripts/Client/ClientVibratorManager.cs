using SDD.Events;
using System.Collections;
using UnityEngine;

public class ClientVibratorManager : ClientManager<ClientVibratorManager>
{
    #region Attributes

    private bool m_IsVibratorEnable;

    #endregion

    #region Subs Event

    public override void SubscribeEvents()
    {
        base.SubscribeEvents();

        EventManager.Instance.AddListener<VibrateEvent>(Vibrate);
    }

    public override void UnsubscribeEvents()
    {
        base.UnsubscribeEvents();

        EventManager.Instance.RemoveListener<VibrateEvent>(Vibrate);
    }

    #endregion

    #region Request

    public bool IsVibratorEnable()
    {
        return m_IsVibratorEnable;
    }

    #endregion

    #region Methods

    public void SetVibrator(bool b)
    {
        m_IsVibratorEnable = b;
    }

    #endregion

    #region ManagerImplementation

    protected override IEnumerator InitCoroutine()
    {
        yield break;
    }

    #endregion

    #region Event call back

    private void Vibrate(VibrateEvent e)
    {
#if UNITY_ANDROID
        if (m_IsVibratorEnable && Application.platform == RuntimePlatform.Android)
        {
            Handheld.Vibrate();
        }
#endif
    }

#endregion
}
