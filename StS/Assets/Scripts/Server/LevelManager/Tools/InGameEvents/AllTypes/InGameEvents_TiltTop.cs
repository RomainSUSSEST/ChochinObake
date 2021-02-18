using CommonVisibleManager;
using UnityEngine;

public class InGameEvents_TiltTop : InGameEvents_Tilt
{
    protected override void EventBegin()
    {
        Debug.Log("Devant !");
        MessagingManager.Instance.RaiseNetworkedEventOnClient(new InputListenRequestEvent(AssociatedCharacter.AssociedClientID,
            InputListenRequestEvent.Input.TILT_FRONT,
            EVENT_TIME,
            REFRESH_DELAI));
    }
}
