using TMPro;
using UnityEngine;

public class ASuppr : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    public void SetText(string msg)
    {
        text.text = msg;
    }

    private void Start()
    {
        Destroy(this.gameObject, 2);
    }

    private void Update()
    {
        transform.Translate(new Vector3(0, 4f, 0) * Time.deltaTime);
    }
}
