using UnityEngine;

public class MarkForSpawn : MonoBehaviour
{
    [Tooltip("Set the prefab to spawn if hasProgressed is true")]
    public GameObject prefabToSpawn;

    [Tooltip("Position to spawn the prefab")]
    public Vector3 spawnPosition;
    
    [Tooltip("The specific step they are in the game they need")]
    public int spawnNum;

    void Start()
    {
        if (GameProgress.hasProgressed == spawnNum)
        {
            SpawnObject();
        }
    }

    public void SpawnObject()
    {
        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("No prefab assigned to spawn.");
        }
    }
}
