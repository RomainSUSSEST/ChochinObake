using SDD.Events;
using System.Collections.Generic;
using UnityEngine;

public class SlimeServer : Slime
{
    // Attributs

    [Header("InputActionValidArea")]
    [SerializeField] private List<InputActionValidArea> ListInputActionValidArea;
    [SerializeField] private GameObject Spawn_InputActionValidArea;

    public ulong AssociedClientID { get; set; }

    private InputActionValidArea CurrentInputActionValidArea;
    private Animator InputActionValidArea_Animator;


    // Life cycle

    private void Awake()
    {
        SubscribeEvent();
    }

    private void OnDestroy()
    {
        UnsubscribeEvent();
    }

    private void Start()
    {
        foreach (InputActionValidArea input in ListInputActionValidArea)
        {
            if (input.GetAssociatedBody() == GetSlimeBody().GetBodyType())
            {
                CurrentInputActionValidArea = Instantiate(input,
                    Spawn_InputActionValidArea.transform);

                InputActionValidArea_Animator = CurrentInputActionValidArea.GetComponent<Animator>();

                break;
            }
        }
    }


    // Requete

    public Vector3 GetInputActionValidAreaPosition()
    {
        return Spawn_InputActionValidArea.transform.position;
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
            InputActionValidArea_Animator.SetTrigger("InputTriggered");
        }
    }

    private void SwipeRight(SwipeRightEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            InputActionValidArea_Animator.SetTrigger("InputTriggered");
        }
    }

    private void SwipeLeft(SwipeLeftEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            InputActionValidArea_Animator.SetTrigger("InputTriggered");
        }
    }

    private void SwipeDown(SwipeDownEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            InputActionValidArea_Animator.SetTrigger("InputTriggered");
        }
    }

    private void DoublePress(DoublePressEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            InputActionValidArea_Animator.SetTrigger("InputTriggered");
        }
    }
    #endregion
}
