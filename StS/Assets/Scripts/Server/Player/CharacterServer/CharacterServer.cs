using SDD.Events;
using ServerManager;
using System.Collections.Generic;
using UnityEngine;

public class CharacterServer : CharacterPlayer
{
    // Constante

    private static readonly float MARGIN_ERROR_S = 0.3f; // en % 0-1
    private static readonly float MARGIN_ERROR_A = 0.5f; // en % 0-1
    private static readonly float MARGIN_ERROR_B = 0.8f; // en % 0-1
    private static readonly float MARGIN_ERROR_C = 1f; // En %

    private enum Grade { S, A, B, C, NONE }


    // Attributs

    public ulong AssociedClientID { get; set; }

    private Queue<Obstacle> QueueObstacle; // Queue des obstacles suivant associé à ce slime

    private int LineIndex; // Index de la ligne ou se trouve le joueur

    #region Life Cycle

    private void Start()
    {
        // On initialise la Queue des obstacles associés
        QueueObstacle = new Queue<Obstacle>();
    }

    private void Update()
    {
        if (QueueObstacle.Count > 0) // Si il y a un obstacle
        {
            Obstacle obs = QueueObstacle.Peek();
            if (obs == null) // Si celui-ci est null
            {
                QueueObstacle.Dequeue();
            } else
            {
                // Si un obstacle enregistré à dépassé la position limite (sur Z), il est considéré comme raté.
                if (obs.transform.position.z < transform.position.z - obs.GetSizePer2_Z() * MARGIN_ERROR_A)
                {
                    DeregisterObstacle(); // On désenregistre l'obstacle.

                    ObstacleMiss();
                }
            }
        }
    }

    #endregion


    // Méthode

    #region Event subscription
    protected override void SubscribeEvents()
    {
        // ClientInputsManager
        EventManager.Instance.AddListener<FireEvent>(Fire);
        EventManager.Instance.AddListener<EarthEvent>(Earth);
        EventManager.Instance.AddListener<WaterEvent>(Water);
        EventManager.Instance.AddListener<AirEvent>(Air);

        EventManager.Instance.AddListener<ServerDisconnectionSuccessEvent>(OnClientDisconnected);
    }

    protected override void UnsubscribeEvents()
    {
        // ClientInputsManager
        EventManager.Instance.RemoveListener<FireEvent>(Fire);
        EventManager.Instance.RemoveListener<EarthEvent>(Earth);
        EventManager.Instance.RemoveListener<WaterEvent>(Water);
        EventManager.Instance.RemoveListener<AirEvent>(Air);

        EventManager.Instance.RemoveListener<ServerDisconnectionSuccessEvent>(OnClientDisconnected);
    }
    #endregion

    /// <summary>
    /// Permet d'enregistrer un obstacle aupres du slime
    /// </summary>
    /// <param name="obs"> l'obstacle à enregistrer </param>
    public void RegisterObstacle(Obstacle obs)
    {
        QueueObstacle.Enqueue(obs);
    }

    #region Line Index
    /// <summary>
    /// Indique au slime à quelle ligne il appartient, aucun effet de bord
    /// </summary>
    /// <param name="index"></param>
    public void SetLineIndex(int index)
    {
        LineIndex = index;
    }

    /// <summary>
    /// Si possible, le slime descend d'une ligne, lui et ses obstacles associé s'adapteront
    /// </summary>
    //public void DecreaseLineIndex()
    //{
    //    if (LineIndex > 0) // On vérifie que le minimum n'est pas déjà atteint.
    //    {
    //        --LineIndex;
    //        Vector3 add = new Vector3(0, 0, -ServerLevelManager.DISTANCE_BETWEEN_LINE);
    //        transform.Translate(add);

    //        // On resynchronise les obstacles existant

    //        Queue<Obstacle>.Enumerator i = QueueObstacle.GetEnumerator();
    //        while (i.MoveNext())
    //        {
    //            Obstacle obs = (Obstacle)i.Current;
    //            obs.transform.Translate(add);
    //        }
    //    }
    //}

