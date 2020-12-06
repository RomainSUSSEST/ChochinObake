using SDD.Events;
using ServerManager;
using System.Collections.Generic;
using UnityEngine;

public class WorldForest : MonoBehaviour
{
    // Constante

    public static readonly float DEFAULT_SPEED = 2.5f;


    // Attributs

    [Header("Map Elements")]
    [SerializeField] private Ground DefaultGround;
    [SerializeField] private List<Obstacle> ListObstacle;

    [Header("Map Config")]
    [SerializeField] private float DistanceBetweenGround;

    [Header("Camera")]
    [SerializeField] private Camera Camera;

    private System.Collections.ObjectModel.ReadOnlyCollection<SpectralFluxInfo> CurrentMap;
    private IReadOnlyDictionary<ulong, Player> Players;

    private Ground LastGround;
    private Vector3 GroundSize;
    private int NbrWaves;

    private int CmptWavesHasBeenDestroyed;


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

        // On récupére la tailles des grounds
        GroundSize = DefaultGround.GetComponent<Renderer>().bounds.size;

        // Initialisation des élements
        Ground.MOVE_SPEED = ((CurrentMap.Count / clip.length) * DEFAULT_SPEED);
        Ground.DESTROY_Z_POSITION = transform.position.z - GroundSize.z;


        // On initialise la carte

        NbrWaves = Mathf.Max(ServerLevelManager.MIN_NUMBER_WAVES, Players.Count); // Il y a un nombre de wave minimum
        InstantiateNewGroundsAt(transform.position);
        InstantiateNewGroundsAt(new Vector3(
             LastGround.transform.position.x,
             LastGround.transform.position.y,
             LastGround.transform.position.z + GroundSize.z));

        // On intialise la position de la camera au milieu des waves
        Camera.transform.position = new Vector3(
            (transform.position.x + LastGround.transform.position.x) / 2,
            Camera.transform.position.y,
            Camera.transform.position.z);
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
                LastGround.transform.position.x,
                LastGround.transform.position.y,
                LastGround.transform.position.z + GroundSize.z));

            CmptWavesHasBeenDestroyed = 0;
        }
    }

    #endregion

    #region Tools

    /// <summary>
    /// Effet de bord : Update LastGround
    /// </summary>
    /// <param name="StartPosition"></param>
    private void InstantiateNewGroundsAt(Vector3 StartPosition)
    {
        for (int i = 0; i < NbrWaves; ++i)
        {
            LastGround = Instantiate(DefaultGround, new Vector3(
                StartPosition.x + (GroundSize.x + DistanceBetweenGround) * i,
                StartPosition.y,
                StartPosition.z),
                Quaternion.identity, transform);
        }
    }

    #endregion
}
