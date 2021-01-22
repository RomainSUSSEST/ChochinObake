using SDD.Events;
using ServerManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe mère des mondes, permet de générer les joueurs et les obstacles.
/// 
/// Leur comportement est ensuite autonome.
/// </summary>
public abstract class World : MonoBehaviour
{
    // Attributs

    [Header("Map Elements")]
    [SerializeField] private List<Obstacle> ListObstacle;
    [SerializeField] private List<Ground> ListGroundColor;
    [SerializeField] private Ground UnusedGround;

    [Header("Map Config")]
    [SerializeField] private Camera MainCamera;

    [SerializeField] private float DistanceBetweenGround = 5.7f;
    [SerializeField] private float ObstacleDistanceToSlimeSpawn;

    [Header("SlimeServer")]
    [SerializeField] private CharacterServer CharacterServerPrefab;

    // Information sur la carte
    private System.Collections.ObjectModel.ReadOnlyCollection<SpectralFluxInfo> CurrentMap;
    private IReadOnlyDictionary<ulong, Player> Players;

    #region Gestion des grounds
    private Ground LastGround; // Dernier ground spawné
    private Vector3 GroundSize; // Taille des grounds
    private int NbrWaves; // Nombre de waves

    private float StartWavesSpawnPosition_X; // Position d'apparition des waves

    private int CmptWavesHasBeenDestroyed; // Nombre de wave détruite depuis le dernier spawn
    #endregion

    #region Wave Array

    private CharacterServer[] CharacterArray; // CharacterArray[i] contient le Character à la wave i ou null.
    private Ground[] GroundArray; // Contient la prefab à instancier à la wave i.

    #endregion

    #region Obstacle

    private float CurrentMaxPrunedSpectralFlux;
    private float CurrentSensitivity;
    private float CurrentThresholdSensitivity;
    private float AheadTimeToSpawn;

    #endregion

    #region Life Cycle

