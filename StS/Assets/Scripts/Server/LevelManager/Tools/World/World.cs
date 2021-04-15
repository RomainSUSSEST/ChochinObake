using SDD.Events;
using ServerManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe mère des mondes, permet de générer une représentation des joueurs,
/// le ground, le background, les obstacles et l'arrivée.
/// 
/// Leur comportement est ensuite autonome.
/// 
/// Pour le gameplay et le scoring, voir ServerLevelManager
/// </summary>
public class World : MonoBehaviour
{
    #region Attributes

    [Header("Map Elements")]
    [SerializeField] private List<Obstacle> ListObstacle; // Obstacle
    [SerializeField] private Ground SpiritWay; // Ground

    [SerializeField] private List<Background> ListBackgrounds_P1; // Background
    [SerializeField] private List<Background> ListBackgrounds_P2; // ...
    [SerializeField] private List<Background> ListBackgrounds_P3; // ...

    [SerializeField] private GameObject EndMapPrefab; // Arrivé

    [SerializeField] private Material CustomFog;

    [Header("Map Config")]
    [SerializeField] private float MinDistanceBeetweenGround;
    [SerializeField] private float MaxDistanceBeetweenGround;
    [SerializeField] private float ObstacleDistanceToCharacterSpawn;
    [SerializeField] private float ArrivalDistanceToCharacterSpawn;
    [SerializeField] private float DistanceToBackgroundY = -50;
    [SerializeField] private float DestroyElementsMargin = 10f; // Distance supplémentaire avant de détruire un élément

    [SerializeField] private float FogSpeed_X;
    [SerializeField] private float FogSpeed_Y;
    [SerializeField] private float FogRatio_Z;

    [Header("SlimeServer")]
    [SerializeField] private CharacterServer CharacterServerPrefab;

    // Information sur la carte
    private System.Collections.ObjectModel.ReadOnlyCollection<SpectralFluxInfo> CurrentMap;
    private float CurrentDistanceBetweenGround;

    #region Gestion des grounds

    private Ground LastSpiritWay; // Dernier ground spawné
    private Vector3 SpiritWaySize; // Taille des grounds
    private int NbrWays; // Nombre de chemins

    private float StartWaySpawnPosition_X; // Position d'apparition des chemins X

    private int CmptWaysHasBeenDestroyed; // Nombre de chemins détruit depuis le dernier spawn

    #endregion

    #region Gestion des backgrounds

    private Background LastBackground; // Dernier background spawné
    private Vector3 BackgroundSize; // Taille des backgrounds

    private float StartBackgroundSpawnPosition_X; // Position x de départ des backgrounds

    #endregion

    #region Character Array

    private CharacterServer[] CharacterArray; // CharacterArray[i] contient le Character représentant le joueur
                                              // à la wave i ou null.

    #endregion

    #region Obstacle

    private float CurrentMaxPrunedSpectralFlux;
    private float CurrentSensitivity;
    private float CurrentThresholdSensitivity;
    private float AheadTimeToSpawn;

    #endregion

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

    private IEnumerator Start()
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
        IReadOnlyDictionary<ulong, Player> Players = ServerGameManager.Instance.GetPlayers();
        // On récupére la liste des IA
        IReadOnlyCollection<AI_Player> AI = ServerGameManager.Instance.GetAIList();

        // On vérifie les données
        if (CurrentMap == null || clip == null || Players == null)
        {
            throw new System.Exception("Donnée de carte invalide");
        }
        #endregion

        #region Background & Ground & Player

        #region Variables

        // Ground
        SpiritWaySize = SpiritWay.GetComponent<Renderer>().bounds.size; // On récupére la taille d'un des grounds
        NbrWays = Players.Count + AI.Count; // Il y a un nombre de wave minimum
        CurrentDistanceBetweenGround = Mathf.Lerp(MaxDistanceBeetweenGround, MinDistanceBeetweenGround, (float) NbrWays * 1.2f / ServerNetworkManager.MAX_PLAYER_CONNECTED);
        StartWaySpawnPosition_X = transform.position.x - (((SpiritWaySize.x + CurrentDistanceBetweenGround) * NbrWays) - CurrentDistanceBetweenGround) / 2;

