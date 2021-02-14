using CommonVisibleManager;

public class InGameEvents_TiltRight : InGameEvents_Tilt
{
    protected override void EventBegin()
    {
        MessagingManager.Instance.RaiseNetworkedEventOnClient(new InputListenRequestEvent(AssociatedCharacter.AssociedClientID,
            InputListenRequestEvent.Input.TILT_RIGHT,
            EVENT_TIME,
            REFRESH_DELAI));
    }
}
