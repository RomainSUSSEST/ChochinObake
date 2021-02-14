using CommonVisibleManager;

public class InGameEvents_TiltTop : InGameEvents_Tilt
{
    protected override void EventBegin()
    {
        MessagingManager.Instance.RaiseNetworkedEventOnClient(new InputListenRequestEvent(AssociatedCharacter.AssociedClientID,
            InputListenRequestEvent.Input.TILT_TOP,
            EVENT_TIME,
            REFRESH_DELAI));
    }
}
