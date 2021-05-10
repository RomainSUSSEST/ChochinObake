﻿using SDD.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonVisibleManager;
using ServerManager;
using System;

public class RoomModel : MonoBehaviour
{
    #region Constants

    public static readonly string DEFAULT_NAME = "No Name";
    private static readonly int PLAYER_MARGIN_HEIGHT = 5; // en px

    #endregion

    #region Attributs

    [Header("Panel Info")]

    [SerializeField] private RectTransform ViewPanel;

    [Header("RoomModel")]

    [SerializeField] private Button NextButton;
    [SerializeField] private GameObject ContentNodePlayers;
    [SerializeField] private PlayerListModel PlayerPrefabItems;

    [SerializeField] private List<CharacterBody> ListBody;

    private Dictionary<ulong, Player> Players;

    private List<CharacterBody.BodyType> InvalidBody; // Enregistre la liste des body déjà pris

    private List<AI_Player> AI_Players;

    [Header("Panel Add Song")]

    [SerializeField] private GameObject PanelSongList;

    #endregion

    #region Life Cycle

    private void OnEnable()
    {
        // Initialisation

        SubscribeEvents();

        Players = new Dictionary<ulong, Player>();
        if (ServerGameManager.Instance.GetPlayers() != null) // On conserve les joueurs toujours présents
        {
            IEnumerator<KeyValuePair<ulong, Player>> Enumerator = ServerGameManager.Instance.GetPlayers().GetEnumerator();

            while (Enumerator.MoveNext())
            {
                Player p = Enumerator.Current.Value;

                if (p.PlayerState != PlayerState.Disconnected)
                {
                    p.Body = null;
                    p.Pseudo = "";
                    p.PlayerState = PlayerState.Selection;
                    Players.Add(Enumerator.Current.Key, p);
                }
            }
        }

        AI_Players = new List<AI_Player>();
        IReadOnlyList<AI_Player> ai = ServerGameManager.Instance.GetAIList();
        if (ai != null) // On conserve les AI
        {
            for (int i = 0; i < ai.Count; ++i) 
            {
                AI_Players.Add(ai[i]);
            }
        }

        InvalidBody = new List<CharacterBody.BodyType>();
        PanelSongList.SetActive(false); // On s'assure que le panel de gestion des sons est désactivé.

        RefreshListPlayer();
        ActualiseNextButton();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    #endregion

    #region Event Subscription

    public void SubscribeEvents()
    {
        // Network Server
        EventManager.Instance.AddListener<ServerConnectionSuccessEvent>(AddPlayer);
        EventManager.Instance.AddListener<ServerDisconnectionSuccessEvent>(RemovePlayer);

        // Network Common Event
        EventManager.Instance.AddListener<PlayerEnterInCharacterSelectionEvent>(PlayerEnterInCharacterSelection);
        EventManager.Instance.AddListener<RequestPlayerReadyInCharacterSelectionEvent>(RequestPlayerReadyInCharacterSelection);

        // SongListModel Panel Event
        EventManager.Instance.AddListener<SongListModelHasBeenClosedEvent>(SongListModelHasBeenClosed);
    }

    public void UnsubscribeEvents()
    {
        // Network Server
        EventManager.Instance.RemoveListener<ServerConnectionSuccessEvent>(AddPlayer);
        EventManager.Instance.RemoveListener<ServerDisconnectionSuccessEvent>(RemovePlayer);

        // Network Common Event
        EventManager.Instance.RemoveListener<PlayerEnterInCharacterSelectionEvent>(PlayerEnterInCharacterSelection);
        EventManager.Instance.RemoveListener<RequestPlayerReadyInCharacterSelectionEvent>(RequestPlayerReadyInCharacterSelection);

        // SongListModel Panel Event
        EventManager.Instance.RemoveListener<SongListModelHasBeenClosedEvent>(SongListModelHasBeenClosed);
    }

    #endregion

    #region OnClick button

    public void SongButtonHasBeenClicked()
    {
        PanelSongList.SetActive(true); // On Active le panel de gestion des sons.
    }

    public void RoomNextButtonHasBeenClicked()
    {
        SetAIBodys();

        EventManager.Instance.Raise(new RoomNextButtonClickedEvent()
        {
            PlayerList = Players,
            AI = AI_Players
        });
    }

    public void AddAI()
    {
        if (Players.Count + AI_Players.Count < ServerNetworkManager.MAX_PLAYER_CONNECTED)
        {
            AI_Player currentAI = new AI_Player();
            currentAI.Name = AI_Players.Count.ToString();

            currentAI.Difficulty = UnityEngine.Random.Range(AI_Player.MIN_SUCCESS_RATE, AI_Player.MAX_SUCCESS_RATE);

            AI_Players.Add(currentAI);

            RefreshListPlayer();
            ActualiseNextButton();
        }
    }

    public void RemoveAI()
    {
        if (AI_Players.Count > 0)
        {
            AI_Players.RemoveAt(AI_Players.Count - 1);
            RefreshListPlayer();
            ActualiseNextButton();
        }
    }

    #endregion

    #region Network event call

    private void AddPlayer(ServerConnectionSuccessEvent e)
    {
        if (Players.Count + AI_Players.Count >= ServerNetworkManager.MAX_PLAYER_CONNECTED)
        {
            RemoveAI();
        }

        Players.Add(e.ClientID, new Player()
        {
            PlayerState = PlayerState.Connection,
            Body = null,
            Pseudo = ""
        });

        RefreshListPlayer();
        ActualiseNextButton();
    }

    private void RemovePlayer(ServerDisconnectionSuccessEvent e)
    {
        if (Players[e.ClientID].Body != null)
        {
            InvalidBody.Remove(Players[e.ClientID].Body.GetBodyType());
        }

        Players.Remove(e.ClientID);
        RefreshListPlayer();
        ActualiseNextButton();
    }

    /// <summary>
    /// Ajoute le player au dictionnaire, puis lui envoie les informations du lobby
    /// </summary>
    /// <param name="e"></param>
    private void PlayerEnterInCharacterSelection(PlayerEnterInCharacterSelectionEvent e)
    {
        Players[e.PlayerID.Value].PlayerState = PlayerState.Selection;
        RefreshListPlayer();

        // On envoie les informations du lobby au nouveau joueur
        MessagingManager.Instance.RaiseNetworkedEventOnClient(new LobbyInformationEvent(e.PlayerID.Value, InvalidBody));
    }

    /// <summary>
    /// Enregistre les informations sur la customisation du player et réserve la couleur ou la libère selon si le player
    /// est ready ou non.
    /// De meme, interdit les doublons dans les pseudos.
    /// </summary>
    /// <param name="e"></param>
    private void RequestPlayerReadyInCharacterSelection(RequestPlayerReadyInCharacterSelectionEvent e)
    {
        if (!e.PlayerID.HasValue)
        {
            throw new System.Exception("from ?");
        }

        Player p = Players[e.PlayerID.Value];
        
        if (e.IsReady) // Si le joueur demande à être pret
        {
            // On vérifie que le pseudo et la couleur ne sont pas déjà pris.
            Dictionary<ulong, Player>.ValueCollection values = Players.Values;

            foreach (Player v in values)
            {
                if (v.Pseudo != "" && v.Pseudo.Equals(e.Pseudo))
                {
                    MessagingManager.Instance.RaiseNetworkedEventOnClient(new InvalidPseudoEvent(e.PlayerID.Value));
                    return;
                }
                if (v.Body != null && v.Body.GetBodyType().Equals(e.BodyType))
                {
                    MessagingManager.Instance.RaiseNetworkedEventOnClient(new InvalidColorEvent(e.PlayerID.Value));
                    return;
                }
            }

            // On met à jours l'invalid Body
            InvalidBody.Add(e.BodyType);

            p.PlayerState = PlayerState.Ready;

            // On enregistre la customisation

            p.Body = GetSlimeBody(e.BodyType);
            p.Pseudo = e.Pseudo;

        } else // Sinon on désenregistre les choix du joueur.
        {
            // On met à jours l'InvalidBody
            InvalidBody.Remove(p.Body.GetBodyType());

            p.PlayerState = PlayerState.Selection;

            // On désenregistre la customisation

            p.Body = null;
            p.Pseudo = "";
        }
        
        RefreshListPlayer();
        ActualiseNextButton();

        MessagingManager.Instance.RaiseNetworkedEventOnClient(
            new RequestAcceptedPlayerReadyInCharacterSelectionEvent(e.PlayerID.Value));

        // On indique que tout les joueurs sauf celui identifier par l'id doivent rendre la couleur bodyType indisponible/disponible selon si on met ready le joueur ou non
        MessagingManager.Instance.RaiseNetworkedEventOnAllClient(
            new InverseStateOfColorEvent(e.PlayerID.Value, e.BodyType));
    }

    #endregion

    #region SongListModel Event call back

    private void SongListModelHasBeenClosed(SongListModelHasBeenClosedEvent e)
    {
        ActualiseNextButton();
    }

    #endregion

    #region Tools

    private void RefreshListPlayer()
    {
        #region clear

        foreach (Transform child in ContentNodePlayers.transform)
        {
            Destroy(child.gameObject);
        }

        #endregion

        #region Generate
        Dictionary<ulong, Player>.ValueCollection value = Players.Values;

        List<PlayerListModel> list = new List<PlayerListModel>();
        PlayerListModel tampon;

        foreach (Player v in value)
        {
            tampon = Instantiate(PlayerPrefabItems, ContentNodePlayers.transform);
            if (v.Pseudo == "")
            {
                tampon.m_Pseudo.text = DEFAULT_NAME;
            } else
            {
                tampon.m_Pseudo.text = v.Pseudo;
            }

            tampon.m_Score.text = v.Score.ToString();
            tampon.m_Victory.text = v.Victory.ToString();

            list.Add(tampon);
        }

        for (int i = 0; i < AI_Players.Count; ++i)
        {
            tampon = Instantiate(PlayerPrefabItems, ContentNodePlayers.transform);

            tampon.m_Pseudo.text = "Chochin " + AI_Players[i].Name;
            tampon.m_Score.text = AI_Players[i].Score.ToString();
            tampon.m_Victory.text = AI_Players[i].Victory.ToString();

            list.Add(tampon);
        }
        #endregion

        #region Sort

        list.Sort((PlayerListModel x, PlayerListModel y) =>
            Int32.Parse(y.m_Score.text) - Int32.Parse(x.m_Score.text)
            );

        #endregion

        #region Placement

        float areaHeight = PlayerPrefabItems.GetComponent<RectTransform>().rect.height;
        float currentMarginHeight = PLAYER_MARGIN_HEIGHT;

        // On tient compte du rescale de la vue
        areaHeight *= ViewPanel.lossyScale.y;
        currentMarginHeight *= ViewPanel.lossyScale.y;

        // On estime la hauteur à allouer
        float height = (list.Count + 1) * currentMarginHeight
            + list.Count * areaHeight;
     
        height /= ViewPanel.lossyScale.y;

        // On redimenssionne le content
        RectTransform contentRectTransform = ContentNodePlayers.GetComponent<RectTransform>();
        contentRectTransform.sizeDelta = new Vector2
            (
            contentRectTransform.rect.width,
            height
            );

        // Position de départ

        Vector3 currentPositionButtonSpawn = new Vector3
            (
                contentRectTransform.position.x,
                (contentRectTransform.position.y
                    + height * ViewPanel.lossyScale.y / 2 - currentMarginHeight - areaHeight / 2),
                contentRectTransform.position.z
            );

        // On les affiches

        foreach (PlayerListModel plm in list)
        {
            plm.transform.position = currentPositionButtonSpawn;
            currentPositionButtonSpawn -= new Vector3(0, areaHeight + currentMarginHeight, 0);
        }

        // On décale le content pour afficher le premier en haut
        contentRectTransform.localPosition = new Vector3
            (
            contentRectTransform.localPosition.x,
            -height / 2 - contentRectTransform.parent.GetComponent<RectTransform>().rect.height,
            contentRectTransform.localPosition.z
            );

        #endregion
    }

    private void SetAIBodys()
    {
        int i = 0;
        foreach (AI_Player ai in AI_Players)
        {
            while (i < ListBody.Count)
            {
                if (!InvalidBody.Contains(ListBody[i].GetBodyType())) { // Si le body est disponible

                    ai.Body = ListBody[i];
                    ++i;
                    break;
                }
                ++i;
            }
        }
    }

    private string GetTextFrom(PlayerState p)
    {
        switch (p)
        {
            case PlayerState.Connection :
                return "Connexion";
            case PlayerState.Selection :
                return "Character Selection";
            case PlayerState.Ready :
                return "Ready";
            default :
                throw new System.Exception();
        }
    }

    /**
     * NextButton.interactable == (Players.Count > 0 || AI.Count > 0) && foreach player, playerState == ready
     * && Il y a au moin 1 chanson d'enregistré.
     */
    private void ActualiseNextButton()
    {
        // au moins 1 joueur ou 1 AI, et une chanson
        bool IsValid = (Players.Count > 0 || AI_Players.Count > 0) && ServerAccountManager.Instance.GetSongList().Length > 0;

        if (!IsValid)
        {
            NextButton.interactable = IsValid;
            return;
        } else // Est ce que chaque joueurs est pret ?
        {
            Dictionary<ulong, Player>.ValueCollection values = Players.Values;

            foreach (Player v in values)
            {
                if (!v.PlayerState.Equals(PlayerState.Ready))
                {
                    NextButton.interactable = false;
                    return;
                }
            }

            NextButton.interactable = true;
        }
    }

    #region Correspondance Slime Type <=> Prefab

    private CharacterBody GetSlimeBody(CharacterBody.BodyType type)
    {
        foreach (CharacterBody body in ListBody)
        {
            if (body.GetBodyType().Equals(type))
            {
                return body;
            }
        }

        throw new System.Exception("Prefab manquante");
    }
    #endregion

    #endregion
}
