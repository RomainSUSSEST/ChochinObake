using SDD.Events;

public abstract class InGameEvents_Tilt : InGameEvents
{
    #region Constants

    protected static readonly float REFRESH_DELAI = 0.5f; // temps d'actualisation de la réponse.
    private static readonly float TARGET_TIME = 3; // Temps correcte à effectuer

    #endregion

    #region Attributes

    private float TotalValidTime; // Temps correcte actuellement effectué


    #endregion

    protected override void SubscribeEvents()
    {
        EventManager.Instance.AddListener<InputListenAnswerEvent>(InputListenAnswer);
    }

    protected override void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<InputListenAnswerEvent>(InputListenAnswer);
    }

    #region Event callback

    private void InputListenAnswer(InputListenAnswerEvent e)
    {
        if (e.Value)
        {
            TotalValidTime += REFRESH_DELAI;

            if (TotalValidTime >= TARGET_TIME)
            {
                EventSuccess();
            }
        }
    }

    #endregion
}
