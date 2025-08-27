using UnityEngine;

public class CameraSpawner : MonoBehaviour
{
    [Header("Prefab to Spawn at All Camera Points")]
    public GameObject prefabToSpawn;  // Just one prefab

    void Start()
    {
        Transform[] spawnPoints = SceneSettings.CameraTransforms;

        if (prefabToSpawn == null)
        {
            Debug.LogError("❌ No prefab assigned.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("❌ SceneSettings.CameraTransforms is empty or not set.");
            return;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform point = spawnPoints[i];

            GameObject spawned = Instantiate(prefabToSpawn, point.position, point.rotation);
            spawned.transform.rotation = point.rotation; // ✅ Apply correct rotation manually
            spawned.name = $"Camera_{i + 1}";
        }

        Debug.Log($"✅ Spawned {spawnPoints.Length} instances of '{prefabToSpawn.name}' at predefined positions.");
    }
}