        // Background
        BackgroundSize = Vector3.one * Background.SIZE;
        StartBackgroundSpawnPosition_X = transform.position.x - BackgroundSize.x / 2;

        // Player
        CharacterArray = new CharacterServer[NbrWays];

        #endregion

        #region Array Player initialization

        // On initialise l'array player
        int nextValue = 0;

        float CharacterSpawn_X = StartWaySpawnPosition_X;
        float CharacterSpawn_Y = transform.position.y;

        IEnumerator<ulong> enumPlayer = Players.Keys.GetEnumerator(); // Enum sur les joueurs
        IEnumerator<AI_Player> enumAI = AI.GetEnumerator(); // Enum sur les AI

        int index = NbrWays % 2 == 0 ? (NbrWays / 2) - 1 : (int)Mathf.Floor(NbrWays / 2f);

        for (int i = index; 0 <= i && i < NbrWays; i += nextValue)
        {
            if (enumPlayer.MoveNext())
            {

                // Slime Array
                CharacterArray[i] = Instantiate(CharacterServerPrefab,
                    new Vector3(
                        CharacterSpawn_X + (SpiritWaySize.x + CurrentDistanceBetweenGround) * i,
                        CharacterSpawn_Y,
                        transform.position.z - DestroyElementsMargin),
                    Quaternion.identity, transform); // On créer le slime

                CharacterArray[i].AssociedClientID = (ulong)enumPlayer.Current; // On récupére l'ulong du playerCourant

                // On récupére le Player associé
                Player p = Players[CharacterArray[i].AssociedClientID];

                // On set le body
                CharacterArray[i].SetBody(p.Body);

            } else if (enumAI.MoveNext())
            {
                // Personnage array
                CharacterArray[i] = Instantiate(CharacterServerPrefab,
                    new Vector3(
                        CharacterSpawn_X + (SpiritWaySize.x + CurrentDistanceBetweenGround) * i,
                        CharacterSpawn_Y,
                        transform.position.z - DestroyElementsMargin),
                    Quaternion.identity, transform); // On crée le personnage

                CharacterArray[i].IsAI = true; // On indique que c'est une AI.

                // On créer l'AI Player
                AIPlayer ai = CharacterArray[i].gameObject.AddComponent<AIPlayer>();
                ai.SetAssociatedCharacterServer(CharacterArray[i]); // On associe l'ai au personnage
                ai.SetAssociatedAIProfil(enumAI.Current); // On associe l'ai au profil d'ai

                CharacterArray[i].AssociatedAIManager = ai; // On défini l'ai pour l'objet joueur, afin d'envoyer les évenements.

                // On set le body
                CharacterArray[i].SetBody(enumAI.Current.Body);
            } else
            {
                CharacterArray[i] = null;
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
        Ground.DESTROY_Z_POSITION = transform.position.z - SpiritWaySize.z - DestroyElementsMargin;

        Obstacle.DESTROY_Z_POSITION = transform.position.z - DestroyElementsMargin;

        Background.MOVE_SPEED = Ground.MOVE_SPEED / 1.4f;
        Background.DESTROY_Z_POSITION = transform.position.z - BackgroundSize.z;
        Background.LOOP = true;

        #region On Initialise la carte (Génération)
        // Ground
        InstantiateNewGroundsAt(new Vector3(StartWaySpawnPosition_X, transform.position.y, transform.position.z - DestroyElementsMargin));
        InstantiateNewGroundsAt(new Vector3(
             StartWaySpawnPosition_X,
             transform.position.y,
             LastSpiritWay.transform.position.z + SpiritWaySize.z));

        // Background
        InstantiateNewBackgroundAt(new Vector3(StartBackgroundSpawnPosition_X, DistanceToBackgroundY, transform.position.z));
        InstantiateNewBackgroundAt(new Vector3(
            StartBackgroundSpawnPosition_X,
            DistanceToBackgroundY,
            LastBackground.transform.position.z + BackgroundSize.z));

        #endregion

        #endregion

        #region Obstacle

        float[] tampon = new float[CurrentMap.Count];

        // On recopie le tableau
        for (int i = 0; i < CurrentMap.Count; ++i)
        {
            tampon[i] = CurrentMap[i].prunedSpectralFlux;
        }

        Array.Sort(tampon); // On trie

        if (tampon.Length == 0)
        {
            CurrentMaxPrunedSpectralFlux = 0;
            CurrentThresholdSensitivity = 0;
        } else
        {
            CurrentMaxPrunedSpectralFlux = tampon[tampon.Length - 1]; // On récupére le maximum
            CurrentThresholdSensitivity = tampon[(int)Math.Floor(tampon.Length * CurrentSensitivity)]; // On calcul la valeur de seuil minimal de la sensitivité de l'algo
        }

        #region Variable (AHeadTimeToSpawn)

        // On calcul le temps d'avance que doivent prendre les obstacles pour apparaitre à ObstacleDistanceToSpawn
        // du spawn des slimes et tj arriver au moment du beats.
        AheadTimeToSpawn = ObstacleDistanceToCharacterSpawn / Ground.MOVE_SPEED;

        #endregion

        #endregion

        #region Shader (Fog)

        CustomFog.SetVector("_FogSpeed", new Vector3(FogSpeed_X, FogSpeed_Y, FogRatio_Z * Background.MOVE_SPEED));

        #endregion

        // On lance l'algo procédural des obstacles & gestionnaire de la manche
        StartCoroutine("RoundManager");

        StartCoroutine("Rythme"); // Génére un son lorsque c'est le moment idéal de passer l'obstacle

        // On synchronise les obstacles avec la musique
        yield return new WaitForSeconds(AheadTimeToSpawn);

        // on indique que la manche commence et on communique un accès en lecture seul aux représentations
        // des joueurs.
        EventManager.Instance.Raise(new RoundStartEvent()
        {
            RoundPlayers = new System.Collections.ObjectModel.ReadOnlyCollection<CharacterServer>(CharacterArray)
        });
    }

    #endregion

    #region Coroutine

    /// <summary>
    /// Parcourt les beats de la musique pour générer les obstacles.
    /// Une fois ceci terminé, renvoie un RoundEndEvent lorsque la 
    /// manche est totalement terminée
    /// </summary>
    /// <returns></returns>
    private IEnumerator RoundManager()
    {
        // Map (obstacles)
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

        // On attend que la musique commence, si on atteint cette étape trop rapidement
        while (!ServerMusicManager.Instance.IsPlayingMusic())
        {
            yield return new CoroutineTools.WaitForFrames(1);
        }

        // on attend la Fin
        yield return new WaitForSeconds(ServerMusicManager.Instance.GetTimeLeftRoundMusic());

        MusicRoundEnd();
    }

    private IEnumerator Rythme() // TODO ?
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
                    SfxManager.Instance.PlaySfx(SfxManager.Instance.Balloon);
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
        EventManager.Instance.AddListener<BackgroundEndMapEvent>(BackgroundEndMap);
    }

