using CommonVisibleManager;
using SDD.Events;
using System.Collections;
using UnityEngine;

namespace ClientManager
{

    public class ClientInputsManager : ClientManager<ClientInputsManager>
    {
        #region Request

        public bool TiltLeft()
        {
            return true;
        }

        #endregion

        #region Manager implementation
        protected override IEnumerator InitCoroutine()
		{
			yield break;
		}
        #endregion

        #region Event subscription

        public override void SubscribeEvents()
        {
            base.SubscribeEvents();

            EventManager.Instance.AddListener<InputListenRequestEvent>(InputListenRequest);
        }

        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();

            EventManager.Instance.RemoveListener<InputListenRequestEvent>(InputListenRequest);
        }

        #endregion

        #region Event Callback

        private void InputListenRequest(InputListenRequestEvent e)
        {
            switch (e.Type)
            {
                case InputListenRequestEvent.Input.TILT_LEFT:
                    StartCoroutine(InputListenAnswer(e.During, e.RefreshDelai));
                    break;
                case InputListenRequestEvent.Input.TILT_BOTTOM:
                    StartCoroutine(InputListenAnswer(e.During, e.RefreshDelai));
                    break;
                case InputListenRequestEvent.Input.TILT_RIGHT:
                    StartCoroutine(InputListenAnswer(e.During, e.RefreshDelai));
                    break;
                default:
                    StartCoroutine(InputListenAnswer(e.During, e.RefreshDelai));
                    break;
            }
        }

        #endregion

        #region Coroutine

        private IEnumerator InputListenAnswer(float during, float refreshDelai)
        {
            float cmptTotalTime = 0;
            float cmptRefreshTime = 0;
            while (cmptTotalTime < during)
            {
                if (cmptRefreshTime >= refreshDelai)
                {
                    EventManager.Instance.Raise(
                        new InputListenAnswerEvent(
                            ClientNetworkManager.Instance.GetPlayerID().Value,
                            true));

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

        public void AirButtonHasBeenPressed()
        {
            MessagingManager.Instance.RaiseNetworkedEventOnServer(new AirEvent(ClientNetworkManager.Instance.GetPlayerID().Value));
        }

        public void WaterButtonHasBeenPressed()
        {
            MessagingManager.Instance.RaiseNetworkedEventOnServer(new WaterEvent(ClientNetworkManager.Instance.GetPlayerID().Value));
        }

        public void EarthButtonHasBeenPressed()
        {
            MessagingManager.Instance.RaiseNetworkedEventOnServer(new EarthEvent(ClientNetworkManager.Instance.GetPlayerID().Value));
        }

        #endregion
    }
}