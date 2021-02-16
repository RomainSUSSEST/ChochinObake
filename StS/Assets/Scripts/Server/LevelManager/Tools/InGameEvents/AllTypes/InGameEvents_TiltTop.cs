using CommonVisibleManager;
using UnityEngine;

public class InGameEvents_TiltTop : InGameEvents_Tilt
{
    protected override void EventBegin()
    {
        Debug.Log("En haut !");
        MessagingManager.Instance.RaiseNetworkedEventOnClient(new InputListenRequestEvent(AssociatedCharacter.AssociedClientID,
            InputListenRequestEvent.Input.TILT_TOP,
            EVENT_TIME,
            REFRESH_DELAI));
    }
}
