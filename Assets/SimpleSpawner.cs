using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public Transform spawnArea;  // Assign in inspector (acts as center)
    public Vector2 spawnSize = new Vector2(10, 10);  // X-Z plane
    public int numberToSpawn = 5;
    public float yOffset = 0.5f;

    void Start()
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            Vector3 randomPos = GetRandomPoint();
            Instantiate(prefabToSpawn, randomPos, Quaternion.identity);
        }

        Debug.Log($"✅ Spawned {numberToSpawn} prefab(s) in defined area.");
    }

    Vector3 GetRandomPoint()
    {
        Vector3 center = spawnArea != null ? spawnArea.position : Vector3.zero;

        float x = Random.Range(-spawnSize.x / 2f, spawnSize.x / 2f);
        float z = Random.Range(-spawnSize.y / 2f, spawnSize.y / 2f);

        return new Vector3(center.x + x, center.y + yOffset, center.z + z);
    }
}
