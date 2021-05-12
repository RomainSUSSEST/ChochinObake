using ClientManager;
using CommonVisibleManager;
using SDD.Events;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelInGameModel : MonoBehaviour
{
    #region Attributes

    [Header("Bonus Icons")]
    [SerializeField] private Image bonusImageDefault;
    [SerializeField] private Image bonusImageRevert;

    // Bonus icons
    [SerializeField] Sprite ResetAllCombo;
    [SerializeField] Sprite Shield;
    [SerializeField] Sprite InvertKanji;
    [SerializeField] Sprite UncolorKanji;
    [SerializeField] Sprite FlashKanji;
    [SerializeField] Sprite InvertInputKanji;
    [SerializeField] Sprite DisableOtherPlayers;
    [SerializeField] Sprite Default;

    #region Malus
    [Header("Gameplay Panels")]
    [SerializeField] private GameObject DefaultPanel;
    [SerializeField] private GameObject InvertPanel;

    #endregion


    #endregion

    #region Life Cycle

    private void OnEnable()
    {
        // Initialisation
        DefaultPanel.SetActive(true);
        InvertPanel.SetActive(false);

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

        // Malus
        EventManager.Instance.AddListener<InvertInputEvent>(InvertInput);
        EventManager.Instance.AddListener<StopInvertInputEvent>(StopInvertInput);
    }

    private void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<InputListenRequestEvent>(InputListenRequest);
        EventManager.Instance.RemoveListener<UpdateSuccessiveSuccessEvent>(UpdateSuccessiveSuccess);

        // Malus
        EventManager.Instance.RemoveListener<InvertInputEvent>(InvertInput);
        EventManager.Instance.RemoveListener<StopInvertInputEvent>(StopInvertInput);
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
        switch (e.Value)
        {
            case UpdateSuccessiveSuccessEvent.BonusStreak.ResetAllCombo:
                bonusImageDefault.sprite = ResetAllCombo;
                bonusImageRevert.sprite = ResetAllCombo;
                break;
            case UpdateSuccessiveSuccessEvent.BonusStreak.Shield:
                bonusImageDefault.sprite = Shield;
                bonusImageRevert.sprite = Shield;
                break;
            case UpdateSuccessiveSuccessEvent.BonusStreak.InvertKanji:
                bonusImageDefault.sprite = InvertKanji;
                bonusImageRevert.sprite = InvertKanji;
                break;
            case UpdateSuccessiveSuccessEvent.BonusStreak.UncolorKanji:

                bonusImageDefault.sprite = UncolorKanji;
                bonusImageRevert.sprite = UncolorKanji;
                break;
            case UpdateSuccessiveSuccessEvent.BonusStreak.FlashKanji:
                bonusImageDefault.sprite = FlashKanji;
                bonusImageRevert.sprite = FlashKanji;
                break;
            case UpdateSuccessiveSuccessEvent.BonusStreak.InvertInput:
                bonusImageDefault.sprite = InvertInputKanji;
                bonusImageRevert.sprite = InvertInputKanji;
                break;
            default:
                bonusImageDefault.sprite = Default;
                bonusImageRevert.sprite = Default;
                break;
        }
    }

    #region Malus

    private void InvertInput(InvertInputEvent e)
    {
        DefaultPanel.SetActive(false);
        InvertPanel.SetActive(true);
    }

    private void StopInvertInput(StopInvertInputEvent e)
    {
        DefaultPanel.SetActive(true);
        InvertPanel.SetActive(false);
    }

    #endregion

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
