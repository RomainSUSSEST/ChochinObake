using SDD.Events;
using ServerManager;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultModel : MonoBehaviour
{
    #region Constants

    [SerializeField] TextMeshProUGUI TextPrinter;

    #endregion

    #region LifeCycle

    private void OnEnable()
    {
        RefreshListPlayer();
    }

    #endregion

    #region UI OnClick Button

    public void ButtonNextHasBeenPressed()
    {
        EventManager.Instance.Raise(new ViewResultEndEvent());
    }

    #endregion

    #region Tools
    private void RefreshListPlayer()
    {
        string text = "";

        IReadOnlyDictionary<ulong, Player> players = ServerGameManager.Instance.GetPlayers();
        IEnumerator<Player> enumPlayer = players.Values.GetEnumerator();

        while (enumPlayer.MoveNext())
        {
            text += enumPlayer.Current.Pseudo + " : " + enumPlayer.Current.Score + "\n";
        }

        IReadOnlyList<AI_Player> AI_Players = ServerGameManager.Instance.GetAIList();

        for (int i = 0; i < AI_Players.Count; ++i)
        {
            text +=  AI_Players[i].Name + " : " + AI_Players[i].Score + "\n";
        }

        TextPrinter.text = text;
    }
    #endregion
}
