using ClientManager;
using CommonVisibleManager;
using SDD.Events;
using UnityEngine;

public class MusicSelectionClientModel : MonoBehaviour
{
    // Attributs

    private string[] MusicList;


    #region Life cycle

    private void OnEnable()
    {
        SubscribeEvents(); // On s'abonne au différent event

        AskServerForMusicList(); // On demande la liste des chansons au serveur
        RefreshPanelMusicList(); // On refresh la liste des chansons.
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    #endregion

    #region Event call back

    private void AnswerForMusicListRequest(AnswerForMusicListRequestEvent e)
    {
        MusicList = e.MusicList;
    }

    #endregion


    // Outils

    /// <summary>
    /// Permet de demander au serveur la liste des chansons
    /// </summary>
    private void AskServerForMusicList()
    {
        if (ClientNetworkManager.Instance.GetPlayerID() == null)
        {
            throw new System.Exception("");
        }

        MessagingManager.Instance.RaiseNetworkedEventOnServer(new AskForMusicListEvent(ClientNetworkManager.Instance.GetPlayerID().Value));
    }

    private void RefreshPanelMusicList()
    {

    }


    #region Event Subs

    private void SubscribeEvents()
    {
        // Networked Event

        EventManager.Instance.AddListener<AnswerForMusicListRequestEvent>(AnswerForMusicListRequest);
    }

    private void UnsubscribeEvents()
    {
        // Networked Event

        EventManager.Instance.RemoveListener<AnswerForMusicListRequestEvent>(AnswerForMusicListRequest);
    }

    #endregion
}
