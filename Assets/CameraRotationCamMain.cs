using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class CameraRotationCamMain : MonoBehaviour
{
    private bool isRotating = false;
    private float targetRotation = 0f;
    private float rotationSpeed = 10f;

    private void Update()
    {
        // Check for user input or condition to trigger the rotation
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Set the target rotation based on the current rotation
            targetRotation = transform.eulerAngles.y + 10f;

            // Start rotating
            isRotating = true;
        }

        // Rotate the camera if the flag is set
        if (isRotating)
        {
            RotateCameraByYAxis();
        }
    }

    private void RotateCameraByYAxis()
    {
        // Get the current rotation
        Vector3 currentRotation = transform.eulerAngles;

        // Calculate the new rotation
        float newYRotation = Mathf.MoveTowardsAngle(currentRotation.y, targetRotation, rotationSpeed * Time.deltaTime);

        // Apply the new rotation
        transform.eulerAngles = new Vector3(currentRotation.x, newYRotation, currentRotation.z);

        // Check if the target rotation has been reached
        if (Mathf.Abs(newYRotation - targetRotation) < 0.01f)
        {
            // Stop rotating
            isRotating = false;
        }
    }
}


