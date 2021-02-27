using UnityEngine;

public abstract class CharacterPlayer : MonoBehaviour
{
    #region Attributs

    private CharacterBody Body;

    private Animator HatAnimator;
    private Animator BodyAnimator;

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

    public CharacterBody GetSlimeBody()
    {
        return Body;
    }

    #endregion


    #region Methods

    public void SetBody(CharacterBody body)
    {
        // On détruit l'ancien corps

        if (Body != null)
            Destroy(Body.gameObject);

        // On crée et positionne le nouveau
        this.Body = Instantiate(body, transform, false);
        BodyAnimator = Body.GetComponent<Animator>();
        Body.SetSlimeRoot(this);
    }

    #endregion

    #region Tools

    protected abstract void SubscribeEvents();

    protected abstract void UnsubscribeEvents();

    #endregion
}
