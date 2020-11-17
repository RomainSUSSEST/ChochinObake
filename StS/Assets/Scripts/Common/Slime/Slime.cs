using UnityEngine;

public abstract class Slime : MonoBehaviour
{
    // Constante

    private static readonly string TAG_OBSTACLE = "Obstacle";
    private static readonly string TAG_GROUND = "Ground";


    // Attributs

    [Header("Action")]

    [SerializeField] private float JumpForce;

    private SlimeHats Hat;
    private SlimeBody Body;

    private Animator HatAnimator;
    private Animator BodyAnimator;

    private Rigidbody rb;

    private bool LastAttackisRight;


    // Life Cycle

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(TAG_GROUND)) {
            BodyAnimator.SetBool("Jumping", false);
        }
    }


    // Requetes

    public SlimeHats GetSlimeHat()
    {
        return Hat;
    }

    public SlimeBody GetSlimeBody()
    {
        return Body;
    }

    protected Rigidbody GetRigidbody()
    {
        return rb;
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

    #region Animation Event Call back
    public void StartJumpPhysic()
    {
        rb.AddForce(new Vector3(0, JumpForce, 0), ForceMode.Impulse);
    }

    public void LiquefactionEnd()
    {
        BodyAnimator.SetBool("Liquefaction", false);
    }

    public void AttackEnd()
    {
        if (LastAttackisRight)
        {
            Body.TentaclesRotateLeft();
        } else
        {
            Body.TentaclesRotateRight();
        }

        BodyAnimator.SetBool("Attack", false);
    }
    #endregion


    // Outils

    protected void Jump()
    {
        BodyAnimator.SetBool("Jumping", true);
    }

    protected void Liquefaction()
    {
        BodyAnimator.SetBool("Liquefaction", true);
    }

    protected void AttackRight()
    {
        if (!BodyAnimator.GetBool("Attack"))
        {
            Body.TentaclesRotateRight();
            BodyAnimator.SetBool("Attack", true);
            LastAttackisRight = true;
        }
    }

    protected void AttackLeft()
    {
        if (!BodyAnimator.GetBool("Attack"))
        {
            Body.TentaclesRotateLeft();
            BodyAnimator.SetBool("Attack", true);
            LastAttackisRight = false;
        }
    }

    protected void DownAction()
    {
        if (IsOnAir())
        {
            rb.AddForce(new Vector3(0, -JumpForce, 0), ForceMode.Impulse); // On se propulse sur le sol
        } else
        {
            BodyAnimator.SetBool("Liquefaction", true);
        }
    }

    private bool IsOnAir()
    {
        return BodyAnimator.GetBool("Jumping");
    }
}
