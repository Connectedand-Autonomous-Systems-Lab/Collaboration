using UnityEngine;

public class MultipleSpawner : MonoBehaviour
{
    [Header("Prefabs to spawn")]
    public GameObject[] prefabsToSpawn;  // Add multiple prefabs here

    [Header("Spawn Area")]
    public Transform spawnArea;  // Center of spawn region
    public Vector2 spawnSize = new Vector2(10, 10);  // Size of spawn region (X-Z)

    [Header("Spawn Settings")]
    public int numberToSpawn = GameSettings.NumberOfHumans;
    public float yOffset = 0.5f;

    void Start()
    {
        if (prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        {
            Debug.LogError("❌ No prefabs assigned in SimpleSpawner.");
            return;
        }

        for (int i = 0; i < numberToSpawn; i++)
        {
            SpawnRandomPrefab();
        }

        Debug.Log($"✅ Spawned {numberToSpawn} prefabs randomly.");
    }

    void SpawnRandomPrefab()
    {
        // Pick a random prefab from the list
        GameObject prefab = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)];
        Vector3 spawnPos = GetRandomPoint();

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    Vector3 GetRandomPoint()
    {
        Vector3 center = spawnArea != null ? spawnArea.position : Vector3.zero;

        float x = Random.Range(-spawnSize.x / 2f, spawnSize.x / 2f);
        float z = Random.Range(-spawnSize.y / 2f, spawnSize.y / 2f);

        return new Vector3(center.x + x, center.y + yOffset, center.z + z);
    }
}
