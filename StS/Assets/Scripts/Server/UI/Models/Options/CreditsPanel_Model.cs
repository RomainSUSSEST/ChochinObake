using UnityEngine;

public class CreditsPanel_Model : MonoBehaviour
{
    #region Attributes

    [SerializeField] private GameObject Content;

    private Vector3 defaultPosition;

    #endregion

    #region Life Cycle

    private void Awake()
    {
        defaultPosition = Content.transform.position;
    }

    private void OnEnable()
    {
        Content.transform.position = defaultPosition;
    }

    #endregion
}
