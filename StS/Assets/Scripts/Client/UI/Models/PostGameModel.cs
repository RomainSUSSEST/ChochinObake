using ClientManager;
using TMPro;
using UnityEngine;

public class PostGameModel : MonoBehaviour
{
    #region Attributes

    [SerializeField] private TextMeshProUGUI LastScore;
    [SerializeField] private TextMeshProUGUI LastBestCombo;
    [SerializeField] private TextMeshProUGUI LastPowerUse;
    [SerializeField] private TextMeshProUGUI LastLanternSuccessPerTotal;
    [SerializeField] private TextMeshProUGUI LastRank;

    #endregion

    #region Life cycle

    private void OnEnable()
    {
        LastScore.text = ClientGameManager.Instance.GetLastScore().ToString();
        LastBestCombo.text = ClientGameManager.Instance.GetLastBestCombo().ToString();
        LastPowerUse.text = ClientGameManager.Instance.GetLastPowerUse().ToString();
        LastLanternSuccessPerTotal.text = ClientGameManager.Instance.GetLastLanternSuccess() + "/" + ClientGameManager.Instance.GetLastTotalLantern();
        LastRank.text = ClientGameManager.Instance.GetLastRank().ToString();
    }

    #endregion
}