    private void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GroundEndMapEvent>(GroundEndMap);
        EventManager.Instance.RemoveListener<BackgroundEndMapEvent>(BackgroundEndMap);
    }

    #endregion

    #region Event Call backs

    private void GroundEndMap(GroundEndMapEvent e)
    {
        if (++CmptWaysHasBeenDestroyed >= NbrWays)
        {
            InstantiateNewGroundsAt(new Vector3(
                StartWaySpawnPosition_X,
                transform.position.y,
                LastSpiritWay.transform.position.z + SpiritWaySize.z));

            CmptWaysHasBeenDestroyed = 0;
        }
    }

    private void BackgroundEndMap(BackgroundEndMapEvent e)
    {
        InstantiateNewBackgroundAt(new Vector3(
            StartBackgroundSpawnPosition_X,
            DistanceToBackgroundY,
            LastBackground.transform.position.z + BackgroundSize.z));
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
        for (int i = 0; i < NbrWays; ++i)
        {
            LastSpiritWay = Instantiate(SpiritWay, new Vector3(
                StartPosition.x + (SpiritWaySize.x + CurrentDistanceBetweenGround) * i,
                StartPosition.y,
                StartPosition.z),
                Quaternion.identity, transform);
        }
    }

    /// <summary>
    /// Instantie un background selon StartPosition en tenant compte du temps de la musqiue restant
    /// 1/3 -> ListBackground_P1
    /// 2/3 -> ListBackground_P2
    /// 3/3 -> ListBackground_P3
    /// 
    /// Effet de bord : Update LastBackground
    /// </summary>
    /// <param name="StartPosition"></param>
    private void InstantiateNewBackgroundAt(Vector3 StartPosition)
    {
        float percentTotalTime = ServerMusicManager.Instance.GetCurrentTimeRoundMusic() / ServerMusicManager.Instance.GetTotalDurationRoundMusic();

        int index;
        if (percentTotalTime < 1f / 3f) // On instantie un background P1
        {
            index = UnityEngine.Random.Range(0, ListBackgrounds_P1.Count);
            LastBackground = Instantiate(ListBackgrounds_P1[index], StartPosition, Quaternion.identity, transform);
        } else if (percentTotalTime < 2f / 3f) // On instantie un background P2
        {
            index = UnityEngine.Random.Range(0, ListBackgrounds_P2.Count);
            LastBackground = Instantiate(ListBackgrounds_P2[index], StartPosition, Quaternion.identity, transform);
        } else // On instantie un background P3
        {
            index = UnityEngine.Random.Range(0, ListBackgrounds_P3.Count);
            LastBackground = Instantiate(ListBackgrounds_P3[index], StartPosition, Quaternion.identity, transform);
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
        if (flux.prunedSpectralFlux >= CurrentThresholdSensitivity)
        {
            // On cherche à combien de pourcentage du maximum correspond le beats sans compter le threshold.
            float curPercentOfMax = (flux.prunedSpectralFlux - CurrentThresholdSensitivity)
                / (CurrentMaxPrunedSpectralFlux - CurrentThresholdSensitivity); // Max : 1

            // On obtient une valeur entre 0 et 1 que l'on multiplie au nombre d'obstacle - 1
            int index = (int) Mathf.Round(curPercentOfMax * (ListObstacle.Count - 1));

            Obstacle curObstacle;
           // Pour chaque slime, on invoque l'obstacle
           foreach (CharacterServer ss in CharacterArray)
            {
                if (ss != null) {
                    Vector3 pos = ss.transform.position;

                    // On instantie l'obstacle à ObstacleDistanceToCharacterSpawn
                    curObstacle = Instantiate(ListObstacle[index], new Vector3(
                        pos.x,
                        pos.y,
                        ss.GetCharacterBody().GetValidArea().transform.position.z + ObstacleDistanceToCharacterSpawn),
                        Quaternion.identity,
                        transform);

                    curObstacle.SetAssociatedCharacter(ss); // On associe le character à l'obstacle
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Fonction appelée lorsque la musique est terminée
    /// </summary>
    private void MusicRoundEnd()
    {
        // On génère l'arrivée
        Vector3 pos = transform.position;
        GameObject arrival = Instantiate(EndMapPrefab,
            new Vector3(pos.x, pos.y, pos.z + ArrivalDistanceToCharacterSpawn),
            Quaternion.identity,
            transform);

        // Désactivation du background
        Background.LOOP = false;

        // On averti de la fin de la chanson
        EventManager.Instance.Raise(new MusicRoundEndEvent()
        {
            TransformArrival = arrival.transform
        });
    }

    #endregion
}