using CommonVisibleManager;
using UnityEngine;

public class InGameEvents_TiltRight : InGameEvents_Tilt
{
    protected override void EventBegin()
    {
        Debug.Log("A droite !");
        MessagingManager.Instance.RaiseNetworkedEventOnClient(new InputListenRequestEvent(AssociatedCharacter.AssociedClientID,
            InputListenRequestEvent.Input.TILT_RIGHT,
            EVENT_TIME,
            REFRESH_DELAI));
    }
}
