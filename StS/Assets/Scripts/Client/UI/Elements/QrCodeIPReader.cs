using UnityEngine;
using UnityEngine.UI;
using ZXing;
using SDD.Events;
using System.Collections;
using UnityEngine.Android;
using System.Threading.Tasks;

public class QrCodeIPReader : MonoBehaviour
{
    // Attributs

    [Header("QRCodeIPReader")]

    [SerializeField] private Text errorMessage;

    private RawImage display;
    private WebCamTexture camTexture;
    private IBarcodeReader barcodeReader;

    private volatile bool StopRead;


    #region Life Cycle

    private void Awake()
    {
        // On récupére les components
        display = GetComponent<RawImage>();
    }

    private void Start()
    {
        // On demande les autorisation de la caméra
        if (Application.platform == RuntimePlatform.Android
            && !Permission.HasUserAuthorizedPermission(Permission.Camera)) // Si on est sur android et que l'on a pas les autorisations
        {
            Permission.RequestUserPermission(Permission.Camera); // On les demande

            // On prepare le recepteur de texture
            display.transform.localScale = new Vector3(display.transform.localScale.x * -1, display.transform.localScale.y * -1, display.transform.localScale.z);

        } else if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            // On prépare le recepteur de texture
            display.transform.localScale = new Vector3(-1 * display.transform.localScale.x, display.transform.localScale.y, display.transform.localScale.z);
        }
    }

    private void OnEnable()
    {
        #region On lance la lecture

        StopRead = false;
        StartCoroutine("SearchQRCode");

        #endregion
    }

    private void OnDisable()
    {
        // On s'assure que la caméra est éteinte
        if (camTexture != null)
        {
            camTexture.Stop();
        }
        StopRead = true;
    }

    #endregion

    #region Coroutine

    private IEnumerator SearchQRCode()
    {
        // Si on est sur android
        if (Application.platform == RuntimePlatform.Android)
        {
            // On attend les permissions
            while (!StopRead && !Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                yield return new WaitForSeconds(0.5f);
            }

            // Si la lecture est terminé, on arrete
            if (StopRead)
            {
                yield break;
            }
        }

        // Nous avons les permissions et la lecture continue...

        if (!InitializeCamera()) // Si l'initialisation se passe mal
        {
            yield break;
        }
        else
        {
            #region Initialisation des composants & lancement de la caméra
            errorMessage.text = ""; // initialisation du message d'erreur.
            display.texture = camTexture;
            camTexture.Play();

            barcodeReader = new BarcodeReader();
            #endregion

            Result result = null;
            Task.Run(() =>
            {
                while (!StopRead)
                {
                    // Decode the current frame
                    Result tampon = barcodeReader.Decode(camTexture.GetPixels32(),
                    camTexture.width, camTexture.height); // On décode l'image

                    if (IPManager.ValidateIPv4(tampon.Text))
                    {
                        StopRead = true;
                        result = tampon;
                    }
                }
            });
            
            while (!StopRead)
                yield return new WaitForSeconds(0.5f);

            EventManager.Instance.Raise(new ServerConnectionEvent()
            {
                Adress = result.Text
            });
        }
    }

    #endregion


    // Outils

    /// <summary>
    /// Renvoie true si l'initialisation c'est bien passé
    /// False sinon
    /// </summary>
    /// <returns></returns>
    private bool InitializeCamera()
    {
        #region On identifie les caméras.

        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.LogError("No camera detected");
            return false;
        }

        #endregion

        #region On initialise les components pour s'adapter à la platforme.
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            // On recherche la front camera
            for (int i = 0; i < devices.Length; ++i)
            {
                if (devices[i].isFrontFacing)
                {
                    // On prend en dimension, la taille de l'écran.
                    camTexture = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
                    break;
                }
            }
        }
        else
            if (Application.platform == RuntimePlatform.Android)
            {            
                Screen.sleepTimeout = SleepTimeout.NeverSleep;

                // On recherche la back camera
                for (int i = 0; i < devices.Length; ++i)
                {
                    if (!devices[i].isFrontFacing)
                    {
                        // On prend en dimension, la taille de l'écran.
                        camTexture = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
                        break;
                    }
                }
            }
        #endregion

        // Si aucune caméra n'est trouvé
        if (camTexture == null)
        {
            Debug.LogError("No back camera");
            return false;
        }
        else
        {
            return true;
        }
            
    }
}