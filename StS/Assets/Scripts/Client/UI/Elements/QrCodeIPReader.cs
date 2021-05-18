using UnityEngine;
using UnityEngine.UI;
using ZXing;
using SDD.Events;
using System.Collections;
using UnityEngine.Android;

public class QrCodeIPReader : MonoBehaviour
{
    // Attributes

    [SerializeField] private Text errorMessage;

    private RawImage display;
    private WebCamTexture camTexture;

    private Coroutine Reader;


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

            if (Screen.orientation == ScreenOrientation.LandscapeRight || Screen.orientation == ScreenOrientation.LandscapeLeft)
            {
                display.transform.localScale = new Vector3(-1, -1, 1);
            }
        }
    }

    private void OnEnable()
    {
        #region On lance la lecture

        Reader = StartCoroutine("SearchQRCode");

        #endregion
    }

    private void OnDisable()
    {
        // On s'assure que la caméra est éteinte
        if (camTexture != null)
        {
            camTexture.Stop();
        }
        if (Reader != null)
        {
            StopCoroutine(Reader);
        }
    }

    #endregion

    #region Coroutine

    private IEnumerator SearchQRCode()
    {
        // Si on est sur android
        if (Application.platform == RuntimePlatform.Android)
        {
            // On attend les permissions
            while (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                yield return new WaitForSeconds(0.5f);
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
            display.texture = camTexture;
            camTexture.Play();
            #endregion

            Result result = null;
            BarcodeReader barcodeReader = new BarcodeReader();

            while (result == null)
            {
                // Decode the current frame
                result = barcodeReader.Decode(camTexture.GetPixels32(),
                camTexture.width, camTexture.height); // On décode l'image

                if (result == null)
                {
                    yield return new WaitForSeconds(0.5f);
                } else if (!IPManager.ValidateIPv4(result.Text))
                {
                    errorMessage.text = "Please enter valid IP address";
                    result = null;
                    yield return new WaitForSeconds(0.5f);
                }
            }

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
        int width = (int) (Screen.width * 0.4f);
        int height = (int) (Screen.height * 0.4f);
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            // On recherche la front camera
            for (int i = 0; i < devices.Length; ++i)
            {
                if (devices[i].isFrontFacing)
                {
                    // On prend en dimension, la taille de l'écran.
                    camTexture = new WebCamTexture(devices[i].name, width, height);
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
                        camTexture = new WebCamTexture(devices[i].name, width, height);
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