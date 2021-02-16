using CommonVisibleManager;
using UnityEngine;

public class InGameEvents_TiltLeft : InGameEvents_Tilt
{
    protected override void EventBegin()
    {
        Debug.Log("A gauche !");
        MessagingManager.Instance.RaiseNetworkedEventOnClient(new InputListenRequestEvent(AssociatedCharacter.AssociedClientID,
            InputListenRequestEvent.Input.TILT_LEFT,
            EVENT_TIME,
            REFRESH_DELAI));
    }
}
