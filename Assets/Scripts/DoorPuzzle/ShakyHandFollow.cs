using UnityEngine;

public class ShakyHandFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private Vector3 followOffset = Vector3.zero; // relative point on the hand that follows the cursor

    [Header("Jitter Settings")]
    [SerializeField] private bool enableJitter = true;
    [SerializeField][Range(0f, 10f)] private float jitterAmount = 0.01f;
    [SerializeField][Range(0f, 5f)] private float jitterSpeed = 1.5f;

    [Header("Gizmo Settings")]
    [SerializeField] private Color gizmoColor = Color.red;
    [SerializeField] private float gizmoSize = 0.1f;

    private float noiseOffsetX;
    private float noiseOffsetY;

    private void Start()
    {
        noiseOffsetX = Random.Range(0f, 100f);
        noiseOffsetY = Random.Range(0f, 100f);
    }

    private void Update()
    {
        // Mouse world position
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // Apply optional jitter to the follow point
        Vector3 followTarget = mousePos;
        if (enableJitter)
        {
            float jitterX = (Mathf.PerlinNoise(Time.time * jitterSpeed + noiseOffsetX, 0f) - 0.5f) * jitterAmount;
            float jitterY = (Mathf.PerlinNoise(0f, Time.time * jitterSpeed + noiseOffsetY) - 0.5f) * jitterAmount;
            followTarget += new Vector3(jitterX, jitterY, 0f);
        }

        // Move the hand so that the followOffset point reaches the follow target
        Vector3 targetPos = followTarget - followOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        // Draw the point that is actually following the mouse
        Vector3 gizmoPos = transform.position + followOffset;
        Gizmos.DrawSphere(gizmoPos, gizmoSize);
    }
}