    private void Awake()
    {
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void Start()
    {
        #region MapInfo

        // On récupére les informations de la carte.
        CurrentMap = ServerGameManager.Instance.GetCurrentMapData();
        AudioClip clip = ServerGameManager.Instance.GetCurrentAudioClip();
        CurrentSensitivity = Mathf.Lerp(
            ServerLevelManager.ALGO_MIN_SENSITIVITY,
            ServerLevelManager.ALGO_MAX_SENSITIVITY,
            ServerGameManager.Instance.GetCurrentDifficulty());

        // On récupére la liste des joueurs
        Players = ServerGameManager.Instance.GetPlayers();

        // On vérifie les données
        if (CurrentMap == null || clip == null || Players == null)
        {
            throw new System.Exception("Donnée de carte invalide");
        }
        #endregion

        #region Ground & Player

        #region Variable

        GroundSize = UnusedGround.GetComponent<Renderer>().bounds.size; // On récupére la taille d'un des grounds
        NbrWaves = Mathf.Max(ServerLevelManager.MIN_NUMBER_WAVES, Players.Count); // Il y a un nombre de wave minimum
        StartWavesSpawnPosition_X = transform.position.x - (((GroundSize.x + DistanceBetweenGround) * NbrWaves) - DistanceBetweenGround) / 2;
        
        CharacterArray = new CharacterServer[NbrWaves];
        GroundArray = new Ground[NbrWaves];

        #endregion

        #region Array Player & Ground initialization

        // On initialise les Array (player & ground)
        int nextValue = 0;

        float slimeSpawn_X = StartWavesSpawnPosition_X + GroundSize.x / 2;
        float slimeSpawn_Y = transform.position.y + GroundSize.y;

        IEnumerator<ulong> enumPlayer = Players.Keys.GetEnumerator(); // Enum sur les joueurs

        int index = NbrWaves % 2 == 0 ? (NbrWaves / 2)-1 : (int)Mathf.Floor(NbrWaves / 2f);

        for (int i = index; 0 <= i && i < NbrWaves; i += nextValue)
        {
            if (enumPlayer.MoveNext())
            {
                // Line index
                int lineIndex = (int)Mathf.Ceil(ServerLevelManager.NBR_LINE_MAX * ServerLevelManager.LINE_SLIME_SPAWN);

                // Slime Array
                CharacterArray[i] = Instantiate(CharacterServerPrefab,
                    new Vector3(
                        slimeSpawn_X + (GroundSize.x + DistanceBetweenGround) * i,
                        slimeSpawn_Y,
                        transform.position.z + lineIndex * ServerLevelManager.DISTANCE_BETWEEN_LINE),
                    Quaternion.identity, transform); // On créer le slime

                CharacterArray[i].AssociedClientID = (ulong)enumPlayer.Current; // On récupére l'ulong du playerCourant

                // On récupére le Player associé
                Player p = Players[CharacterArray[i].AssociedClientID];

                // On set le hat et le body
                CharacterArray[i].SetHat(p.Hat);
                CharacterArray[i].SetBody(p.Body);
                CharacterArray[i].SetLineIndex(lineIndex);

                // Ground Array

                // On cherche le ground qui correspond au slime body courant.
                foreach (Ground g in ListGroundColor)
                {
                    if (g.GetAssociatedSlimeBody() == p.Body.GetBodyType())
                    {
                        GroundArray[i] = g;
                        break;
                    }
                    
                }
            } else
            {
                CharacterArray[i] = null;
                GroundArray[i] = UnusedGround;
            }

            if (nextValue <= 0)
            {
                nextValue = (nextValue - 1) * -1;
            } else
            {
                nextValue = (nextValue + 1) * -1;
            }
        }

        #endregion

        // Initialisation des constantes pour les élements
        Ground.MOVE_SPEED = Mathf.Min(Mathf.Max(((CurrentMap.Count / clip.length) * ServerLevelManager.DEFAULT_SPEED), ServerLevelManager.MIN_SPEED), ServerLevelManager.MAX_SPEED);
        Ground.DESTROY_Z_POSITION = transform.position.z - GroundSize.z * 3 / 2;

        #region On Initialise la carte (Génération)
        InstantiateNewGroundsAt(new Vector3(StartWavesSpawnPosition_X, transform.position.y, transform.position.z));
        InstantiateNewGroundsAt(new Vector3(
             StartWavesSpawnPosition_X,
             LastGround.transform.position.y,
             LastGround.transform.position.z + GroundSize.z));
        #endregion
        #endregion

        //#region Camera

        //InitializeCamera();

        //#endregion

        #region Obstacle

        // On récupére le MaxPrunedSpectralFlux
        CurrentMaxPrunedSpectralFlux = 0;
        foreach (SpectralFluxInfo sf in CurrentMap)
        {
            if (sf.prunedSpectralFlux > CurrentMaxPrunedSpectralFlux)
            {
                CurrentMaxPrunedSpectralFlux = sf.prunedSpectralFlux;
            }
        }

        CurrentThresholdSensitivity = CurrentMaxPrunedSpectralFlux * CurrentSensitivity; // On calcul la valeur de seuil minimal de la sensitivité de l'algo

        #region Variable (AHeadTimeToSpawn)

        // On calcul le temps d'avance que doivent prendre les obstacles pour apparaitre à ObstacleDistanceToSpawn
        // du spawn des slimes et tj arriver au moment du beats.
        AheadTimeToSpawn = ObstacleDistanceToSlimeSpawn / Ground.MOVE_SPEED;

        #endregion

        // On lance la musique après aHeadTimeToSpawn
        StartCoroutine("StartMusic");
        // On lance l'algo procédural des obstacles
        StartCoroutine("ProceduralGenerator");

        StartCoroutine("ASuppr"); // Génére un son lorsque c'est le moment de l'obstacle
        #endregion
    }

    #endregion

    #region Coroutine

    /// <summary>
    /// Lance la musique du round après AheadTimeToSpawn
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartMusic()
    {
        yield return new WaitForSeconds(AheadTimeToSpawn);

        ServerMusicManager.Instance.StartRoundMusic();
    }

    /// <summary>
    /// Parcourt les beats de la musique pour générer les obstacles
    /// </summary>
    /// <returns></returns>
    private IEnumerator ProceduralGenerator()
    {
        int cpt = 0;
        float time = 0; // Compteur de temps
        while (cpt < CurrentMap.Count)
        {
            while (cpt < CurrentMap.Count &&
                CurrentMap[cpt].time <= time)
            {
                AddObstacle(CurrentMap[cpt]);
                ++cpt;
            }

            yield return new CoroutineTools.WaitForFrames(1);
            time += Time.deltaTime; // On ajoute le temps passé
        }
    }

    private IEnumerator ASuppr()
    {
        int cpt = 0;
        float time = -AheadTimeToSpawn; // Compteur de temps
        while (cpt < CurrentMap.Count)
        {
            while (cpt < CurrentMap.Count &&
                CurrentMap[cpt].time <= time)
            {
                if (CurrentMap[cpt].prunedSpectralFlux > CurrentThresholdSensitivity)
                {
                    SfxManager.Instance.PlaySfx2D("Balloon");
                }
                    
                ++cpt;
            }

            yield return new CoroutineTools.WaitForFrames(1);
            time += Time.deltaTime; // On ajoute le temps passé
        }
    }

    #endregion

    #region Subscribe Event

    private void SubscribeEvents()
    {
        EventManager.Instance.AddListener<GroundEndMapEvent>(GroundEndMap);
    }

    private void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GroundEndMapEvent>(GroundEndMap);
    }

