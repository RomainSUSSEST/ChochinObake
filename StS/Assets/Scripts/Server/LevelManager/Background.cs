using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{

    [Header("Taille")]
    [SerializeField] private int NbrCase_Z = 1;

    public static readonly float DEFAULT_Z_SIZE = 2.5f;
    public static readonly float DEFAULT_SPEED = 2.5f;

    private static float MoveSpeed;
    private float ThresholdDestroyZ;

    public static void SetCurrentMoveSpeed(float v)
    {
        if (v <= 0)
        {
            throw new Exception("Moyenne incorrecte " + v);
        }

        MoveSpeed = v;
    }

    private void Start()
    {
        transform.Translate(new Vector3(-MoveSpeed * Time.deltaTime, 0, 0)); // Initialisation
        NbrCase_Z = NbrCase_Z * 10;
    }

    void Update()
    {

        // On décrémente la size en Y.
        --NbrCase_Z;

        if (NbrCase_Z == 0)
        {
            Destroy(this.gameObject);
        }

        transform.Translate(new Vector3(-MoveSpeed * Time.deltaTime, 0,0));
    }

    void DestroyBackGround() {
        Destroy(this.gameObject);
    }
}
