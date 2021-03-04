using UnityEngine;

public abstract class CharacterPlayer : MonoBehaviour
{
    #region Attributs

    private CharacterBody Body;

    #endregion

    #region Life Cycle

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

    /// <summary>
    /// Renvoie le body nouvellement ajouté
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public CharacterBody SetBody(CharacterBody body)
    {
        // On détruit l'ancien corps
        if (Body != null)
            Destroy(Body.gameObject);

        // On crée et positionne le nouveau
        this.Body = Instantiate(body, transform, false);
        this.Body.SetAssociatedCharacterPlayer(this);

        return Body;
    }

    #region Trigger Attack

    public virtual void TriggeredAttackFire()
    {

    }

    public virtual void TriggeredAttackEarth()
    {

    }

    public virtual void TriggerAttackAir()
    {

    }

    public virtual void TriggerAttackWater()
    {

    }

    #endregion

    #endregion

    #region Tools

    protected abstract void SubscribeEvents();

    protected abstract void UnsubscribeEvents();

    #endregion
}
