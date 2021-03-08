using CommonVisibleManager;
using SDD.Events;
using ServerManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterServer : CharacterPlayer
{
    #region Constants

    private static readonly float UPDATE_POSITION_TIME = 1.5f; // En seconde
    private static readonly float FIRST_UPDATE_POSITION_TIME = 3f; // en s

    #endregion

    #region Attributes

    public ulong AssociedClientID { get; set; }
    public bool IsSafe { get; set; }

    private Queue<Obstacle> QueueObstacle; // Queue des obstacles suivant associé à ce slime

    private int CmptSuccess;
    private int CmptObstacle;
    private int CmptCombo;

    private Coroutine LastUpdatePositionCoroutine;

    #region Malus

    private bool IsSleeping;

    #endregion

    #endregion

    #region Life Cycle

    private void Start()
    {
        // On initialise la Queue des obstacles associés
        QueueObstacle = new Queue<Obstacle>();

        GetCharacterBody().IsRunning(true); // On lance l'animation de course

        UpdatePosition(FIRST_UPDATE_POSITION_TIME);
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

                    UpdatePosition(UPDATE_POSITION_TIME);
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
        EventManager.Instance.AddListener<PowerEvent>(Power);

        EventManager.Instance.AddListener<ServerDisconnectionSuccessEvent>(OnClientDisconnected);
    }

    protected override void UnsubscribeEvents()
    {
        // ClientInputsManager
        EventManager.Instance.RemoveListener<FireEvent>(Fire);
        EventManager.Instance.RemoveListener<EarthEvent>(Earth);
        EventManager.Instance.RemoveListener<WaterEvent>(Water);
        EventManager.Instance.RemoveListener<PowerEvent>(Power);

        EventManager.Instance.RemoveListener<ServerDisconnectionSuccessEvent>(OnClientDisconnected);
    }
    #endregion

    /// <summary>
    /// Permet d'enregistrer un obstacle aupres du joueur
    /// </summary>
    /// <param name="obs"> l'obstacle à enregistrer </param>
    public void RegisterObstacle(Obstacle obs)
    {
        QueueObstacle.Enqueue(obs);
    }

    #region Override Triggered Attack

    public override void TriggeredAttackFire()
    {
        base.TriggeredAttackFire();

        TriggeredAttack(Obstacle.Elements.FIRE);
    }

    public override void TriggerAttackPower()
    {
        base.TriggerAttackPower();

        EventManager.Instance.Raise(new PowerDeclenchementEvent()
        {
            CharacterServer = this,
            CmptCombo = CmptCombo
        });
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

    #region Combo

    //public void SubstractCombo(int n)
    //{
    //    CmptCombo -= n;
    //    MessagingManager.Instance.RaiseNetworkedEventOnClient(new UpdateSuccessiveSuccessEvent(AssociedClientID, CmptCombo));
    //}

    public void ResetCombo()
    {
        CmptCombo = 0;
        // Mise à jours des combo auprès du joueur.
        MessagingManager.Instance.RaiseNetworkedEventOnClient(new UpdateSuccessiveSuccessEvent(AssociedClientID, CmptCombo));
    }

    #endregion

    #region Malus

    public void Sleep(float delai)
    {
        StartCoroutine("Sleeping", delai);
    }

    private IEnumerator Sleeping(float delai)
    {
        IsSleeping = true;

        yield return new WaitForSeconds(delai);

        IsSleeping = false;
    }

    #endregion

    #endregion

    #region Couroutines

    private IEnumerator UpdateZPosition(float delai)
    {
        // On détruit l'ancienne coroutine
        if (LastUpdatePositionCoroutine != null)
            StopCoroutine(LastUpdatePositionCoroutine);

        // On calcul les positions
        float ZStart = transform.localPosition.z;
        float ZEnd;

        if (CmptObstacle == 0)
        {
            ZEnd = Mathf.Lerp(0, ServerLevelManager.MAXIMUM_ADVANCE_DISTANCE, 1);
        } else
        {
            ZEnd = Mathf.Lerp(0, ServerLevelManager.MAXIMUM_ADVANCE_DISTANCE, (float)CmptSuccess / CmptObstacle);
            Debug.Log(CmptSuccess + " " + CmptObstacle);
            Debug.Log((float)CmptSuccess / CmptObstacle);
        }   

        float time = 0;
        float distance;
        while (time < delai)
        {
            float newZPos = Mathf.Lerp(ZStart, ZEnd, time / delai);
            distance = newZPos - transform.localPosition.z;

            // Mise à jours de la position du joueur
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, newZPos);

            // Mise à jours de la position de ces obstacles
            foreach (Obstacle obs in QueueObstacle)
            {
                obs.transform.position = new Vector3(
                    obs.transform.localPosition.x,
                    obs.transform.localPosition.y,
                    obs.transform.localPosition.z + distance);
            }

            yield return new CoroutineTools.WaitForFrames(1);
            time += Time.deltaTime;
        }

        distance = ZEnd - transform.localPosition.z;

        // Position final du joueur
        transform.position = new Vector3(transform.position.x, transform.localPosition.y, ZEnd);

        // Position final des obstacles
        foreach (Obstacle obs in QueueObstacle)
        {
            obs.transform.position = new Vector3(
                obs.transform.localPosition.x,
                obs.transform.localPosition.y,
                obs.transform.localPosition.z + distance);
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
        if (e.DoesThisConcernMe(AssociedClientID) && !IsSleeping)
        {
            GetCharacterBody().StartAttackFire();
        }
    }

    private void Earth(EarthEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID) && !IsSleeping)
        {
            GetCharacterBody().StartAttackEarth();
        }
    }

    private void Water(WaterEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID) && !IsSleeping)
        {
            GetCharacterBody().StartAttackWater();
        }
    }

    private void Power(PowerEvent e)
    {
        if (e.DoesThisConcernMe(AssociedClientID) && !IsSleeping)
        {
            GetCharacterBody().StartAttackPower();
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

        UpdatePosition(UPDATE_POSITION_TIME);
    }

    /// <summary>
    /// Actualise la position du joueur sur le plateau
    /// </summary>
    private void UpdatePosition(float delai)
    {
        LastUpdatePositionCoroutine = StartCoroutine("UpdateZPosition", delai);
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
        if (CmptCombo > 0)
            CmptCombo = 0;
        else
            --CmptCombo;

        ++CmptObstacle;

        MessagingManager.Instance.RaiseNetworkedEventOnClient(new UpdateSuccessiveSuccessEvent(AssociedClientID, CmptCombo));
    }

    private void ObstacleFail()
    {
        GetCharacterBody().Animation_AttackFailure();

        if (CmptCombo > 0)
            CmptCombo = 0;
        else
            --CmptCombo;

        ++CmptObstacle;

        MessagingManager.Instance.RaiseNetworkedEventOnClient(new UpdateSuccessiveSuccessEvent(AssociedClientID, CmptCombo));
    }

    private void ObstacleSuccess()
    {
        GetCharacterBody().Animation_AttackSuccess();

        if (CmptCombo <= 0)
        {
            CmptCombo = 1;
        } else
        {
            ++CmptCombo;
        }

        ++CmptSuccess;
        ++CmptObstacle;

        MessagingManager.Instance.RaiseNetworkedEventOnClient(new UpdateSuccessiveSuccessEvent(AssociedClientID, CmptCombo));
    }

    private void ObstacleToEarly()
    {
        //Debug.Log("Trop tot !");
    }

    #endregion

    #endregion
}
