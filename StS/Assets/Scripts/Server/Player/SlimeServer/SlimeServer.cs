using SDD.Events;
using System.Collections.Generic;
using UnityEngine;

public class SlimeServer : Slime
{
    // Constante

    private static readonly float MARGIN_ERROR_S = 0.05f; // en % 0-1
    private static readonly float MARGIN_ERROR_A = 0.20f; // en % 0-1
    private static readonly float MARGIN_ERROR_B = 0.50f; // en % 0-1

    private enum Grade { S, A, B, C, NONE }


    // Attributs

    [Header("InputActionValidArea")]
    [SerializeField] private List<InputActionValidArea> ListInputActionValidArea;
    [SerializeField] private GameObject Spawn_InputActionValidArea;

    public ulong AssociedClientID { get; set; }

    private InputActionValidArea CurrentInputActionValidArea;
    private Animator InputActionValidArea_Animator;

    private Queue<Obstacle> QueueObstacle; // Queue des obstacles suivant associé à ce slime

    private float InputActionSize_Z_Per2; // La taille d'un input action sur l'axe Z divisé par 2


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
        foreach (InputActionValidArea input in ListInputActionValidArea) // On récupére la validArea associé.
        {
            if (input.GetAssociatedBody() == GetSlimeBody().GetBodyType())
            {
                CurrentInputActionValidArea = Instantiate(input,
                    Spawn_InputActionValidArea.transform);

                InputActionValidArea_Animator = CurrentInputActionValidArea.GetComponent<Animator>();

                break;
            }
        }

        // On initialise la Queue des obstacles associé
        QueueObstacle = new Queue<Obstacle>();

        // On récupére la taille sur Z d'un input action divisé par 2
        InputActionSize_Z_Per2 = CurrentInputActionValidArea.GetComponent<Renderer>().bounds.size.z / 2;
    }

    private void Update()
    {
        // Les obstacles ne doivent pas valoir null, puisqu'il se détruise loin derriere les slimes !
        // Si un obstacle enregistré à dépassé la position limite (sur Z), il est considéré comme raté.
        if (QueueObstacle.Count > 0 
            && QueueObstacle.Peek().transform.position.z < CurrentInputActionValidArea.transform.position.z - InputActionSize_Z_Per2)
        {
            DeregisterObstacle(); // On désenregistre l'obstacle.
            Debug.Log("Raté !");
        }
    }


    // Requete

    public Vector3 GetInputActionValidAreaPosition()
    {
        return Spawn_InputActionValidArea.transform.position;
    }

    public InputAction_Obstacle GetInputAction(InputActionValidArea.InputAction type)
    {
        return CurrentInputActionValidArea.GetInputAction(type);
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

    /// <summary>
    /// Permet d'enregistrer un obstacle aupres du slime
    /// </summary>
    /// <param name="obs"></param>
    public void RegisterObstacle(Obstacle obs)
    {
        QueueObstacle.Enqueue(obs);
    }


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
            InputPressed(InputActionValidArea.InputAction.SWIPE_TOP);
        }
    }

    private void SwipeRight(SwipeRightEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            InputPressed(InputActionValidArea.InputAction.SWIPE_RIGHT);
        }
    }

    private void SwipeLeft(SwipeLeftEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            InputPressed(InputActionValidArea.InputAction.SWIPE_LEFT);
        }
    }

    private void SwipeDown(SwipeDownEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            InputPressed(InputActionValidArea.InputAction.SWIPE_BOTTOM);
        }
    }

    private void DoublePress(DoublePressEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            InputPressed(InputActionValidArea.InputAction.DOUBLE_PRESS);
        }
    }
    #endregion

    /// <summary>
    /// Renvoie Grade 'NONE' -> Aucune collision avec l'obstacle
    /// Renvoie Grade 'S', 'A', 'B', 'C' -> Selon les seuil défini en constante pour noter la qualité
    /// du timing de l'input effectué
    /// </summary>
    /// <param name="obs"></param>
    /// <returns></returns>
    private Grade GetGrade(Obstacle obs)
    {
        float PosObs_Z = obs.transform.position.z; // Position de l'obstacle sur Z
        float PosInputValidArea_Z = CurrentInputActionValidArea.transform.position.z; // Position du référentiel sur Z

        // On test si il y a une collision
        float marg = InputActionSize_Z_Per2;
        if (PosInputValidArea_Z - marg <= PosObs_Z
            && PosObs_Z <= PosInputValidArea_Z + marg)
        {
            // On test si c'est un B
            marg = InputActionSize_Z_Per2 * MARGIN_ERROR_B;
            if (PosInputValidArea_Z - marg <= PosObs_Z 
                && PosObs_Z <= PosInputValidArea_Z + marg)
            {
                // On test si c'est un A
                marg = InputActionSize_Z_Per2 * MARGIN_ERROR_A;
                if (PosInputValidArea_Z - marg <= PosObs_Z
                    && PosObs_Z <= PosInputValidArea_Z + marg)
                {
                    // On test si c'est un S
                    marg = InputActionSize_Z_Per2 * MARGIN_ERROR_S;
                    if (PosInputValidArea_Z - marg <= PosObs_Z
                        && PosObs_Z <= PosInputValidArea_Z + marg)
                    {
                        return Grade.S;
                    } else
                    {
                        return Grade.A;
                    }
                } else
                {
                    return Grade.B;
                }
            } else
            {
                return Grade.C; // Note C
            }
        } else
        {
            return Grade.NONE; // Pas de collision
        }        
    }

    /// <summary>
    /// Gére le comportement du slime en cas d'input pressé par le joueur.
    /// </summary>
    /// <param name="action"> L'action effectué par le joueur </param>
    private void InputPressed(InputActionValidArea.InputAction action)
    {
        // On lance l'animation d'un input effectué
        InputActionValidArea_Animator.SetTrigger("InputTriggered");

        // S'il n'y a pas d'obstacle suivant
        if (QueueObstacle.Count == 0)
        {
            return;
        }

        Grade g = GetGrade(QueueObstacle.Peek());

        if (g != Grade.NONE) // On regarde si il y a collision
        {
            Obstacle obs = DeregisterObstacle(); // On retire le premier éléments
            
            if (obs.GetInput() == action) // Si les actions matchs
            {
                Debug.Log("Success ! " + g.ToString());
            } else
            {
                Debug.Log("Echec");
            }
        } else
        {
            Debug.Log("Trop tot !");
        }
    }

    /// <summary>
    /// Désenregistre le prochain obstacle
    /// </summary>
    private Obstacle DeregisterObstacle()
    {
        return QueueObstacle.Dequeue();
    }
}
