using UnityEngine;

public class FileSpawner : MonoBehaviour
{
    [Header("File Prefabs")]
    public GameObject goodFilePrefab;
    public GameObject badFilePrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 1f;
    [Range(0f, 1f)] public float goodFileChance = 0.2f; // 20% good files

    [Header("Movement Settings")]
    public float minFallSpeed = 2f;
    public float maxFallSpeed = 5f;

    [Header("Spawn Area")]
    public float minX = -8f;
    public float maxX = 8f;
    public float spawnY = 6f;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnFile), 0f, spawnInterval);
    }

    private void SpawnFile()
    {
        // Decide if this file is good or bad
        bool isGoodFile = Random.value < goodFileChance;
        GameObject prefabToSpawn = isGoodFile ? goodFilePrefab : badFilePrefab;

        // Pick a random spawn position
        Vector3 spawnPos = new Vector3(Random.Range(minX, maxX), spawnY, 0f);

        // Create the file
        GameObject file = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // Assign a random fall speed
        float speed = Random.Range(minFallSpeed, maxFallSpeed);
        FallingFile fallingFile = file.GetComponent<FallingFile>();
        if (fallingFile != null)
        {
            fallingFile.SetFallSpeed(speed);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Set gizmo color
        Gizmos.color = Color.yellow;

        // Draw the spawn line in the scene view
        Vector3 leftPoint = new Vector3(minX, spawnY, 0f);
        Vector3 rightPoint = new Vector3(maxX, spawnY, 0f);
        Gizmos.DrawLine(leftPoint, rightPoint);

        // Draw small markers at the ends
        Gizmos.DrawSphere(leftPoint, 0.1f);
        Gizmos.DrawSphere(rightPoint, 0.1f);
    }
}
