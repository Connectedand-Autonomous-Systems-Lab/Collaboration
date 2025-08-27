using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public GameObject arena1Prefab;
    public GameObject arena2Prefab;

    private GameObject currentArena;

    public Light sceneLight;
    public Camera mainCamera;

    // Call this externally with user choice
    public void LoadEnvironment(int selection)
    {
        if (currentArena != null)
        {
            Destroy(currentArena);
        }

        switch (selection)
        {
            case 1:
                currentArena = Instantiate(arena1Prefab, Vector3.zero, Quaternion.identity);
                ConfigureLighting(Color.white, 1.2f);
                PositionCamera(new Vector3(0, 10, -10), Quaternion.Euler(30, 0, 0));
                break;

            case 2:
                currentArena = Instantiate(arena2Prefab, Vector3.zero, Quaternion.identity);
                ConfigureLighting(Color.red, 0.8f);
                PositionCamera(new Vector3(0, 15, -15), Quaternion.Euler(45, 0, 0));
                break;

            default:
                Debug.LogWarning("Invalid environment selection.");
                break;
        }
    }

    private void ConfigureLighting(Color color, float intensity)
    {
        if (sceneLight != null)
        {
            sceneLight.color = color;
            sceneLight.intensity = intensity;
        }
    }

    private void PositionCamera(Vector3 position, Quaternion rotation)
    {
        if (mainCamera != null)
        {
            mainCamera.transform.position = position;
            mainCamera.transform.rotation = rotation;
        }
    }
}
