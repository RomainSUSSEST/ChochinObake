using SDD.Events;
using ServerManager;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ResultModel : MonoBehaviour
{
    #region Constants

    private static readonly int MARGIN_HEIGHT = 5; // En px

    #endregion

    #region Attributs

    [SerializeField] private RectTransform ViewPanel;
    [SerializeField] private GameObject ContentNode;
    [SerializeField] private ServerResultDefaultItem DefaultItemPrefab;

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
        #region Clear

        foreach (Transform Child in ContentNode.transform)
        {
            Destroy(Child.gameObject);
        }

        #endregion

        #region Generate

        IReadOnlyDictionary<ulong, Player> players = ServerGameManager.Instance.GetPlayers();
        List<ServerResultDefaultItem> list = new List<ServerResultDefaultItem>();
        ServerResultDefaultItem tampon;

        foreach (Player p in players.Values)
        {
            tampon = Instantiate(DefaultItemPrefab, ContentNode.transform);

            tampon.Pseudo.text = p.Pseudo;
            tampon.Score.text = p.LastGameScore.ToString();

            list.Add(tampon);
        }

        IReadOnlyList<AI_Player> ais = ServerGameManager.Instance.GetAIList();
        foreach (AI_Player ai in ais)
        {
            tampon = Instantiate(DefaultItemPrefab, ContentNode.transform);

            tampon.Pseudo.text = "Chochin " + ai.Name;
            tampon.Score.text = ai.LastGameScore.ToString();

            list.Add(tampon);
        }

        #endregion

        #region Sort

        list.Sort((ServerResultDefaultItem x, ServerResultDefaultItem y) =>
            Int32.Parse(y.Score.text) - Int32.Parse(x.Score.text)
        );

        #endregion

        #region Placement

        float areaHeight = DefaultItemPrefab.GetComponent<RectTransform>().rect.height;
        float currentMarginHeight = MARGIN_HEIGHT;

        // On tient compte du rescale de la vue
        areaHeight *= ViewPanel.lossyScale.y;
        currentMarginHeight *= ViewPanel.lossyScale.y;

        // On estime la hauteur à allouer
        float height = (list.Count + 1) * currentMarginHeight
               + list.Count * areaHeight;

        height /= ViewPanel.lossyScale.y;

        // On redimenssionne le content
        RectTransform contentRectTransform = ContentNode.GetComponent<RectTransform>();
        contentRectTransform.sizeDelta = new Vector2
            (
            contentRectTransform.rect.width,
            height
            );

        // Position de départ

        Vector3 currentPositionButtonSpawn = new Vector3
            (
                contentRectTransform.position.x,
                (contentRectTransform.position.y
                    + height * ViewPanel.lossyScale.y / 2 - currentMarginHeight - areaHeight / 2),
                contentRectTransform.position.z
            );

        // On les affiches

        foreach (ServerResultDefaultItem srdi in list)
        {
            srdi.transform.position = currentPositionButtonSpawn;
            currentPositionButtonSpawn -= new Vector3(0, areaHeight + currentMarginHeight, 0);
        }

        // On décale le content pour afficher le premier en haut
        contentRectTransform.localPosition = new Vector3
            (
            contentRectTransform.localPosition.x,
            -height / 2 - contentRectTransform.parent.GetComponent<RectTransform>().rect.height,
            contentRectTransform.localPosition.z
            );

        #endregion
    }
    #endregion
}
