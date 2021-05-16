using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SDD.Events;

public class JoinRoomModel : MonoBehaviour
{
    #region ATTRIBUTS

    [Header("Manual Connection")]
    [SerializeField] private TMP_InputField ip_Address;
    [SerializeField] private Text errorMessage;

    #endregion

    #region LIFE CYCLES

    private void OnEnable()
    {
        errorMessage.text = "";
    }

    #endregion

    #region OnClickButtonEvents

    public void EnterIPButtonHasBeenClicked()
    {
        if (ip_Address.text == "")
        {
            errorMessage.text = "Please enter IP address";
        } else if (!IPManager.ValidateIPv4(ip_Address.text))
        {
            errorMessage.text = "Please enter valid IP address";
        } else
        {
            EventManager.Instance.Raise(new ServerConnectionEvent()
            {
                Adress = ip_Address.text
            });
        }
    }

    #endregion
}
