using UnityEngine;
using ServerManager;

public class LightProjectiles : MonoBehaviour
{
    #region Attributes

    [SerializeField] private float m_Time;

    [Space]
    [Header("PROJECTILE PATH")]
    private float randomUpAngle;
    private float randomSideAngle;
    [SerializeField] private float sideAngle = 18;
    [SerializeField] private float upAngle = 18;

    public Transform Target;
    public CharacterServer AssociatedCharacter;

    private float speed;

    #endregion

    #region Life Cycle

    private void Start()
    {
        speed = Vector3.Distance(Target.position, transform.position) / m_Time;
        newRandom();
    }

    private void Update()
    {
        Vector3 forward = Target.position - transform.position;
        Vector3 crossDirection = Vector3.Cross(forward, Vector3.up);
        Quaternion randomDeltaRotation = Quaternion.Euler(0, randomSideAngle, 0) * Quaternion.AngleAxis(randomUpAngle, crossDirection);
        Vector3 direction = randomDeltaRotation * forward;

        float distanceThisFrame = Time.deltaTime * speed;

        if (direction.magnitude <= distanceThisFrame)
        {
            Destroy(gameObject);
            AssociatedCharacter.PowerStart();

            if (AssociatedCharacter.IsAI)
            {
                SfxManager.Instance.AIPlaySfx(AssociatedCharacter.AssociatedAIManager.GetAssociatedProfil().Name, SfxManager.Instance.FireworksExplosion);
            } else
            {
                SfxManager.Instance.PlayerPlaySfx(AssociatedCharacter.AssociedClientID, SfxManager.Instance.FireworksExplosion);
            }

            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        transform.rotation = Quaternion.LookRotation(direction);
    }

    #endregion

    #region Tools

    private void newRandom()
    {
        randomUpAngle = Random.Range(0, upAngle);
        randomSideAngle = Random.Range(-sideAngle, sideAngle);
    }

    #endregion
}
