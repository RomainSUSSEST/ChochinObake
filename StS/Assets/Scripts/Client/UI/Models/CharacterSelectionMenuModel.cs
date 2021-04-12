using SDD.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonVisibleManager;
using ClientManager;
using TMPro;

public class CharacterSelectionMenuModel : MonoBehaviour
{
    // Classe

    private enum Direction { Left, Right }

        
    // Attributs

    [Header("Slime Elements")]

    [SerializeField] private List<CharacterBody> ListBody;

    [SerializeField] private CharacterClient PrefabSlimeClient;
    [SerializeField] private Transform SlimeSpawn;

    [Header("Editor")]

    [SerializeField] private List<GameObject> HaveToDisableWhenIsReady;

    [SerializeField] private PseudoInputField Pseudo;

    [SerializeField] private TextMeshProUGUI YourStory;

    [Header("Button Ready & Vibrator")]

    [SerializeField] private Button ButtonReady;
    [SerializeField] private Button Vibrator;

    [SerializeField] private Sprite UnOKButtonSprite;
    [SerializeField] private Sprite OKButtonSprite;


    [Header("Invalid Body Effect")]

    [SerializeField] private RawImage InvalidBodyImage;

    private CharacterClient InstanceSlime;
    private Camera InstanceCamera;

    private int IndexBody;

    private List<CharacterBody.BodyType> InvalidBody; // Contient la liste des corps qui sont déjà pris

    private bool PlayerIsReady;


    // Life cycle

    public void Start()
    {
        if (ListBody.Count == 0)
        {
            throw new System.Exception("Erreur dans l'initialisation de la classe CharacterSelectionMenu");
        }

        IndexBody = 0;

        // Initialisation du vibrator
        Vibrator.image.sprite = OKButtonSprite;
        ClientVibratorManager.Instance.SetVibrator(true);
    }

    private void OnEnable()
    {
        SubscribeEvent();

        // On averti le serveur que le joueur est entré dans la selection de personnage.
        // Si celui-ci est bien connecté à un serveur.
        if (ClientNetworkManager.Instance == null || ClientNetworkManager.Instance.GetPlayerID() == null)
        {
            return;
        } else
        {
            PlayerIsReady = false;
            EnableReadyButton(); // On active le bouton pour changer d'état.
            MessagingManager.Instance.RaiseNetworkedEventOnServer(
                new PlayerEnterInCharacterSelectionEvent(
                    ClientNetworkManager.Instance.GetPlayerID().Value));
        }
    }

    private void OnDisable()
    {
        UnsubscribeEvent();

        if (InstanceSlime != null)
            // On détruit l'instance de Slime.
            Destroy(InstanceSlime.gameObject);

        if (InstanceCamera != null)
            // On détruit la caméra
            Destroy(InstanceCamera.gameObject);
    }


    // Requete

    public CharacterBody GetCurrentBody()
    {
        return ListBody[IndexBody];
    }


    // Méthode

    #region Button has been clicked

    public void LeftBodyButtonHasBeenClicked()
    {
        MoveBody(Direction.Left);
    }

    public void RightBodyButtonHasBeenClicked()
    {
        MoveBody(Direction.Right);
    }

    public void VibratorButtonHasBeenClicked()
    {
        if (ClientVibratorManager.Instance.IsVibratorEnable())
        {
            Vibrator.image.sprite = UnOKButtonSprite;
            ClientVibratorManager.Instance.SetVibrator(false);
        } else
        {
            Vibrator.image.sprite = OKButtonSprite;
            ClientVibratorManager.Instance.SetVibrator(true);
        }
    }

    #endregion

    #region Event Subscribe
    private void SubscribeEvent()
    {
        // GameState
        EventManager.Instance.AddListener<ReadyCharacterSelectionEvent>(RequestForChangeState);

        //Networked Event
        EventManager.Instance.AddListener<RequestAcceptedPlayerReadyInCharacterSelectionEvent>(ChangeStateApproved);
        EventManager.Instance.AddListener<InvalidPseudoEvent>(InvalidPseudo);
        EventManager.Instance.AddListener<InvalidColorEvent>(InvalidColor);
        EventManager.Instance.AddListener<InverseStateOfColorEvent>(InverseStateOfColor);
        EventManager.Instance.AddListener<LobbyInformationEvent>(LoadLobbyInformation);
    }

    private void UnsubscribeEvent()
    {
        // GameState
        EventManager.Instance.RemoveListener<ReadyCharacterSelectionEvent>(RequestForChangeState);

        // Networked Event
        EventManager.Instance.RemoveListener<RequestAcceptedPlayerReadyInCharacterSelectionEvent>(ChangeStateApproved);
        EventManager.Instance.RemoveListener<InvalidPseudoEvent>(InvalidPseudo);
        EventManager.Instance.RemoveListener<InvalidColorEvent>(InvalidColor);
        EventManager.Instance.RemoveListener<InverseStateOfColorEvent>(InverseStateOfColor);
        EventManager.Instance.RemoveListener<LobbyInformationEvent>(LoadLobbyInformation);
    }
    #endregion


    // Outils

    private void SetupCharacterSelectionMenu()
    {
        // On crée un objet SlimeClient sur spawn point et on le rend enfant de spawn point
        InstanceSlime = Instantiate(PrefabSlimeClient, SlimeSpawn.position, SlimeSpawn.rotation, SlimeSpawn);

        SetBody();

        // On vérifie que les boutons d'édition soit activé
        ActivatePersonnalisation(true);

        // On met la couleur du button ready en accord avec l'étape de personnalisation
        SetUnreadyButtonColor();
    }

