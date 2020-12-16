using TMPro;
using UnityEngine;

public class Value : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    public void SetText(string msg)
    {
        text.text = msg;
    }
}
