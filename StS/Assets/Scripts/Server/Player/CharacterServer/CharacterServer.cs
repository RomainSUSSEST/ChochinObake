using CommonVisibleManager;
using SDD.Events;
using ServerManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterServer : CharacterPlayer
{
    #region Constants

    private static readonly float UPDATE_POSITION_TIME = 1.5f; // En seconde
    private static readonly float FIRST_UPDATE_POSITION_TIME = 3f; // en s
    private static readonly float VIBRATION_DELAI = 250;

    #endregion

    #region Attributes

    public bool IsAI { get; set; }
    public ulong AssociedClientID { get; set; }
    public AIPlayer AssociatedAIManager { get; set; }

    #region Stats

    private int PowerUse;
    private int BestCombo;

    #endregion

    private Queue<Obstacle> QueueObstacle; // Queue des obstacles suivant associé à ce character

    private int CmptSuccess; // Nombre de succès
    private int CmptObstacle; // Obstacle passé
    private int CmptCombo; // Nombre courant de combos

    private bool PowerLocked; // Renvoie si un pouvoir est entrain d'etre utilisé.
    private int PowerCmptCombo; // Nombre de combo enregistré lors du lancement d'un pouvoir
    private List<CharacterServer> CurrentTargets; // Les targets indiqué par le levelmanager

    private bool m_IsSafe; // Si le joueur est safe aux pouvoirs des autres joueurs

    [SerializeField] private GameObject ShieldPrefab;
    private GameObject CurrentShield; // Eventuel shield actuel

    [Header("Effect")]
    [SerializeField] private Effect m_UsePower;
    [SerializeField] private Effect m_InvertKanji;
    [SerializeField] private Effect m_UncolorKanji;
    [SerializeField] private Effect m_FlashKanji;

    [SerializeField] private Transform LightProjectilesTarget;

    [Header("Rank Position")]

    [SerializeField] private List<Sprite> AllRankImages;
    [SerializeField] private Image RankImage;

    private Coroutine LastUpdatePositionCoroutine;

    #region Malus

    private bool IsSleeping;
    private Coroutine Sleeping_Couroutine;

    private Coroutine FlashKanji_Coroutine;

    #endregion

    #endregion

    #region Life Cycle

    private void Start()
    {
        // On initialise la Queue des obstacles associés
        QueueObstacle = new Queue<Obstacle>();

        GetCharacterBody().IsRunning(true); // On lance l'animation de course

        UpdatePosition(FIRST_UPDATE_POSITION_TIME);
        ResetCombo();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // On détruit tous les obstacles associés
        Obstacle o;
        while (QueueObstacle.Count > 0)
        {
            Destroy(QueueObstacle.Dequeue().gameObject);
        }

        // On met à jours le player profil
        if (IsAI)
        {
            AI_Player ai = AssociatedAIManager.GetAssociatedProfil();

            ai.LastGameBestCombo = GetBestCombo();
            ai.LastGameLanternSuccess = GetTotalSuccess();
            ai.LastGamePowerUse = GetPowerUse();
            ai.LastGameTotalLantern = GetTotalObstacle();

        } else
        {
            Player p = ServerGameManager.Instance.GetPlayers()[AssociedClientID];

            p.LastGameBestCombo = GetBestCombo();
            p.LastGameLanternSuccess = GetTotalSuccess();
            p.LastGamePowerUse = GetPowerUse();
            p.LastGameTotalLantern = GetTotalObstacle();
        }
    }

    #endregion

    #region Request

    public bool IsSafe()
    {
        return m_IsSafe;
    }

    public int GetTotalSuccess()
    {
        return CmptSuccess;
    }

    public int GetTotalObstacle()
    {
        return CmptObstacle;
    }

    public List<CharacterServer> GetCurrentTargets()
    {
        return CurrentTargets;
    }

    public Transform GetLightProjectilesTarget()
    {
        return LightProjectilesTarget;
    }

    public int GetBestCombo()
    {
        return BestCombo;
    }

    public int GetPowerUse()
    {
        return PowerUse;
    }

    #endregion

    #region Methods

    #region Event subscription
    protected override void SubscribeEvents()
    {
        EventManager.Instance.AddListener<MusicRoundEndEvent>(MusicRoundEnd);
        
        if (!IsAI)
        {
            // ClientInputsManager
            EventManager.Instance.AddListener<FireEvent>(Fire);
            EventManager.Instance.AddListener<EarthEvent>(Earth);
            EventManager.Instance.AddListener<WaterEvent>(Water);
            EventManager.Instance.AddListener<PowerEvent>(Power);

            EventManager.Instance.AddListener<ServerDisconnectionSuccessEvent>(OnClientDisconnected);
        }
    }

    protected override void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<MusicRoundEndEvent>(MusicRoundEnd);

        if (!IsAI)
        {
            // ClientInputsManager
            EventManager.Instance.RemoveListener<FireEvent>(Fire);
            EventManager.Instance.RemoveListener<EarthEvent>(Earth);
            EventManager.Instance.RemoveListener<WaterEvent>(Water);
            EventManager.Instance.RemoveListener<PowerEvent>(Power);

            EventManager.Instance.RemoveListener<ServerDisconnectionSuccessEvent>(OnClientDisconnected);
        }
    }
    #endregion

    #region Obstacles

    /// <summary>
    /// Permet d'enregistrer un obstacle aupres du joueur
    /// </summary>
    /// <param name="obs"> l'obstacle à enregistrer </param>
    public void RegisterObstacle(Obstacle obs)
    {
        QueueObstacle.Enqueue(obs);
    }

    public void ObstacleSendMissTime()
    {
        DeregisterObstacle(); // On désenregistre l'obstacle.

        if (CmptCombo > 0)
            CmptCombo = 0;
        else
            --CmptCombo;

        ++CmptObstacle;

        UpdateStreakStatus(CmptCombo);

        UpdatePosition(UPDATE_POSITION_TIME);
        Vibrate();
    }

    public void ObstacleSendSuccessTime(Obstacle.Elements element)
    {
        if (IsAI)
            AssociatedAIManager.SuccessTime(element);
    }

    #endregion

    public void SetSafeStatus(bool b)
    {
        m_IsSafe = b;

        if (m_IsSafe)
        {
            if (CurrentShield == null)
            {
                CurrentShield = Instantiate(ShieldPrefab, this.transform);
            }
        }
        else if (CurrentShield != null)
        {
            Destroy(CurrentShield.gameObject);
        }
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

        if (PowerLocked)
        {
            return;
        }

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

    public void ResetCombo()
    {
        CmptCombo = 0;

        UpdateStreakStatus(CmptCombo);
    }

    #endregion

    #region Power Lock

    public void LockPowerInput()
    {
        PowerLocked = true;
    }

    public void UnLockPowerInput()
    {
        PowerLocked = false;
    }

    #endregion

    #region Malus

    public void Sleep(float delai)
    {
        if (Sleeping_Couroutine != null)
        {
            StopCoroutine(Sleeping_Couroutine);
        }

        Sleeping_Couroutine = StartCoroutine("Sleeping", delai);
    }

    private IEnumerator Sleeping(float delai)
    {
        IsSleeping = true;

        yield return new WaitForSeconds(delai);

        IsSleeping = false;
    }

    public void InvertInput(float delai)
    {
        if (!IsAI)
        {
            MessagingManager.Instance.RaiseNetworkedEventOnClient(new InvertInputEvent(AssociedClientID, delai));
        } else
        {
            AssociatedAIManager.InvertInput(delai);
        }
    }

    public void FlashKanji(float delai, float delaiInterFlash)
    {
        UseFlashKanjiEffect();

        if (FlashKanji_Coroutine != null)
        {
            StopCoroutine(FlashKanji_Coroutine);
        }

        FlashKanji_Coroutine = StartCoroutine(_FlashKanji(delai, delaiInterFlash));

        if (IsAI)
        {
            AssociatedAIManager.FlashKanji(delai);
        }
    }

    private IEnumerator _FlashKanji(float delai, float delaiInterFlash)
    {
        float time = 0;
        bool status = false;
        while (time < delai)
        {
            foreach (Obstacle o in QueueObstacle)
            {
                o.SetKanjiRendererStatus(status);
            }

            yield return new WaitForSeconds(delaiInterFlash);

            time += delaiInterFlash;
            status = !status;
        }

        // On s'assure que l'on fini par réactiver les kanjis
        foreach (Obstacle o in QueueObstacle)
        {
            o.SetKanjiRendererStatus(true);
        }
    }

    public void UncolorKanji()
    {
        UseUncolorKanjiEffect();

        foreach (Obstacle o in QueueObstacle)
        {
            o.UncolorKanji();
        }

        if (IsAI)
        {
            AssociatedAIManager.UncolorKanji(QueueObstacle.Count);
        }
    }

    /// <summary>
    /// Essai d'inverser les kanji du joueur
    /// Renvoie true en cas de succès, false en cas d'échec
    /// </summary>
    public void InvertKanji()
    {
        if (QueueObstacle.Count >= 2)
        {
            UseInvertKanjiEffect();
            Obstacle[] currentObstacle = QueueObstacle.ToArray();

            Obstacle tampon = currentObstacle[0];
            Vector3 tampon2 = currentObstacle[0].transform.position;

            currentObstacle[0].transform.position = currentObstacle[1].transform.position;
            currentObstacle[0] = currentObstacle[1];

            currentObstacle[1].transform.position = tampon2;
            currentObstacle[1] = tampon;
            

            // On reforme la queue
            QueueObstacle = new Queue<Obstacle>();

            foreach (Obstacle o in currentObstacle)
            {
                QueueObstacle.Enqueue(o);
            }

        } else
        {
            StartCoroutine("_InvertKanji");
        }
    }

    #endregion

    #region AI

    public void AIFire()
    {
        GetCharacterBody().StartAttackFire();
    }

    public void AIEarth()
    {
        GetCharacterBody().StartAttackEarth();
    }

    public void AIWater()
    {
        GetCharacterBody().StartAttackWater();
    }

    public void AIPower()
    {
       GetCharacterBody().StartAttackPower();
    }

    #endregion

    #region Effect_Animation

    public void UsePowerEffect(int CmptCombo, List<CharacterServer> targets)
    {
        m_UsePower.gameObject.SetActive(true);
        PowerCmptCombo = CmptCombo;
        CurrentTargets = targets;
    }

    public void PowerStart()
    {
        EventManager.Instance.Raise(new PowerStartEvent()
        {
            Targets = CurrentTargets,
            CmptCombo = PowerCmptCombo,
            Player = this
        });

        PowerUse++;
        PowerCmptCombo = 0;
        CurrentTargets = null;
    }

    private void UseInvertKanjiEffect()
    {
        m_InvertKanji.gameObject.SetActive(true);
    }

    private void UseUncolorKanjiEffect()
    {
        m_UncolorKanji.gameObject.SetActive(true);
    }

    private void UseFlashKanjiEffect()
    {
        m_FlashKanji.gameObject.SetActive(true);
    }

    #endregion

    #region Rank

    /// <summary>
    /// Commence à 0
    /// </summary>
    /// <param name="rank"></param>
    public void SetRank(int rank)
    {
        RankImage.sprite = AllRankImages[rank];
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

    /// <summary>
    /// Fixe le parent de this à newParent après delai seconde
    /// </summary>
    /// <param name="t"></param>
    /// <param name="delai"></param>
    /// <returns></returns>
    private IEnumerator SetParent(Transform newParent, float delai)
    {
        yield return new WaitForSeconds(delai);

        transform.parent = newParent;
    }

    private IEnumerator _InvertKanji()
    {
        while (QueueObstacle.Count < 2)
        {
            yield return new WaitForSeconds(0.5f);
        }

        InvertKanji();
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

    private void MusicRoundEnd(MusicRoundEndEvent e)
    {
        float delai = (e.TransformArrival.position.z - transform.position.z) / Ground.MOVE_SPEED;
        StartCoroutine(SetParent(e.TransformArrival, delai));
    }

    #endregion

    private void SetPowerLogo(Sprite logo)
    {
        GetCharacterBody().SetPowerLogo(logo);
    }

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

        switch (obs.GetStatut()) // On regarde le status de l'obstacle
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
                {
                    obs.EchecObstacle();
                    ObstacleFail();
                }

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

    private void UpdateStreakStatus(int cmptCombo)
    {
        // On cherche le pouvoir correspondant
        ServerLevelManager.Power power;

        if (cmptCombo <= (int)ServerLevelManager.Power.ResetAllCombo)
        {
            power = ServerLevelManager.Power.ResetAllCombo;
        }
        else if (cmptCombo >= (int)ServerLevelManager.Power.InvertInput)
        {
            power = ServerLevelManager.Power.InvertInput;
        }
        else if (cmptCombo >= (int)ServerLevelManager.Power.FlashKanji)
        {
            power = ServerLevelManager.Power.FlashKanji;
        }
        else if (cmptCombo >= (int)ServerLevelManager.Power.UncolorKanji)
        {
            power = ServerLevelManager.Power.UncolorKanji;
        }
        else if (cmptCombo >= (int)ServerLevelManager.Power.InvertKanji)
        {
            power = ServerLevelManager.Power.InvertKanji;
        }
        else
        {
            power = ServerLevelManager.Power.NoPower;
        }

        // On met à jours auprès du joueur
        if (IsAI)
        {
            AssociatedAIManager.SetCmptCombo(cmptCombo);
        } else
        {
            UpdateSuccessiveSuccessEvent.BonusStreak bonusValue;

            switch (power)
            {
                case ServerLevelManager.Power.FlashKanji:
                    bonusValue = UpdateSuccessiveSuccessEvent.BonusStreak.FlashKanji;
                    break;
                case ServerLevelManager.Power.InvertInput:
                    bonusValue = UpdateSuccessiveSuccessEvent.BonusStreak.InvertInput;
                    break;
                case ServerLevelManager.Power.InvertKanji:
                    bonusValue = UpdateSuccessiveSuccessEvent.BonusStreak.InvertKanji;
                    break;
                case ServerLevelManager.Power.NoPower:
                    bonusValue = UpdateSuccessiveSuccessEvent.BonusStreak.Default;
                    break;
                case ServerLevelManager.Power.ResetAllCombo:
                    bonusValue = UpdateSuccessiveSuccessEvent.BonusStreak.ResetAllCombo;
                    break;
                default:// ServerLevelManager.Power.UncolorKanji:
                    bonusValue = UpdateSuccessiveSuccessEvent.BonusStreak.UncolorKanji;
                    break;
            }

            MessagingManager.Instance.RaiseNetworkedEventOnClient(new UpdateSuccessiveSuccessEvent(AssociedClientID, bonusValue));
        }

        SetPowerLogo(ServerLevelManager.Instance.GetAssociatedSprite(power)); // On met à jours l'icone joueur in game
    }

    private void Vibrate()
    {
        if (!IsAI)
            MessagingManager.Instance.RaiseNetworkedEventOnClient(new VibrateEvent(AssociedClientID, VIBRATION_DELAI));
    }

    #region ObstacleStatus

    private void NoObstacles()
    {

    }

    private void ObstacleFail()
    {
        GetCharacterBody().Animation_AttackFailure();

        if (CmptCombo > 0)
            CmptCombo = 0;
        else
            --CmptCombo;

        ++CmptObstacle;

        UpdateStreakStatus(CmptCombo);
        Vibrate();
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

        if (CmptCombo > BestCombo)
        {
            BestCombo = CmptCombo;
        }

        UpdateStreakStatus(CmptCombo);
    }

    private void ObstacleToEarly()
    {
        //Debug.Log("Trop tot !");
    }

    #endregion

    #endregion
}
