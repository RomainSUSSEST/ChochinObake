using ClientManager;
using UnityEngine;

public class PanelInGameModel : MonoBehaviour
{
    [SerializeField] SlimeClient mySlimePrefab;
    [SerializeField] private Transform SlimeSpawn;

    private SlimeClient currentSlime;

    // Start is called before the first frame update
    void OnEnable()
    {
        if(ClientGameManager.Instance.GetCurrentBody() != null && ClientGameManager.Instance.GetCurrentHat() != null)
        {
            currentSlime = Instantiate(mySlimePrefab, SlimeSpawn.position, SlimeSpawn.rotation, SlimeSpawn);
            currentSlime.SetBody(ClientGameManager.Instance.GetCurrentBody());
            currentSlime.SetHat(ClientGameManager.Instance.GetCurrentHat());
        }
    }

    void OnDisable()
    {
        if (currentSlime != null)
        {
            Destroy(currentSlime.gameObject);
        }
    }
}
