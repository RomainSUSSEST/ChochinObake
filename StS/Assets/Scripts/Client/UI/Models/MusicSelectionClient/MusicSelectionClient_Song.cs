using ClientManager;
using CommonVisibleManager;
using TMPro;
using UnityEngine;

public class MusicSelectionClient_Song : MonoBehaviour
{
    // Attributs

    [Header("Song info")]

    [SerializeField] private TextMeshProUGUI songTitle;


    // Méthode

    public void SetTitle(string title)
    {
        songTitle.text = title;
    }


    // Onclick button Event

    public void VoteButtonHasBeenClicked()
    {
        if (ClientNetworkManager.Instance.GetPlayerID() == null) {
            throw new System.Exception();
        }

        MessagingManager.Instance.RaiseNetworkedEventOnServer(
            new VoteButtonHasBeenClickedEvent(
                ClientNetworkManager.Instance.GetPlayerID().Value, songTitle.text));
    }
}
