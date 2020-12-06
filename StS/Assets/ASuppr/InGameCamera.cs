using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameCamera : MonoBehaviour
{
    float skyboxStartAngle = 30f;
    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time);
    }
}
