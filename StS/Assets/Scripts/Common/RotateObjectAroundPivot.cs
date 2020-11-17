using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RotateObjectAroundPivot : MonoBehaviour
{
    // Attributs

    [Header("RotateObjectAroundPivot")]

    [SerializeField] private float RotateSpeed;
    [SerializeField] private Vector3 PivotInWorldSpace;
    [SerializeField] private Vector3 Direction;


    // Life cycle

    private void Start()
    {
        // On calcul le pivot centre de l'objet
        Renderer currentRenderer = GetComponent<Renderer>();

        if (currentRenderer != null) // Si l'objet posséde un renderer
        {
            PivotInWorldSpace = currentRenderer.bounds.center;
        } else // Si l'objet n'a pas de renderer, on calcul le pivot basé sur ces enfants et leur renderer.
        {
            Vector3? result = CenterPivotOnChildrenBasedOnRenderer(transform);

            if (result != null)
            {
                PivotInWorldSpace = CenterPivotOnChildrenBasedOnRenderer(transform).Value;
            } else
            { // Impossible de trouver un point de pivot basé sur les renderer, on prend la position du parent.
                PivotInWorldSpace = transform.position;
            }
        }

        Direction = Vector3.up;
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(PivotInWorldSpace, Direction, RotateSpeed * Time.deltaTime); 
    }


    // Requete

    public float GetRotateSpeed()
    {
        return RotateSpeed;
    }


    // Méthode

    public void SetRotateSpeed(float speed)
    {
        RotateSpeed = speed;
    }

    public void SetPivotInWorldSpace(Vector3 pivot)
    {
        PivotInWorldSpace = pivot;
    }


    // Outils

    private static Vector3? CenterPivotOnChildrenBasedOnRenderer(Transform parent)
    {
        List<Transform> childs = parent.Cast<Transform>().ToList(); // On récupere les enfants
       
        Vector3? pos = Vector3.zero;
        Renderer currentRenderer;
        int numberRendererFind = 0;

        Vector3? posChildren;
        foreach (Transform t in childs)
        {
            currentRenderer = t.gameObject.GetComponent<Renderer>();
            if (currentRenderer != null)
            {
                pos += currentRenderer.bounds.center;
                ++numberRendererFind;
            }

            posChildren = CenterPivotOnChildrenBasedOnRenderer(t);

            if (posChildren != null)
            {
                pos += posChildren;
                ++numberRendererFind;
            }
        }

        if (numberRendererFind > 0)
        {
            return pos /= numberRendererFind;
        } else
        {
            return null;
        }
    }
}
