using ClientManager;
using CommonVisibleManager;
using SDD.Events;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PanelInGameModel : MonoBehaviour
{
    #region Attributes

    [SerializeField] private TextMeshProUGUI text;

    #endregion

    #region Life Cycle

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    #endregion

    #region Request

    public bool TiltLeft()
    {
        return Input.acceleration.x < 0;
    }

    public bool TiltRight()
    {
        return Input.acceleration.x > 0;
    }

    public bool TiltFront()
    {
        return Input.acceleration.z < 0;
    }

    public bool TiltBack()
    {
        return Input.acceleration.z > 0;
    }

    #endregion

    #region Event subscription

    private void SubscribeEvents()
    {
        EventManager.Instance.AddListener<InputListenRequestEvent>(InputListenRequest);
        EventManager.Instance.AddListener<UpdateSuccessiveSuccessEvent>(UpdateSuccessiveSuccess);
    }

    private void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<InputListenRequestEvent>(InputListenRequest);
        EventManager.Instance.RemoveListener<UpdateSuccessiveSuccessEvent>(UpdateSuccessiveSuccess);
    }

    #endregion

    #region Event Callback

    private void InputListenRequest(InputListenRequestEvent e)
    {
        switch (e.Type)
        {
            case InputListenRequestEvent.Input.TILT_LEFT:
                StartCoroutine(InputListenAnswer(e.During, e.RefreshDelai, TiltLeft));
                break;
            case InputListenRequestEvent.Input.TILT_BACK:
                StartCoroutine(InputListenAnswer(e.During, e.RefreshDelai, TiltBack));
                break;
            case InputListenRequestEvent.Input.TILT_RIGHT:
                StartCoroutine(InputListenAnswer(e.During, e.RefreshDelai, TiltRight));
                break;
            default:
                StartCoroutine(InputListenAnswer(e.During, e.RefreshDelai, TiltFront));
                break;
        }
    }

    private void UpdateSuccessiveSuccess(UpdateSuccessiveSuccessEvent e)
    {
        text.text = e.Value.ToString();
    }

    #endregion

    #region Coroutine

    private IEnumerator InputListenAnswer(float during, float refreshDelai, Func<bool> condition)
    {
        float cmptTotalTime = 0;
        float cmptRefreshTime = 0;
        while (cmptTotalTime <= during) // Tant que le temps cible n'est pas atteint
        {
            if (cmptRefreshTime >= refreshDelai) // Si on doit actualiser
            {
                MessagingManager.Instance.RaiseNetworkedEventOnServer(
                    new InputListenAnswerEvent(
                        ClientNetworkManager.Instance.GetPlayerID().Value,
                        condition()));

                cmptRefreshTime -= refreshDelai;
            }

            yield return new CoroutineTools.WaitForFrames(1);

            cmptRefreshTime += Time.deltaTime;
            cmptTotalTime += Time.deltaTime;
        }
    }

    #endregion

    #region Inputs methods

    public void FireButtonHasBeenPressed()
    {
        MessagingManager.Instance.RaiseNetworkedEventOnServer(new FireEvent(ClientNetworkManager.Instance.GetPlayerID().Value));
    }

    public void WaterButtonHasBeenPressed()
    {
        MessagingManager.Instance.RaiseNetworkedEventOnServer(new WaterEvent(ClientNetworkManager.Instance.GetPlayerID().Value));
    }

    public void EarthButtonHasBeenPressed()
    {
        MessagingManager.Instance.RaiseNetworkedEventOnServer(new EarthEvent(ClientNetworkManager.Instance.GetPlayerID().Value));
    }

    public void PowerButtonhasBeenPressed()
    {
        MessagingManager.Instance.RaiseNetworkedEventOnServer(new PowerEvent(ClientNetworkManager.Instance.GetPlayerID().Value));
    }

    #endregion
}
