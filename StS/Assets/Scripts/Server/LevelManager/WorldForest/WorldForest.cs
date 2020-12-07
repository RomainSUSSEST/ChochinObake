using SDD.Events;
using ServerManager;
using System.Collections.Generic;
using UnityEngine;

public class WorldForest : MonoBehaviour
{
    // Constante

    public static readonly float DEFAULT_SPEED = 2.5f;
    private static readonly float DESTROY_MARGIN = 2;


    // Attributs

    [Header("Map Elements")]
    [SerializeField] private List<Obstacle> ListObstacle;
    [SerializeField] private List<Ground> ListGroundColor;
    [SerializeField] private Ground UnusedGround;

    [Header("Map Config")]
    [SerializeField] private float DistanceBetweenGround;

    [Header("SlimeServer")]
    [SerializeField] private SlimeServer SlimeServerPrefab;

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
    private SlimeServer[] SlimesArray; // SlimesArray[i] contient le SlimeServer à la wave i ou null.
    private Ground[] GroundArray; // Contient la prefab à instancier à la wave i.
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

        // On récupére la liste des joueurs
        Players = ServerGameManager.Instance.GetPlayers();

        // On vérifie les données
        if (CurrentMap == null || clip == null || Players == null)
        {
            throw new System.Exception("Donnée de carte invalide");
        }
        #endregion

        #region Variable

        GroundSize = UnusedGround.GetComponent<Renderer>().bounds.size; // On récupére la taille d'un des grounds
        NbrWaves = Mathf.Max(ServerLevelManager.MIN_NUMBER_WAVES, Players.Count); // Il y a un nombre de wave minimum
        StartWavesSpawnPosition_X = transform.position.x - (((GroundSize.x + DistanceBetweenGround) * NbrWaves) - DistanceBetweenGround) / 2;
        
        SlimesArray = new SlimeServer[NbrWaves];
        GroundArray = new Ground[NbrWaves];

        #endregion

        #region Array Player & Ground

        // On initialise les Array (player & ground)
        int nextValue = 0;

        float slimeSpawn_X = StartWavesSpawnPosition_X + GroundSize.x / 2;
        float slimeSpawn_Y = transform.position.y + GroundSize.y;

        IEnumerator<ulong> enumPlayer = Players.Keys.GetEnumerator(); // Enum sur les joueurs
        for (int i = (int) Mathf.Floor(NbrWaves / 2f); 0 <= i && i < NbrWaves; i += nextValue)
        {
            if (enumPlayer.MoveNext())
            {
                // Slime Array
                SlimesArray[i] = Instantiate(SlimeServerPrefab,
                    new Vector3(
                        slimeSpawn_X + (GroundSize.x + DistanceBetweenGround) * i,
                        slimeSpawn_Y,
                        transform.position.z), Quaternion.identity, transform); // On créer le slime

                SlimesArray[i].AssociedClientID = (ulong)enumPlayer.Current; // On récupére l'ulong du playerCourant

                // On récupére le Player associé
                Player p = Players[SlimesArray[i].AssociedClientID];

                // On set le hat et le body
                SlimesArray[i].SetHat(p.Hat);
                SlimesArray[i].SetBody(p.Body);

                // Ground Array

                // On cherche le ground qui correspond au slime body courant.
                foreach (Ground g in ListGroundColor)
                {
                    if (g.GetAssociatedSlimeBody() == p.Body)
                    {
                        GroundArray[i] = g;
                        break;
                    }
                }
            } else
            {
                SlimesArray[i] = null;
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

        // Initialisation des élements
        Ground.MOVE_SPEED = ((CurrentMap.Count / clip.length) * DEFAULT_SPEED);
        Ground.DESTROY_Z_POSITION = transform.position.z - GroundSize.z - DESTROY_MARGIN;

        // On initialise la carte
        InstantiateNewGroundsAt(new Vector3(StartWavesSpawnPosition_X, transform.position.y, transform.position.z));
        InstantiateNewGroundsAt(new Vector3(
             StartWavesSpawnPosition_X,
             LastGround.transform.position.y,
             LastGround.transform.position.z + GroundSize.z));
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

    #endregion
}
