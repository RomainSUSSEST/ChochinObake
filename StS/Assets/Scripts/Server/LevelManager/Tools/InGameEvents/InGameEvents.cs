using System.Collections;
using UnityEngine;

public abstract class InGameEvents : MonoBehaviour
{
    #region Constants

    public static readonly float EVENT_TIME = 5; // Durée d'un évènement

    #endregion

    #region Attributes

    protected CharacterServer AssociatedCharacter; // Joueur concerné par l'évènement
    private bool IsSuccess; // Si l'évènement est réussis à l'instant T

    #endregion

    #region Life Cycle

    private IEnumerator Start()
    {
        SubscribeEvents();
        EventBegin();

        yield return new WaitForSeconds(EVENT_TIME);

        if (IsSuccess)
        {
            // Reussis
        } else
        {
            // Echec
        }

        UnsubscribeEvents();
        Destroy(this.gameObject);
    }

    #endregion

    #region Event subscription

    protected abstract void SubscribeEvents();

    protected abstract void UnsubscribeEvents();

    #endregion

    #region Méthods

    public void SetAssociatedCharacter(CharacterServer c)
    {
        AssociatedCharacter = c;
    }

    protected abstract void EventBegin();

    protected void EventSuccess()
    {
        IsSuccess = true;
    }

    #endregion
}
