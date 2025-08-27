using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEngine;
using UnityEngine.Perception.GroundTruth;

public class CameraRotationAutomation : MonoBehaviour
{
    public string cameraTag = "MainCamera";
    public float rotationSpeed = 10f;

    private GameObject[] cameras;
    
    void Start()
    {
        cameras = GameObject.FindGameObjectsWithTag(cameraTag);
    }

    void Update()
    {
        foreach (GameObject camera in cameras)
        {
            camera.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}




