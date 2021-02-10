using CommonVisibleManager;
using System.Collections;

namespace ClientManager
{

    public class ClientInputsManager : ClientManager<ClientInputsManager>
    {
		#region Manager implementation
		protected override IEnumerator InitCoroutine()
		{
			yield break;
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