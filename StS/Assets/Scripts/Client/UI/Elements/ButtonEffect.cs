using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse in");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse out");
    }
}
