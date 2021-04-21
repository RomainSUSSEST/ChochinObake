using UnityEngine;

public abstract class Effect : MonoBehaviour
{
    #region LifeCycle

    private void OnEnable()
    {
        StartEffect();
    }

    #endregion

    #region tools

    protected abstract void StartEffect();

    #endregion
}
