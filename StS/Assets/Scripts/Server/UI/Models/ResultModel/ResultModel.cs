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
    [SerializeField] private GameObject ContentNodeDefault;
    [SerializeField] private ServerResultDefaultItem DefaultItemPrefab;

    [Header("Info with details")]
    [SerializeField] private GameObject PanelWithDetails;
    [SerializeField] private GameObject ContentNodeDetails;
    [SerializeField] private ServerResultDetailsItem DetailsItemPrefab;

    #endregion

    #region LifeCycle

    private void OnEnable()
    {
        RefreshListPlayer();
        PanelWithDetails.SetActive(false);
    }

    #endregion

    #region UI OnClick Button

    public void ButtonNextHasBeenPressed()
    {
        EventManager.Instance.Raise(new ViewResultEndEvent());
    }

    public void MoreDetailsHasBeenPressed()
    {
        PanelWithDetails.SetActive(true);
    }

    public void LessDetailsHasBeenPressed()
    {
        PanelWithDetails.SetActive(false);
    }

    #endregion

    #region Tools
    private void RefreshListPlayer()
    {
        #region Clear

        foreach (Transform Child in ContentNodeDefault.transform)
        {
            Destroy(Child.gameObject);
        }

        foreach (Transform Child in ContentNodeDetails.transform)
        {
            Destroy(Child.gameObject);
        }

        #endregion

        #region Generate

        IReadOnlyDictionary<ulong, Player> players = ServerGameManager.Instance.GetPlayers();
        IReadOnlyList<AI_Player> ais = ServerGameManager.Instance.GetAIList();

        #region Default


        List<ServerResultDefaultItem> listDefault = new List<ServerResultDefaultItem>();
        ServerResultDefaultItem tamponDefault;

        foreach (Player p in players.Values)
        {
            tamponDefault = Instantiate(DefaultItemPrefab, ContentNodeDefault.transform);

            tamponDefault.Pseudo.text = p.Pseudo;
            tamponDefault.Score.text = p.LastGameScore.ToString();

            listDefault.Add(tamponDefault);
        }

        foreach (AI_Player ai in ais)
        {
            tamponDefault = Instantiate(DefaultItemPrefab, ContentNodeDefault.transform);

            tamponDefault.Pseudo.text = "Chochin " + ai.Name;
            tamponDefault.Score.text = ai.LastGameScore.ToString();

            listDefault.Add(tamponDefault);
        }

        #endregion

        #region Details

        List<ServerResultDetailsItem> listDetails = new List<ServerResultDetailsItem>();
        ServerResultDetailsItem tamponDetails;

        foreach (Player p in players.Values)
        {
            tamponDetails = Instantiate(DetailsItemPrefab, ContentNodeDetails.transform);

            tamponDetails.Pseudo.text = p.Pseudo.ToString();
            tamponDetails.Lantern.text = p.LastGameLanternSuccess + "/" + p.LastGameTotalLantern;
            tamponDetails.Power.text = p.LastGamePowerUse.ToString();
            tamponDetails.Combo.text = p.LastGameBestCombo.ToString();
            tamponDetails.Score = p.LastGameScore;

            listDetails.Add(tamponDetails);
        }

        foreach (AI_Player ai in ais)
        {
            tamponDetails = Instantiate(DetailsItemPrefab, ContentNodeDetails.transform);

            tamponDetails.Pseudo.text = "Chochin " + ai.Name;
            tamponDetails.Lantern.text = ai.LastGameLanternSuccess + "/" + ai.LastGameTotalLantern;
            tamponDetails.Power.text = ai.LastGamePowerUse.ToString();
            tamponDetails.Combo.text = ai.LastGameBestCombo.ToString();
            tamponDetails.Score = ai.LastGameScore;

            listDetails.Add(tamponDetails);
        }

        #endregion

        #endregion

        #region Sort

        #region Default

        listDefault.Sort((ServerResultDefaultItem x, ServerResultDefaultItem y) =>
            Int32.Parse(y.Score.text) - Int32.Parse(x.Score.text)
        );

        #endregion

        #region Details

        listDetails.Sort((ServerResultDetailsItem x, ServerResultDetailsItem y) =>
            y.Score - x.Score
        );

        #endregion

        #endregion

        #region Placement

        #region Default

        float areaHeight = DefaultItemPrefab.GetComponent<RectTransform>().rect.height;
        float currentMarginHeight = MARGIN_HEIGHT;

        // On tient compte du rescale de la vue
        areaHeight *= ViewPanel.lossyScale.y;
        currentMarginHeight *= ViewPanel.lossyScale.y;

        // On estime la hauteur à allouer
        float height = (listDefault.Count + 1) * currentMarginHeight
               + listDefault.Count * areaHeight;

        height /= ViewPanel.lossyScale.y;

        // On redimenssionne le content
        RectTransform contentRectTransform = ContentNodeDefault.GetComponent<RectTransform>();
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

        foreach (ServerResultDefaultItem srdi in listDefault)
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

        #region Details

        areaHeight = DetailsItemPrefab.GetComponent<RectTransform>().rect.height;
        currentMarginHeight = MARGIN_HEIGHT;

        // On tient compte du rescale de la vue
        areaHeight *= ViewPanel.lossyScale.y;
        currentMarginHeight *= ViewPanel.lossyScale.y;

        // On estime la hauteur à allouer
        height = (listDetails.Count + 1) * currentMarginHeight
               + listDetails.Count * areaHeight;

        height /= ViewPanel.lossyScale.y;

        // On redimenssionne le content
        contentRectTransform = ContentNodeDetails.GetComponent<RectTransform>();
        contentRectTransform.sizeDelta = new Vector2
            (
            contentRectTransform.rect.width,
            height
            );

        // Position de départ

        currentPositionButtonSpawn = new Vector3
            (
                contentRectTransform.position.x,
                (contentRectTransform.position.y
                    + height * ViewPanel.lossyScale.y / 2 - currentMarginHeight - areaHeight / 2),
                contentRectTransform.position.z
            );

        // On les affiches

        foreach (ServerResultDetailsItem srdi in listDetails)
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

        #endregion
    }
    #endregion
}
