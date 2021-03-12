using SDD.Events;
using System.Collections;
using UnityEngine;

public class ResultModel : MonoBehaviour
{
    #region Constants

    private static readonly float BACK_DELAI = 10;

    #endregion

    #region LifeCycle

    private void OnEnable()
    {
        StartCoroutine("BackToLobby");
    }

    #endregion

    #region Coroutine

    private IEnumerator BackToLobby()
    {
        yield return new WaitForSeconds(BACK_DELAI);

        EventManager.Instance.Raise(new ViewResultEndEvent());
    }

    #endregion
}
