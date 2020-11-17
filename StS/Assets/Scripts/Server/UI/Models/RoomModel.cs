using SDD.Events;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CommonVisibleManager;

public class RoomModel : ServerSimpleGameStateObserver
{
    // Constante

    public static readonly string DEFAULT_NAME = "No Name";


    // Attributs

    [Header("RoomModel")]

    [SerializeField] private Button NextButton;
    [SerializeField] private TextMeshProUGUI TextPrinter;

    [SerializeField] private List<SlimeHats> ListHats;
    [SerializeField] private List<SlimeBody> ListBody;

    private Dictionary<ulong, Player> Players;

    private List<SlimeBody.BodyType> InvalidBody; // Enregistre la liste des body déjà pris

    [Header("Panel Add Song")]

    [SerializeField] private GameObject PanelSongList;


    // Life Cycle

    private void OnEnable()
    {
        // Initialisation

        Players = new Dictionary<ulong, Player>();
        InvalidBody = new List<SlimeBody.BodyType>();
        PanelSongList.SetActive(false); // On s'assure que le panel de gestion des sons est désactivé.

        ActualiseNextButton();
    }


    // Event subscription

    public override void SubscribeEvents()
    {
        base.SubscribeEvents();

        // Network Server
        EventManager.Instance.AddListener<ServerConnectionSuccessEvent>(AddPlayer);
        EventManager.Instance.AddListener<ServerDisconnectionSuccessEvent>(RemovePlayer);

        // Network Common Event
        EventManager.Instance.AddListener<PlayerEnterInCharacterSelectionEvent>(PlayerEnterInCharacterSelection);
        EventManager.Instance.AddListener<RequestPlayerReadyInCharacterSelectionEvent>(RequestPlayerReadyInCharacterSelection);

        // GameManager

        EventManager.Instance.AddListener<AskForNewGameEvent>(SendListPlayer);
    }

    public override void UnsubscribeEvents()
    {
        // Network Server
        EventManager.Instance.RemoveListener<ServerConnectionSuccessEvent>(AddPlayer);
        EventManager.Instance.RemoveListener<ServerDisconnectionSuccessEvent>(RemovePlayer);

        // Network Common Event
        EventManager.Instance.RemoveListener<PlayerEnterInCharacterSelectionEvent>(PlayerEnterInCharacterSelection);
        EventManager.Instance.RemoveListener<RequestPlayerReadyInCharacterSelectionEvent>(RequestPlayerReadyInCharacterSelection);

        // GameManager

        EventManager.Instance.RemoveListener<AskForNewGameEvent>(SendListPlayer);
    }


    // On click button function

    public void SongButtonHasBeenClicked()
    {
        PanelSongList.SetActive(true); // On Active le panel de gestion des sons.
    }


    // Network Event Call

    private void AddPlayer(ServerConnectionSuccessEvent e)
    {
        Players.Add(e.ClientID, new Player()
        {
            PlayerState = PlayerState.Connection,
            Hat = null,
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
            p.Hat = GetSlimeHats(e.HatType);
            p.Body = GetSlimeBody(e.BodyType);
            p.Pseudo = e.Pseudo;

        } else // Sinon on désenregistre les choix du joueur.
        {
            // On met à jours l'InvalidBody
            InvalidBody.Remove(p.Body.GetBodyType());

            p.PlayerState = PlayerState.Selection;

            // On désenregistre la customisation

            p.Hat = null;
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


    // GameManager Event Call

    private void SendListPlayer(AskForNewGameEvent e)
    {
        EventManager.Instance.Raise(new SetPlayerListEvent(Players));
    }


    // Outils

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

        TextPrinter.text = text;
    }

    private string GetTextFrom(PlayerState p)
    {
        switch (p)
        {
            case PlayerState.Connection :
                return "Connexion";
            case PlayerState.Selection :
                return "Selection Slime";
            case PlayerState.Ready :
                return "Ready";
            default :
                throw new System.Exception();
        }
    }

    /**
     * NextButton.interactable == Players.Count > 0 && foreach player, playerState == ready
     */
    private void ActualiseNextButton()
    {
        bool IsValid = Players != null && Players.Count > 0;

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
        }
    }

    #region Correspondance Slime Type <=> Prefab
    private SlimeHats GetSlimeHats(SlimeHats.HatsType type)
    {
        foreach (SlimeHats hat in ListHats)
        {
            if (hat.GetHatsType().Equals(type))
            {
                return hat;
            }
        }

        throw new System.Exception("Prefab manquante");
    }

    private SlimeBody GetSlimeBody(SlimeBody.BodyType type)
    {
        foreach (SlimeBody body in ListBody)
        {
            if (body.GetBodyType().Equals(type))
            {
                return body;
            }
        }

        throw new System.Exception("Prefab manquante");
    }
    #endregion
}
