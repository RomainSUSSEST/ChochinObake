using ClientManager;
using CommonVisibleManager;
using SDD.Events;
using UnityEngine;

public class ClientMusicResultModel : MonoBehaviour
{
    #region Attributes

    [SerializeField] private GameObject DifficultiesPanel;

    #endregion

    #region Life cycle

    private void OnEnable()
    {
        SubscribeEvents();

        DifficultiesPanel.SetActive(true);
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    #endregion

    #region Event subs

    private void SubscribeEvents()
    {
        EventManager.Instance.AddListener<DifficultyVoteAcceptedEvent>(DifficultyVoteAccepted);
    }

    private void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<DifficultyVoteAcceptedEvent>(DifficultyVoteAccepted);
    }

    #endregion

    #region Events Call back

    private void DifficultyVoteAccepted(DifficultyVoteAcceptedEvent e)
    {
        DifficultiesPanel.SetActive(false);
    }

    #endregion


    #region OnClickButton

    public void EasyButtonHasBeenClicked()
    {
        MessagingManager.Instance.RaiseNetworkedEventOnServer(new EasyDifficultySelectedEvent(
            ClientNetworkManager.Instance.GetPlayerID().Value));
    }

    public void MediumButtonHasBeenClicked()
    {
        MessagingManager.Instance.RaiseNetworkedEventOnServer(new MediumDifficultySelectedEvent(
            ClientNetworkManager.Instance.GetPlayerID().Value));
    }

    public void HardButtonHasBeenClicked()
    {
        MessagingManager.Instance.RaiseNetworkedEventOnServer(new HardDifficultySelectedEvent(
            ClientNetworkManager.Instance.GetPlayerID().Value));
    }

    #endregion
}
