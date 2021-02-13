using SDD.Events;

public class InGameEvents_TiltLeft : InGameEvents
{
    #region Constants

    private static readonly float REFRESH_DELAI = 0.5f;

    #endregion

    protected override void SubscribeEvents()
    {
        EventManager.Instance.AddListener<InputListenAnswerEvent>(InputListenAnswer);
    }

    protected override void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<InputListenAnswerEvent>(InputListenAnswer);
    }

    protected override void EventBegin()
    {
        EventManager.Instance.Raise(new InputListenRequestEvent(AssociatedCharacter.AssociedClientID,
            InputListenRequestEvent.Input.TILT_LEFT,
            EVENT_TIME,
            REFRESH_DELAI));
    }

    #region Event callback

    private void InputListenAnswer(InputListenAnswerEvent e)
    {

    }

    #endregion
}
