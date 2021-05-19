using ClientManager;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PostGameModel : MonoBehaviour
{
    #region Attributes

    [SerializeField] private TextMeshProUGUI LastScore;
    [SerializeField] private TextMeshProUGUI LastBestCombo;
    [SerializeField] private TextMeshProUGUI LastPowerUse;
    [SerializeField] private TextMeshProUGUI LastLanternSuccessPerTotal;
    [SerializeField] private Image LastRank;
    [SerializeField] private List<Sprite> AllRanksSprite;

    #endregion

    #region Life cycle

    private void OnEnable()
    {
        LastScore.text = ClientGameManager.Instance.GetLastScore().ToString();
        LastBestCombo.text = ClientGameManager.Instance.GetLastBestCombo().ToString();
        LastPowerUse.text = ClientGameManager.Instance.GetLastPowerUse().ToString();
        LastLanternSuccessPerTotal.text = ClientGameManager.Instance.GetLastLanternSuccess() + "/" + ClientGameManager.Instance.GetLastTotalLantern();
        LastRank.sprite = AllRanksSprite[ClientGameManager.Instance.GetLastRank()];
    }

    #endregion
}
