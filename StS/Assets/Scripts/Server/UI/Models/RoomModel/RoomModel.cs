using SDD.Events;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CommonVisibleManager;
using ServerManager;

public class RoomModel : MonoBehaviour
{
    // Constante

    public static readonly string DEFAULT_NAME = "No Name";


    #region Attributs

    [Header("RoomModel")]

    [SerializeField] private Button NextButton;
    [SerializeField] private TextMeshProUGUI TextPrinter;

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
        if (ServerGameManager.Instance.GetPlayers() != null) // On conserve les joueurs
        {
            IEnumerator<KeyValuePair<ulong, Player>> Enumerator = ServerGameManager.Instance.GetPlayers().GetEnumerator();

            while (Enumerator.MoveNext())
            {
                Player p = Enumerator.Current.Value;
                p.Body = null;
                p.PlayerState = PlayerState.Selection;
                Players.Add(Enumerator.Current.Key, p);
            }
        }

        AI_Players = new List<AI_Player>();
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
            AI_Players.Add(new AI_Player());
            RefreshListPlayer();
        }
    }

    public void RemoveAI()
    {
        if (AI_Players.Count > 0)
        {
            AI_Players.RemoveAt(AI_Players.Count - 1);
            RefreshListPlayer();
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
        string text = "";

        Dictionary<ulong, Player>.ValueCollection value = Players.Values;
 
        foreach (Player v in value)
        {
            if (v.Pseudo == "")
            {
                text += DEFAULT_NAME + " : " + GetTextFrom(v.PlayerState) + "\n";
            }
            else
            {
                text += v.Pseudo + " : " + GetTextFrom(v.PlayerState) + "\n";
            }
        }

        for (int i = 0; i < AI_Players.Count; ++i)
        {
            text += "AI " + i + "\n";
        }

        TextPrinter.text = text;
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
     * NextButton.interactable == Players.Count > 0 && foreach player, playerState == ready
     * && Il y a au moin 1 chanson d'enregistré.
     */
    private void ActualiseNextButton()
    {
        bool IsValid = Players != null && Players.Count > 0 && ServerAccountManager.Instance.GetSongList().Length > 0;

        if (!IsValid)
        {
            NextButton.interactable = IsValid;
            return;
        } else
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
            EventManager.Instance.Raise(new RoomNextButtonClickedEvent()
            {
                PlayerList = Players,
                AI = AI_Players
            });
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
