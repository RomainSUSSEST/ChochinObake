using UnityEngine;

public class PanelsResponsiveElementsOnEnable : MonoBehaviour
{
    // Attributs

    private RectTransform m_ParentRectTransform;
    private RectTransform rectTransform;


    // Life Cycle

    private void Awake()
    {
        m_ParentRectTransform = transform.parent.gameObject.GetComponent<RectTransform>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        ResizeUI();
    }


    // Outils

    private void ResizeUI()
    {
        if (m_ParentRectTransform == null)
        {
            throw new System.Exception("Erreur dans l'utilisation de PanelElements");
        }
        else
        {
            float ratioY = m_ParentRectTransform.rect.height / rectTransform.rect.height;
            float ratioX = m_ParentRectTransform.rect.width / rectTransform.rect.width;
            rectTransform.localScale = new Vector3(ratioX, ratioY, 1);
        }
    }
}
