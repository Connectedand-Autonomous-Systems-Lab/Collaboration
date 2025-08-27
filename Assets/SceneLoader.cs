using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        string envName = GameSettings.SelectedEnvironment;
        string envPath = "Environment/" + envName;
        GameObject prefab = Resources.Load<GameObject>(envPath);

        if (prefab != null)
        {
            Instantiate(prefab, Vector3.zero, Quaternion.identity);
            Debug.Log($"✅ Spawned {envName} in ActiveScene.");
        }
        else
        {
            Debug.LogError("❌ Could not load prefab: " + envPath);
        }
    }
}
