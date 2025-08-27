using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMountAssigner : MonoBehaviour
{
    [Header("Scene Camera Transforms")]
    public Transform[] cameraTransforms;  // Drag all predefined camera positions here

    void Awake()
    {
        if (cameraTransforms == null || cameraTransforms.Length == 0)
        {
            Debug.LogError("❌ No camera transforms assigned.");
            return;
        }

        SceneSettings.CameraTransforms = cameraTransforms;
        Debug.Log($"✅ Assigned {cameraTransforms.Length} camera transforms to SceneSettings.");
    }
}