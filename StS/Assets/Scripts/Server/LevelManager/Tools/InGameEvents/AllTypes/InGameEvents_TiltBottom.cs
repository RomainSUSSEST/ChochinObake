using CommonVisibleManager;
using UnityEngine;

public class InGameEvents_TiltBottom : InGameEvents_Tilt
{
    protected override void EventBegin()
    {
        Debug.Log("En bas !");
        MessagingManager.Instance.RaiseNetworkedEventOnClient(new InputListenRequestEvent(AssociatedCharacter.AssociedClientID,
            InputListenRequestEvent.Input.TILT_BOTTOM,
            EVENT_TIME,
            REFRESH_DELAI));
    }
}
