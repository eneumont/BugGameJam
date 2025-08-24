using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonSpawner : MonoBehaviour
{
    [Header("Prefab & Scene")]
    [SerializeField] private GameObject prefabToSpawn; // prefab to instantiate
    [SerializeField] private Transform spawnParent;    // optional parent for prefab
    [SerializeField] private Vector3 spawnPosition;    // where to spawn
    [SerializeField] private string sceneToLoad;       // scene name to load on click

    private void Start()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogError("Prefab not assigned!");
            return;
        }

        GameObject instance = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, spawnParent);

        // Ensure the prefab has a SceneButton component
        SceneButton button = instance.GetComponent<SceneButton>();
        if (button == null)
        {
            button = instance.AddComponent<SceneButton>();
        }

        // Assign scene name
        button.sceneToLoad = sceneToLoad;
    }
}
