using CommonVisibleManager;

public class InGameEvents_TiltLeft : InGameEvents_Tilt
{
    protected override void EventBegin()
    {
        MessagingManager.Instance.RaiseNetworkedEventOnClient(new InputListenRequestEvent(AssociatedCharacter.AssociedClientID,
            InputListenRequestEvent.Input.TILT_LEFT,
            EVENT_TIME,
            REFRESH_DELAI));
    }
}
