using UnityEngine;

public abstract class CharacterPlayer : MonoBehaviour
{
    // Attributs

    private CharacterHats Hat;
    private CharacterBody Body;

    private Animator HatAnimator;
    private Animator BodyAnimator;


    // Requetes

    public CharacterHats GetSlimeHat()
    {
        return Hat;
    }

    public CharacterBody GetSlimeBody()
    {
        return Body;
    }


    // Méthode

    public void SetHat(CharacterHats hat)
    {
        // On détruit l'ancien chapeau

        if (Hat != null)
            Destroy(Hat.gameObject);

        // On crée et positionne le nouveau
        this.Hat = Instantiate(hat, transform, false);
        HatAnimator = Hat.GetComponent<Animator>();
        Hat.SetSlimeRoot(this);
    }

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
}
