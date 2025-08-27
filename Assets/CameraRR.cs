using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraRR : MonoBehaviour
{
    public string cameraTag = "MainCamera";
    public float rotationSpeed = 10f;
    public int frame_number = 10;
    public int counter = 1;

     


    private GameObject[] cameras;

    void Start()
    {
        cameras = GameObject.FindGameObjectsWithTag(cameraTag);
        


    }

    void Update()
    {
        foreach (GameObject camera in cameras)
        {
             // Get the current rotation
            var tt = (Time.frameCount%frame_number) ;
             
              var current_cal = 2+(counter-1)*frame_number;
              Debug.Log("Startedssssssss!" + current_cal+"  "+Time.frameCount);
              if ((int)Time.frameCount == (int)current_cal)
        {
            counter++;
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


