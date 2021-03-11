using UnityEngine;
using UnityEngine.UI;
using ZXing;
using SDD.Events;
using System.Collections;
using UnityEngine.Android;

public class QrCodeIPReader : MonoBehaviour
{
    // Attributs

    [Header("QRCodeIPReader")]

    [SerializeField] private Text errorMessage;

    private RawImage display;
    private WebCamTexture camTexture;

    private bool StopRead;


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
            display.transform.localScale = new Vector3(-1 * display.transform.localScale.x, display.transform.localScale.y * -1, display.transform.localScale.z);

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

            Result result = null;
            IBarcodeReader barcodeReader = new BarcodeReader();
            #endregion

            while (!StopRead && result == null)
            {
                // Decode the current frame
                Texture2D texture = TextureToTexture2D(display.texture); // On récupére la texture
                result = barcodeReader.Decode(texture.GetPixels32(),
                    texture.width, texture.height); // On écode l'image

                #region Invalid IP
                if (result != null && !IPManager.ValidateIPv4(result.Text))
                {
                    errorMessage.text = "Invalid IP : " + result.Text;
                    result = null;
                }
                #endregion

                yield return new WaitForSeconds(1f);
            }

            // Renvoie du résultat.
            if (result != null)
            {
                EventManager.Instance.Raise(new ServerConnectionEvent()
                {
                    Adress = result.Text
                });
            }
        }
    }

    #endregion


    // Outils

    private Texture2D TextureToTexture2D(Texture texture)
    {
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);
        return texture2D;
    }

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