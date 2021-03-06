using SDD.Events;
using ServerManager;
using System.Collections.Generic;
using UnityEngine;

public class CharacterServer : CharacterPlayer
{
    // Attributs

    public ulong AssociedClientID { get; set; }

    private Queue<Obstacle> QueueObstacle; // Queue des obstacles suivant associé à ce slime

    private int LineIndex; // Index de la ligne ou se trouve le joueur

    #region Life Cycle

    private void Start()
    {
        // On initialise la Queue des obstacles associés
        QueueObstacle = new Queue<Obstacle>();

        GetCharacterBody().IsRunning(true); // On lance l'animation de course
    }

    private void Update()
    {
        if (QueueObstacle.Count > 0) // Si il y a un obstacle
        {
            Obstacle obs = QueueObstacle.Peek();
            if (obs == null) // Si celui-ci est null
            {
                DeregisterObstacle();
            } else
            {
                if (obs.GetStatut() == Obstacle.Statut.MISS)
                {
                    DeregisterObstacle(); // On désenregistre l'obstacle.

                    ObstacleMiss();
                }
            }
        }
    }

    #endregion


    #region Methods

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

    #region Override Triggered Attack

    public override void TriggeredAttackFire()
    {
        base.TriggeredAttackFire();

        TriggeredAttack(Obstacle.Elements.FIRE);
    }

    public override void TriggerAttackAir()
    {
        base.TriggerAttackAir();

        TriggeredAttack(Obstacle.Elements.AIR);
    }

    public override void TriggerAttackWater()
    {
        base.TriggerAttackWater();

        TriggeredAttack(Obstacle.Elements.WATER);
    }

    public override void TriggeredAttackEarth()
    {
        base.TriggeredAttackEarth();

        TriggeredAttack(Obstacle.Elements.EARTH);
    }

    #endregion

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
            GetCharacterBody().StartAttackFire();
        }
    }

    private void Earth(EarthEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            GetCharacterBody().StartAttackEarth();
        }
    }

    private void Water(WaterEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            GetCharacterBody().StartAttackWater();
        }
    }

    private void Air(AirEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID))
        {
            GetCharacterBody().StartAttackAir();
        }
    }

    #endregion

    /// <summary>
    /// Traite les actions d'attaque du joueur pour identifier les collisions.
    /// </summary>
    /// <param name="action"></param>
    private void TriggeredAttack(Obstacle.Elements action)
    {
        // S'il n'y a pas d'obstacle suivant
        if (QueueObstacle.Count == 0)
        {
            NoObstacles();
            return;
        }

        Obstacle obs = QueueObstacle.Peek();

        switch (obs.GetStatut())
        {
            case Obstacle.Statut.EARLY:
                ObstacleToEarly();
                break;
            case Obstacle.Statut.SUCCESS:
                DeregisterObstacle();

                if (obs.GetElement() == action)
                {
                    obs.DestroyKanji();
                    ObstacleSuccess();
                }
                else
                    ObstacleFail();

                break;
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

    private void NoObstacles()
    {

    }

    private void ObstacleMiss()
    {
        Debug.Log("Raté !");
    }

    private void ObstacleFail()
    {
        GetCharacterBody().Animation_AttackFailure();
        Debug.Log("Mauvaise Touche !");
    }

    private void ObstacleSuccess()
    {
        GetCharacterBody().Animation_AttackSuccess();
        Debug.Log("Réussite !");
    }

    private void ObstacleToEarly()
    {
        //Debug.Log("Trop tot !");
    }

    #endregion

    #endregion
}