    #region Event Call Back
    #region Network
    private void LoadLobbyInformation(LobbyInformationEvent e)
    {
        // On récupére les invalides body du serveur.
        InvalidBody = e.InvalidBody;

        SetupCharacterSelectionMenu();
    }

    /// <summary>
    /// On demande à valider nos choix de customisation, si la fonction n'est pas interrompue et que le serveur renvoie
    /// une réponse positive, la demande est accepté.
    /// </summary>
    /// <param name="e"> L'event </param>
    private void RequestForChangeState(ReadyCharacterSelectionEvent e)
    {
        if (ClientNetworkManager.Instance.GetPlayerID() == null) // On s'assure que le client est connecté
        {
            throw new System.Exception("Client non connecté");
        } else if (Pseudo.GetPseudo() == "") // On refuse les Pseudo vide
        {
            Pseudo.StartInvalidAnimation();
            return;
        }
        else
        {
            // On désactive les boutons en attendant une réponse
            ActivatePersonnalisation(false);
            DisableReadyButton(); // On interdit le spam de demande

            // On previent le serveur que l'on  change d'état
            MessagingManager.Instance.RaiseNetworkedEventOnServer(
                new RequestPlayerReadyInCharacterSelectionEvent(
                    ClientNetworkManager.Instance.GetPlayerID().Value,
                    !PlayerIsReady,
                    GetCurrentBody().GetBodyType(),
                    Pseudo.GetPseudo()));
        }
    }

    private void ChangeStateApproved(RequestAcceptedPlayerReadyInCharacterSelectionEvent e)
    {
        PlayerIsReady = !PlayerIsReady; // On change d'état
        
        ActivatePersonnalisation(!PlayerIsReady);

        if (PlayerIsReady)
        {
            SetReadyButtonColor();
            EventManager.Instance.Raise(new RefreshCharacterInformationEvent() { body = GetCurrentBody() });
        } else
        {
            SetUnreadyButtonColor();
            EventManager.Instance.Raise(new RefreshCharacterInformationEvent() { body = null });
        }

        // On réactive le bouton pour changer d'état
        EnableReadyButton();
    }

    /// <summary>
    /// Refus de la demande de validation dû au pseudo.
    /// </summary>
    /// <param name="e"></param>
    private void InvalidPseudo(InvalidPseudoEvent e)
    {
        // On active la personalisation
        ActivatePersonnalisation(true);

        // On réactive le button enable
        EnableReadyButton();

        Pseudo.StartInvalidAnimation();
    }

    private void InvalidColor(InvalidColorEvent e)
    {
        // TO DO
    }

    /// <summary>
    /// Si e.PlayerID.Value n'est pas l'id du joueur en question, inverse la valeur InvalidBody du body cible puis actualise
    /// à l'aide de SetBody si le body est disponible ou non
    /// </summary>
    /// <param name="e"></param>
    private void InverseStateOfColor(InverseStateOfColorEvent e)
    {
        if (e.PlayerID != null && e.PlayerID.Value != ClientNetworkManager.Instance.GetPlayerID())
        {
            // Si on arrive pas a le supprimer, on l'ajoute
            if (!InvalidBody.Remove(e.BodyType))
            {
                InvalidBody.Add(e.BodyType);
            }
        }

        SetBody();
    }
    #endregion
    #endregion

    #region Personnalisation
    private void ActivatePersonnalisation(bool activate)
    {
        foreach (GameObject b in HaveToDisableWhenIsReady)
        {
            b.SetActive(activate);
        }
    }

    private void MoveBody(Direction d)
    {
        if (d == Direction.Left)
        {
            --IndexBody;

            if (IndexBody < 0)
            {
                IndexBody = ListBody.Count - 1;
            }
        } else
        {
            IndexBody = (IndexBody + 1) % ListBody.Count;
        }

        SetBody();
    }

    /// <summary>
    /// Si c'est un body valid, l'affecte et change l'état du bouton ready en conséquence.
    /// </summary>
    private void SetBody()
    {
        // On vérifie que c'est un body valid.
        if (!IsValidBody(ListBody[IndexBody].GetBodyType()))
        {
            DisableReadyButton();
            EnableInvalidBodyImage();
        }
        else
        {
            EnableReadyButton();
            DisableInvalidBodyImage();
        }

        InstanceSlime.SetBody(ListBody[IndexBody]);
        YourStory.text = InstanceSlime.GetCharacterBody().GetStoryBody();
    }

    private bool IsValidBody(CharacterBody.BodyType body)
    {
        return !InvalidBody.Contains(body);
    }
    #endregion

    #region Button Ready Function
    private void DisableReadyButton()
    {
        ButtonReady.interactable = false;
    }

    private void EnableReadyButton()
    {
        ButtonReady.interactable = true;
    }

    private void SetReadyButtonColor()
    {
        ButtonReady.image.sprite = OKButtonSprite;
    }

    private void SetUnreadyButtonColor()
    {
        ButtonReady.image.sprite = UnOKButtonSprite;
    }

    #endregion

    #region Invalid Body Image
    private void EnableInvalidBodyImage()
    {
        InvalidBodyImage.gameObject.SetActive(true);
    }

    private void DisableInvalidBodyImage()
    {
        InvalidBodyImage.gameObject.SetActive(false);
    }
    #endregion
}