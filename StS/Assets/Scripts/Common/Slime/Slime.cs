using UnityEngine;

public abstract class Slime : MonoBehaviour
{
    // Attributs

    private SlimeHats Hat;
    private SlimeBody Body;

    private Animator HatAnimator;
    private Animator BodyAnimator;


    // Requetes

    public SlimeHats GetSlimeHat()
    {
        return Hat;
    }

    public SlimeBody GetSlimeBody()
    {
        return Body;
    }


    // Méthode

    public void SetHat(SlimeHats hat)
    {
        // On détruit l'ancien chapeau

        if (Hat != null)
            Destroy(Hat.gameObject);

        // On crée et positionne le nouveau
        this.Hat = Instantiate(hat, transform, false);
        HatAnimator = Hat.GetComponent<Animator>();
        Hat.SetSlimeRoot(this);
    }

    public void SetBody(SlimeBody body)
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
