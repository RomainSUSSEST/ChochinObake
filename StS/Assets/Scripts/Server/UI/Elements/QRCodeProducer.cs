using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

public class QRCodeProducer : MonoBehaviour
{
    // Attributs

    private RawImage rawImage;


    // Life cyles

    private void Start()
    {
        rawImage = GetComponent<RawImage>();

        rawImage.texture = GenerateQRCode(IPManager.GetIP(ADDRESSFAM.IPv4));
    }


    // Outils

    private Texture2D GenerateQRCode(string text)
    {
        Texture2D encoded = new Texture2D(256, 256);
        Color32[] color32 = Encode(text, encoded.width, encoded.height);

        encoded.SetPixels32(color32);
        encoded.Apply();

        return encoded;
    }

    private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        BarcodeWriter writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };

        return writer.Write(textForEncoding);
    }
}
