using ClientManager;
using TMPro;
using UnityEngine;

public class ClientMainMenuModel : MonoBehaviour
{
    #region Attributes

    [SerializeField] private GameObject m_ErrorPanel;
    [SerializeField] private TextMeshProUGUI m_errorText;

    #endregion

    #region Life Cycle

    private void OnEnable()
    {
        if (ClientMenuManager.Instance.GetErrorMessage() == "")
        {
            m_ErrorPanel.SetActive(false);
        } else
        {
            m_ErrorPanel.SetActive(true);
            m_errorText.text = ClientMenuManager.Instance.GetErrorMessage();
        }
    }

    #endregion

    #region OnClickButton

    public void ButtonCloseHasBeenClicked()
    {
        m_ErrorPanel.SetActive(false);
    }

    #endregion
}
