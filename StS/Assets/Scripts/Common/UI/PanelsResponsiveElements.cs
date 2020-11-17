using SDD.Events;
using UnityEngine;

public class PanelsResponsiveElements : MonoBehaviour
{
    // Attributs

    private RectTransform m_ParentRectTransform;
    private RectTransform rectTransform;

    #region Life Cycle
    private void Start()
    {
        EventManager.Instance.AddListener<ResizeUIEvent>(ResizeUI);

        m_ParentRectTransform = transform.parent.gameObject.GetComponent<RectTransform>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<ResizeUIEvent>(ResizeUI);
    }
    #endregion


    // Outils

    private void ResizeUI(ResizeUIEvent e)
    {
        if (m_ParentRectTransform == null)
        {
            throw new System.Exception("Erreur dans l'utilisation de PanelElements");
        } else
        {
            float ratioY = m_ParentRectTransform.rect.height / rectTransform.rect.height;
            float ratioX = m_ParentRectTransform.rect.width / rectTransform.rect.width;
            rectTransform.localScale = new Vector3(ratioX, ratioY, 1);
        }
    }
}
