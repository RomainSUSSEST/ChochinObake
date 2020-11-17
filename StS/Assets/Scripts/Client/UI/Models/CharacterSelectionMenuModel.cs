using SDD.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonVisibleManager;
using ClientVisibleManager;

public class CharacterSelectionMenuModel : MonoBehaviour
{
    // Classe

    private enum Direction { Left, Right }

        
    // Attributs

    [Header("Slime Elements")]

    [SerializeField] private List<SlimeHats> ListHat;
    [SerializeField] private List<SlimeBody> ListBody;

    [SerializeField] private SlimeClient PrefabSlimeClient;
    [SerializeField] private Camera PrefabCameraView;

    [SerializeField] private Transform SlimeSpawn;
    [SerializeField] private Transform CameraSpawn;

    [SerializeField] private float RotationSpeed;

    [Header("Editor")]

    [SerializeField] private List<GameObject> HaveToDisableWhenIsReady;

    [SerializeField] private PseudoInputField Pseudo;

    [Header("Button Ready")]

    [SerializeField] private Button ButtonReady;
    [SerializeField] private Image ButtonReadyImage;
    [SerializeField] private Color ReadyButtonColor;
    [SerializeField] private Color UnreadyButtonColor;

    [Header("Invalid Body Effect")]

    [SerializeField] private RawImage InvalidBodyImage;

    private SlimeClient InstanceSlime;
    private Camera InstanceCamera;

    private int IndexHats;
    private int IndexBody;

    private List<SlimeBody.BodyType> InvalidBody; // Contient la liste des corps qui sont déjà pris

    private bool PlayerIsReady;


    // Life cycle

    private void Awake()
    {
        SubscribeEvent();
    }

    private void OnDestroy()
    {
        UnsubscribeEvent();
    }

    public void Start()
    {
        if (ListHat.Count == 0 || ListBody.Count == 0)
        {
            throw new System.Exception("Erreur dans l'initialisation de la classe CharacterSelectionMenu");
        }

        IndexHats = 0;
        IndexBody = 0;
    }

    private void OnEnable()
    {
        // On averti le serveur que le joueur est entré dans la selection de personnage.
        // Si celui-ci est bien connecté à un serveur.
        if (ClientNetworkManager.Instance == null || ClientNetworkManager.Instance.GetPlayerID() == null)
        {
            return;
        } else
        {
            PlayerIsReady = false;
            MessagingManager.Instance.RaiseNetworkedEventOnServer(
                new PlayerEnterInCharacterSelectionEvent(
                    ClientNetworkManager.Instance.GetPlayerID().Value));
        }
    }

    private void OnDisable()
    {
        if (InstanceSlime != null)
            // On détruit l'instance de Slime.
            Destroy(InstanceSlime.gameObject);

        if (InstanceCamera != null)
            // On détruit la caméra
            Destroy(InstanceCamera.gameObject);
    }


    // Requete

    public SlimeHats GetCurrentHats()
    {
        return ListHat[IndexHats];
    }

    public SlimeBody GetCurrentBody()
    {
        return ListBody[IndexBody];
    }


    // Méthode

    #region Button has been clicked
    public void ListOnServerButtonHasBeenClicked()
    {
        LoadServerListMusic();
    }

    public void AddSongButtonHasBeenClicked()
    {
        AddSongOnServer();
    }

    public void LeftHatButtonHasBeenClicked()
    {
        MoveHat(Direction.Left);
    }

    public void RightHatButtonHasBeenClicked()
    {
        MoveHat(Direction.Right);
    }

    public void LeftBodyButtonHasBeenClicked()
    {
        MoveBody(Direction.Left);
    }

    public void RightBodyButtonHasBeenClicked()
    {
        MoveBody(Direction.Right);
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
        // On crée la camera permettant de visualiser le slime.
        InstanceCamera = Instantiate(PrefabCameraView, CameraSpawn.position, CameraSpawn.rotation, CameraSpawn);

        SetHat();
        SetBody();

        // On vérifie que les boutons d'édition soit activé
        ActivatePersonnalisation(true);

        // On fait tourner le slime.
        RotateObjectAroundPivot rotateObjectAroundPivot = InstanceSlime.gameObject.AddComponent<RotateObjectAroundPivot>();
        rotateObjectAroundPivot.SetRotateSpeed(RotationSpeed);

        // On met la couleur du button ready en accord avec l'étape de personnalisation
        SetUnreadyButtonColor();
    }

    #region Gestion Music (Server List & Add)
    private void LoadServerListMusic()
    {

    }

    private void AddSongOnServer()
    {

    }
    #endregion

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
    /// <param name="e"></param>
    private void RequestForChangeState(ReadyCharacterSelectionEvent e)
    {
        if (ClientNetworkManager.Instance.GetPlayerID() == null)
        {
            throw new System.Exception("Client non connecté");
        } else if (Pseudo.GetPseudo() == "")
        {
            Pseudo.StartInvalidAnimation();
            return;
        }
        else
        {
            // On désactive les boutons en attendant une réponse
            ActivatePersonnalisation(false);

            // On previent le serveur que l'on  change d'état
            MessagingManager.Instance.RaiseNetworkedEventOnServer(
                new RequestPlayerReadyInCharacterSelectionEvent(
                    ClientNetworkManager.Instance.GetPlayerID().Value,
                    !PlayerIsReady,
                    GetCurrentHats().GetHatsType(),
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
        } else
        {
            SetUnreadyButtonColor();
        }
    }

    private void InvalidPseudo(InvalidPseudoEvent e)
    {
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

    private void MoveHat(Direction d)
    {
        if (d == Direction.Left)
        {
            --IndexHats;

            if (IndexHats < 0)
            {
                IndexHats = ListHat.Count - 1;
            }

        } else
        {
            IndexHats = (IndexHats + 1) % ListHat.Count;
        }

        SetHat();
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
    }

    private void SetHat()
    {
        InstanceSlime.SetHat(ListHat[IndexHats]);
    }

    private bool IsValidBody(SlimeBody.BodyType body)
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
        ButtonReadyImage.color = ReadyButtonColor;
    }

    private void SetUnreadyButtonColor()
    {
        ButtonReadyImage.color = UnreadyButtonColor;
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