    /// <summary>
    /// Si possible, le slime monte d'une ligne, lui et ses obstacles associé s'adappteront.
    /// </summary>
    public void IncreaseLineIndex()
    {
        if (LineIndex < ServerLevelManager.NBR_LINE) // On vérifie que le maximum n'est pas déjà atteint.
        {
            ++LineIndex;
            Vector3 add = new Vector3(0, 0, ServerLevelManager.DISTANCE_BETWEEN_LINE);
            transform.Translate(add);

            // On resynchronise les obstacles existant

            Queue<Obstacle>.Enumerator i = QueueObstacle.GetEnumerator();
            while (i.MoveNext())
            {
                Obstacle obs = (Obstacle)i.Current;
                obs.transform.Translate(add);
            }
        }
    }
    #endregion

    #region Tools

    #region EventCallBack
    private void OnClientDisconnected(ServerDisconnectionSuccessEvent e)
    {
        if (e.ClientID == this.AssociedClientID)
        {
            Destroy(this.gameObject);
        }
    }

    private void Fire(FireEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            InputPressed(Obstacle.Elements.FIRE);
        }
    }

    private void Earth(EarthEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            InputPressed(Obstacle.Elements.EARTH);
        }
    }

    private void Water(WaterEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            InputPressed(Obstacle.Elements.WATER);
        }
    }

    private void Air(AirEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            InputPressed(Obstacle.Elements.AIR);
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
    private Grade GetGrade(Obstacle obs) // TODO
    {
        float PosObs_Z = obs.transform.position.z; // Position de l'obstacle sur Z
        float PosCharacter_Z = transform.position.z; // Position du joueur sur Z

        // On test si il y a une collision
        float marg = obs.GetSizePer2_Z() * MARGIN_ERROR_C;
        if (PosObs_Z - marg <= PosCharacter_Z
            && PosCharacter_Z <= PosObs_Z + marg)
        {
            // On test si c'est un B
            marg = obs.GetSizePer2_Z() * MARGIN_ERROR_B;
            if (PosObs_Z - marg <= PosCharacter_Z
                && PosCharacter_Z <= PosObs_Z + marg)
            {
                // On test si c'est un A
                marg = obs.GetSizePer2_Z() * MARGIN_ERROR_A;
                if (PosObs_Z - marg <= PosCharacter_Z
                    && PosCharacter_Z <= PosObs_Z + marg)
                {
                    // On test si c'est un S
                    marg = obs.GetSizePer2_Z() * MARGIN_ERROR_S;
                    if (PosObs_Z - marg <= PosCharacter_Z
                        && PosCharacter_Z <= PosObs_Z + marg)
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
    private void InputPressed(Obstacle.Elements action)
    {
        // S'il n'y a pas d'obstacle suivant
        if (QueueObstacle.Count == 0)
        {
            return;
        }

        Grade g = GetGrade(QueueObstacle.Peek());

        if (g != Grade.NONE) // On regarde si il y a collision
        {
            Obstacle obs = DeregisterObstacle(); // On retire le premier éléments

            if (obs.GetElement() == action) // Si les actions matchs
            {
                ObstacleSuccess(g);
            } else
            {
                // Mauvaise touche
                ObstacleFail();
            }
        } else
        {
            // Trop tot
            ObstacleToEarly();
        }
    }

    /// <summary>
    /// Désenregistre le prochain obstacle
    /// </summary>
    private Obstacle DeregisterObstacle()
    {
        return QueueObstacle.Dequeue();
    }

    #region ObstacleStatus

    private void ObstacleMiss()
    {
        Debug.Log("Raté !");
    }

    private void ObstacleFail()
    {
        Debug.Log("Mauvaise Touche !");
    }

    private void ObstacleSuccess(Grade g)
    {
        Debug.Log(g);
    }

    private void ObstacleToEarly()
    {
        Debug.Log("Trop tot !");
    }

    #endregion

    #endregion
}
