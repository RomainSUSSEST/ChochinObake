using SDD.Events;
using UnityEngine;

public class SlimeServer : Slime
{
    // Constante

    [SerializeField] private float Speed;

    private Vector3 Respawn;


    // Attributs

    public ulong AssociedClientID { get; set; }

    private float MovementIntensityX;
    private float MovementIntensityZ;


    // Life cycle

    private void Awake()
    {
        SubscribeEvent();
    }

    private void OnDestroy()
    {
        UnsubscribeEvent();
    }

    private void FixedUpdate()
    {
        // On applique le changement de direction

        GetRigidbody().MovePosition(new Vector3(
            transform.position.x + MovementIntensityX * Time.fixedDeltaTime,
            transform.position.y,
            transform.position.z - MovementIntensityZ * Time.fixedDeltaTime));
    }

    protected override void Start()
    {
        base.Start();
        Respawn = transform.position;
    }


    // Méthode

    #region Event subscription
    private void SubscribeEvent()
    {
        // ClientInputsManager
        EventManager.Instance.AddListener<SwipeUpEvent>(SwipeUp);
        EventManager.Instance.AddListener<SwipeLeftEvent>(SwipeLeft);
        EventManager.Instance.AddListener<SwipeRightEvent>(SwipeRight);
        EventManager.Instance.AddListener<SwipeDownEvent>(SwipeDown);

        EventManager.Instance.AddListener<DoublePressEvent>(DoublePress);

        EventManager.Instance.AddListener<TiltLeftRightEvent>(TiltLeftRight);
        EventManager.Instance.AddListener<TiltTopBottomEvent>(TiltTopBottom);

        EventManager.Instance.AddListener<ServerDisconnectionSuccessEvent>(OnClientDisconnected);
    }

    private void UnsubscribeEvent()
    {
        // ClientInputsManager
        EventManager.Instance.RemoveListener<SwipeUpEvent>(SwipeUp);
        EventManager.Instance.RemoveListener<SwipeLeftEvent>(SwipeLeft);
        EventManager.Instance.RemoveListener<SwipeRightEvent>(SwipeRight);
        EventManager.Instance.RemoveListener<SwipeDownEvent>(SwipeDown);

        EventManager.Instance.RemoveListener<DoublePressEvent>(DoublePress);

        EventManager.Instance.RemoveListener<TiltLeftRightEvent>(TiltLeftRight);
        EventManager.Instance.RemoveListener<TiltTopBottomEvent>(TiltTopBottom);

        EventManager.Instance.RemoveListener<ServerDisconnectionSuccessEvent>(OnClientDisconnected);
    }
    #endregion


    // Outils

    #region EventCallBack
    private void OnClientDisconnected(ServerDisconnectionSuccessEvent e)
    {
        if (e.ClientID == this.AssociedClientID)
        {
            Destroy(this.gameObject);
        }
    }

    private void SwipeUp(SwipeUpEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            Jump();
        }
    }

    private void SwipeRight(SwipeRightEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            AttackRight();
        }
    }

    private void SwipeLeft(SwipeLeftEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            AttackLeft();
        }
    }

    private void SwipeDown(SwipeDownEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            DownAction();
        }
    }

    private void DoublePress(DoublePressEvent e)
    {
    }

    private void TiltLeftRight(TiltLeftRightEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            MovementIntensityX = e.intensity * Speed;
        }
    }

    private void TiltTopBottom(TiltTopBottomEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            MovementIntensityZ = e.intensity * Speed;
        }
    }

    #endregion
}
