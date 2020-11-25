using UnityEngine;
using UnityEngine.UI;
using ZXing;
using SDD.Events;
using System.Collections;

public class QrCodeIPReader : MonoBehaviour
{
    // Attributs

    [Header("QRCodeIPReader")]

    [SerializeField] private Text errorMessage;

    private bool camAvailable;
    private RawImage display;
    private WebCamTexture camTexture;
    private volatile bool stoped;

    
    // Life cycle
    void Start()
    {
        // On récupére les components
        display = GetComponent<RawImage>();

        // On identifie les caméras.

        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.LogError("No camera detected");
            camAvailable = false;
            return;
        }

        // On initialise les components pour s'adapter à la platforme.
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            display.transform.localScale = new Vector3(-1 * display.transform.localScale.x, display.transform.localScale.y, display.transform.localScale.z);

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
        } else 
        if (Application.platform == RuntimePlatform.Android)
        {
            display.transform.localScale = new Vector3(-1 * display.transform.localScale.x, -1 * display.transform.localScale.y, display.transform.localScale.z);
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

        // Si aucune caméra n'est trouvé
        if (camTexture == null)
        {
            Debug.LogError("No back camera");
            camAvailable = false;
            return;
        } else
        {
            camAvailable = true;
        }
    }

    private void OnEnable()
    {
        stoped = false;
        StartCoroutine("SearchQRCode");
    }

    private void OnDisable()
    {
        stoped = true;
    }


    // Outils Coroutine

    private IEnumerator SearchQRCode()
    {
        // Si nous n'avons aucune caméra...
        if (!camAvailable) // Attention de vérifié que ce n'est pas un enable de resize dimension !
        {
            // TO DO
        }
        else
        {
            errorMessage.text = ""; // initialisation du message d'erreur.
            display.texture = camTexture;
            camTexture.Play();

            Result result = null;
            IBarcodeReader barcodeReader = new BarcodeReader();
            while (result == null && !stoped)
            {
                // Decode the current frame
                Texture2D texture = TextureToTexture2D(display.texture);
                result = barcodeReader.Decode(texture.GetPixels32(),
                    texture.width, texture.height);

                if (result != null && !IPManager.ValidateIPv4(result.Text))
                {
                    errorMessage.text = "Invalid IP : " + result.Text;
                    result = null;
                }

                yield return new WaitForSeconds(1);
            }

            camTexture.Stop();
            
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
}
