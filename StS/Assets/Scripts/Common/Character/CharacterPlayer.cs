using UnityEngine;

public abstract class CharacterPlayer : MonoBehaviour
{
    #region Attributs

    private CharacterBody Body;

    #endregion

    #region Life Cycle

    // Life cycle

    private void Awake()
    {
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    #endregion


    #region Requests

    public CharacterBody GetCharacterBody()
    {
        return Body;
    }

    #endregion


    #region Methods

    public CharacterBody SetBody(CharacterBody body)
    {
        // On détruit l'ancien corps
        if (Body != null)
            Destroy(Body.gameObject);

        // On crée et positionne le nouveau
        return this.Body = Instantiate(body, transform, false);
    }

    #endregion

    #region Tools

    protected abstract void SubscribeEvents();

    protected abstract void UnsubscribeEvents();

    #endregion
}
