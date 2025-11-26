using System.Linq;
using UnityEngine;
using System.Collections.Generic;
// using UnityEngine.Perception;
using UnityEngine.Perception.GroundTruth;

public class DisableHumanInSight : MonoBehaviour
{
    public Camera cam;                // Assign your camera in Inspector
    public LayerMask obstacleMask; // Layers that can block view (e.g. Walls, Default)

    void Update()
    {
        // Press C to disable the first active GameObject tagged "human" that is in this camera's view.
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (cam == null)
                cam = GetComponent<Camera>() ?? Camera.main;

            var humans = GameObject.FindGameObjectsWithTag("Human");
            bool disabledOne = false;

            foreach (var h in humans)
            {
                if (h == null || !h.activeInHierarchy)
                    continue;

                if (IsReallyVisible(h.transform, cam))
                {
                    h.SetActive(false);
                    // Debug.Log($"Disabled human: {h.name}");
                }
                else
                {
                    // Debug.Log(h.name + " is NOT visible");
                }
            }

        }
    }

    bool IsReallyVisible(Transform obj, Camera cam)
    {
        if (obj == null || cam == null)
            return false;

        // 1) Check if it's in the camera frustum (viewport)
        Vector3 viewportPos = cam.WorldToViewportPoint(obj.position);

        // Behind camera?
        if (viewportPos.z <= 0)
            return false;

        // Outside screen?
        if (viewportPos.x < 0 || viewportPos.x > 1 ||
            viewportPos.y < 0 || viewportPos.y > 1)
            return false;

        // 2) Raycast from camera to the object to check occlusion
        Vector3 dir = obj.position - cam.transform.position;
        float dist = dir.magnitude;

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, dir.normalized, out hit, dist, obstacleMask))
        {
            // Something is in the way before we reach the object
            if (hit.transform != obj && !hit.transform.IsChildOf(obj))
            {
                return false;
            }
        }

        // No obstacle OR the first thing hit is the object
        return true;
    }

}
