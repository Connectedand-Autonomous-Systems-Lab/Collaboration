using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraRR2 : MonoBehaviour
{
    public string cameraTag = "";
    public float rotationSpeed = 10f;
    public float frame_number = 11f;
     


    private GameObject[] cameras;

    void Start()
    {
        cameras = GameObject.FindGameObjectsWithTag(cameraTag);
        Debug.Log("Started!");


    }

    void Update()
    {
        foreach (GameObject camera in cameras)
        {
             // Get the current rotation
              if (Time.frameCount % frame_number == 0)
        {
            Debug.Log("Time.frameCount is divisible by frame_number!");
            Vector3 currentRotation = camera.transform.rotation.eulerAngles;
             Debug.Log("currentRotation!"+ currentRotation + Time.frameCount);
             // Modify the rotation along the Y-axis
             currentRotation.y += 10f;
            // Apply the new rotation
            camera.transform.rotation = Quaternion.Euler(currentRotation);
        }

        }

    }
}


