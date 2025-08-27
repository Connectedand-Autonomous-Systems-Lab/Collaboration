using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public int numberOfPrefabs = 1;
    public float yOffset = 0.5f;  // Lift above floor to prevent clipping
    public string floorTag = "Floor"; // Optional: set a tag like "Floor" to help filter

    void Start()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogError("❌ No prefab assigned to spawn.");
            return;
        }

        GameObject floor = AutoDetectFloor();
        if (floor == null)
        {
            Debug.LogError("❌ No floor detected in scene.");
            return;
        }

        SpawnOnFloor(floor);
    }

    GameObject AutoDetectFloor()
    {
        // First try tagged object if tag is specified
        if (!string.IsNullOrEmpty(floorTag))
        {
            try
            {
                GameObject taggedFloor = GameObject.FindWithTag(floorTag);
                if (taggedFloor != null)
                {
                    Debug.Log("✅ Found floor using tag: " + floorTag);
                    return taggedFloor;
                }
                else{Debug.Log("Tagged floor is null");}
            }
            catch
            {
                Debug.LogWarning("⚠️ Tag '" + floorTag + "' is not defined in Tag Manager.");
            }
        }

        // Fallback: Find the widest flat collider
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        GameObject bestCandidate = null;
        float largestArea = 0f;

        foreach (var obj in allObjects)
        {
            if (!obj.activeInHierarchy)
                continue;

            Collider col = obj.GetComponent<Collider>();
            if (col == null)
                continue;

            Bounds bounds = col.bounds;

            // Check if it's wide and flat (not too thick in Y)
            if (bounds.size.y < 1f)
            {
                float area = bounds.size.x * bounds.size.z;
                if (area > largestArea)
                {
                    bestCandidate = obj;
                    largestArea = area;
                    Debug.Log($"👀 Considering '{obj.name}' as potential floor (area: {area})");
                }
            }
        }

        if (bestCandidate != null)
        {
            Debug.Log("✅ Auto-detected floor: " + bestCandidate.name);
        }

        return bestCandidate;
    }


    void SpawnOnFloor(GameObject floor)
    {
        Collider floorCollider = floor.GetComponent<Collider>();
        if (floorCollider == null)
        {
            Debug.LogError("❌ Floor does not have a collider.");
            return;
        }

        Bounds bounds = floorCollider.bounds;

        for (int i = 0; i < numberOfPrefabs; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.max.y + yOffset,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            Instantiate(prefabToSpawn, randomPos, Quaternion.identity);
        }

        Debug.Log($"✅ Spawned {numberOfPrefabs} prefab(s) on auto-detected floor: {floor.name}");
    }
}
