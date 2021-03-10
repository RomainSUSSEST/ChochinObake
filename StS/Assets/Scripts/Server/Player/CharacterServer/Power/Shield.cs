using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    #region Attributs

    [SerializeField] private List<GameObject> ShieldElements;
    [SerializeField] private float Speed;

    #endregion

    #region Life Cycle

    private void Update()
    {
        foreach (GameObject g in ShieldElements)
        {
            g.transform.RotateAround(transform.position, Vector3.up, Speed * Time.deltaTime);
        }
    }

    #endregion
}
