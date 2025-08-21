using UnityEngine;

public class FileSpawner : MonoBehaviour
{
    [Header("File Prefabs")]
    public GameObject filePrefab;

    [Header("Sprites")]
    public Sprite goodFileSprite;
    public Sprite badFileSprite;

    [Header("Spawn Settings")]
    public float spawnInterval = 1f;
    [Range(0f, 1f)] public float goodFileChance = 0.2f;

    [Header("Movement Settings")]
    public float minFallSpeed = 2f;
    public float maxFallSpeed = 5f;

    [Header("Spawn Area")]
    public float minX = -8f;
    public float maxX = 8f;
    public float spawnY = 6f;

    private float timeSinceLastGoodFile = 0f;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnFile), 0f, spawnInterval);
    }

    private void Update()
    {
        timeSinceLastGoodFile += Time.deltaTime;
    }

    private void SpawnFile()
    {
        bool isGoodFile;

        // Force a good file if none spawned in 15 seconds
        if (timeSinceLastGoodFile >= 15f)
        {
            isGoodFile = true;
            timeSinceLastGoodFile = 0f; // reset timer
        }
        else
        {
            isGoodFile = Random.value < goodFileChance;
            if (isGoodFile)
                timeSinceLastGoodFile = 0f;
        }

        // Pick a random spawn position
        Vector3 spawnPos = new Vector3(Random.Range(minX, maxX), spawnY, 0f);

        // Create the file
        GameObject file = Instantiate(filePrefab, spawnPos, Quaternion.identity);

        // Assign FileData properties
        FileData fileData = file.GetComponent<FileData>();
        if (fileData != null)
        {
            fileData.isGoodFile = isGoodFile;

            // Change sprite based on type
            SpriteRenderer sr = file.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = isGoodFile ? goodFileSprite : badFileSprite;
            }
        }

        // Assign a random fall speed
        float speed = Random.Range(minFallSpeed, maxFallSpeed);

        // Increase fall speed slightly if Hard Mode is enabled
        if (GameProgress.HardMode)
        {
            speed *= 1.2f;
        }

        FallingFile fallingFile = file.GetComponent<FallingFile>();
        if (fallingFile != null)
        {
            fallingFile.SetFallSpeed(speed);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 leftPoint = new Vector3(minX, spawnY, 0f);
        Vector3 rightPoint = new Vector3(maxX, spawnY, 0f);
        Gizmos.DrawLine(leftPoint, rightPoint);
        Gizmos.DrawSphere(leftPoint, 0.1f);
        Gizmos.DrawSphere(rightPoint, 0.1f);
    }
}
