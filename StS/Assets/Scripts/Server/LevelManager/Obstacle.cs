using SDD.Events;
using System;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    // Constante

    public static readonly string TAG = "Obstacle";

    public static readonly float DEFAULT_SPEED = 2.5f;

    private static float MoveSpeed; // Units/Seconde
    private static Transform MapStart;


    // Attributs

    private float EndPosition;


    // Static ---


    // Requete

    public static float GetCurrentMoveSpeed()
    {
        return MoveSpeed;
    }


    // Méthode

    /**
     * @pre v > 0
     * @post GetCurrentMoveSpeed() == v
     */
    public static void SetCurrentMoveSpeed(float v)
    {
        if (v <= 0)
        {
            throw new Exception("Vitesse refusé " + v);
        }

        MoveSpeed = v;
    }

    public static void SetMapStart(Transform start)
    {
        MapStart = start;
    }
    // ---


    // Life Cycle

    void Update()
    {
        if (transform.position.z <= EndPosition)
        {
            EventManager.Instance.Raise(new ObstacleEndMapEvent());
            Destroy(this.gameObject);
        }

        transform.Translate(new Vector3(0, 0, -MoveSpeed * Time.deltaTime));
    }

    private void Start()
    {
        transform.Translate(new Vector3(0, 0, -MoveSpeed * Time.deltaTime)); // Initialisation
        EndPosition = MapStart.position.z - GetComponent<BoxCollider>().bounds.size.z;
    }
}