    #endregion

    #region Event Call backs

    private void GroundEndMap(GroundEndMapEvent e)
    {
        if (++CmptWavesHasBeenDestroyed >= NbrWaves)
        {
            InstantiateNewGroundsAt(new Vector3(
                StartWavesSpawnPosition_X,
                LastGround.transform.position.y,
                LastGround.transform.position.z + GroundSize.z));

            CmptWavesHasBeenDestroyed = 0;
        }
    }

    #endregion

    #region Tools

    /// <summary>
    /// Instantie une rangée de wave à la position StartPosition
    /// Effet de bord : Update LastGround
    /// </summary>
    /// <param name="StartPosition"></param>
    private void InstantiateNewGroundsAt(Vector3 StartPosition)
    {
        for (int i = 0; i < NbrWaves; ++i)
        {
            LastGround = Instantiate(GroundArray[i], new Vector3(
                StartPosition.x + (GroundSize.x + DistanceBetweenGround) * i,
                StartPosition.y,
                StartPosition.z),
                Quaternion.identity, transform);
        }
    }

    /// <summary>
    /// Renvoie true si un obstacle est ajouté, false sinon
    /// </summary>
    /// <param name="flux"></param>
    /// <returns></returns>
    private bool AddObstacle(SpectralFluxInfo flux)
    {
        // Si le beats n'est pas ignoré
        if (flux.prunedSpectralFlux > CurrentThresholdSensitivity)
        {
            // On cherche à combien de pourcentage du maximum correspond le beats.
            float curPercentOfMax = (flux.prunedSpectralFlux - CurrentThresholdSensitivity)
                / (CurrentMaxPrunedSpectralFlux - CurrentThresholdSensitivity); // Max : 1

            // On obtient une valeur entre 0 et 1 que l'on multiplie au nombre d'obstacle - 1
            int index = (int) Mathf.Round(curPercentOfMax * (ListObstacle.Count - 1));

            Obstacle curObstacle;
           // Pour chaque slime, on invoque l'obstacle
           foreach (CharacterServer ss in CharacterArray)
            {
                if (ss != null) {
                    Vector3 pos = ss.GetInputActionValidAreaPosition();

                    // On instantie l'obstacle à ObstacleDistanceToSlimeSpawn de InputActionValidAreaPosition
                    curObstacle = Instantiate(ListObstacle[index], new Vector3(
                        pos.x,
                        pos.y,
                        pos.z + ObstacleDistanceToSlimeSpawn),
                        Quaternion.identity,
                        transform);

                    curObstacle.SetAssociatedSlime(ss); // On associe le character à l'obstacle
                }
            }
            return true;
        }
        return false;
    }

    //private void InitializeCamera()
    //{
    //    float t = NbrWaves / ServerNetworkManager.MAX_PLAYER_CONNECTED;

    //    MainCamera.orthographicSize = Mathf.Lerp(
    //        ServerLevelManager.MIN_CAMERA_SIZE,
    //        ServerLevelManager.MAX_CAMERA_SIZE,
    //        t);

    //    MainCamera.transform.position = new Vector3(
    //        MainCamera.transform.position.x,
    //        Mathf.Lerp(
    //            ServerLevelManager.MIN_CAMERA_YPOS,
    //            ServerLevelManager.MAX_CAMERA_YPOS,
    //            t),
    //        MainCamera.transform.position.z);

    //    MainCamera.transform.rotation = Quaternion.Euler
    //        (
    //            Mathf.Lerp(
    //                ServerLevelManager.MIN_CAMERA_XROT,
    //                ServerLevelManager.MAX_CAMERA_XROT,
    //                t),
    //            MainCamera.transform.rotation.y,
    //            MainCamera.transform.rotation.z
    //        );
    //}

    #endregion
}