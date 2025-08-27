using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class VictimDetect : MonoBehaviour
{
    public Camera captureCamera;  
    public GameObject boundingBoxPrefab;
    public GameObject canvas;
    public bool autoIdentifyHumans = true;
    private bool findHumanInView = true;

    public List<GameObject> activeBoundingBoxes = new List<GameObject>();

    void Start()
    {
        SetupCanvas();
        InvokeRepeating("DetectFromCamera", 1.0f, 1.5f); // Every 1.5s
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            findHumanInView = true;
        }
    }

    void SetupCanvas()
    {
        Canvas cv = FindObjectOfType<Canvas>();
        if (cv != null)
        {
            canvas = cv.gameObject;
        }
    }

    void DetectFromCamera()
    {
        // TODO: Replace with your actual local detection logic
        // Simulated detection result (label x1 y1 x2 y2)
        string fakeDetections = "person 500 300 700 600\n";

        if (autoIdentifyHumans)
        {
            DrawBoundingBoxes(fakeDetections);
        }
        else if (findHumanInView)
        {
            UpdateWithUserKeyPress(fakeDetections);
        }
    }

    void DrawBoundingBoxes(string detections)
    {
        foreach (GameObject box in activeBoundingBoxes)
        {
            Destroy(box);
        }
        activeBoundingBoxes.Clear();

        string[] lines = detections.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(' ');
            if (parts[0] != "person") continue;

            float x1 = float.Parse(parts[1]);
            float y1 = float.Parse(parts[2]);
            float x2 = float.Parse(parts[3]);
            float y2 = float.Parse(parts[4]);

            float normalizedX = (x1 + x2) / 2;
            float normalizedY = (y1 + y2) / 2;

            GameObject bbox = Instantiate(boundingBoxPrefab, canvas.transform);
            bbox.GetComponent<RectTransform>().sizeDelta = new Vector2(x2 - x1, y2 - y1);
            bbox.GetComponent<RectTransform>().anchoredPosition = new Vector2(normalizedX, -normalizedY);

            activeBoundingBoxes.Add(bbox);
        }
    }

    void UpdateWithUserKeyPress(string detections)
    {
        Vector3 pos = Vector3.negativeInfinity;
        string[] lines = detections.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(' ');
            if (parts[0] != "person") continue;

            float x1 = float.Parse(parts[1]);
            float y1 = float.Parse(parts[2]);
            float x2 = float.Parse(parts[3]);
            float y2 = float.Parse(parts[4]);

            float normalizedX = (x1 + x2) / 2;
            float normalizedY = (y1 + y2) / 2;

            pos = GetHitPositionHumans(new Vector2(normalizedX, normalizedY));

            if (pos != Vector3.negativeInfinity)
                Debug.Log("Found Human in Scene at: " + pos);
            else
                Debug.Log("No Human found. Try different angle.");
        }

        findHumanInView = false;
    }

    public Vector3 GetHitPositionHumans(Vector2 screenPosition)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null && hit.collider.CompareTag("Human"))
            {
                return hit.collider.transform.position;
            }
        }
        return Vector3.negativeInfinity;
    }
}
