using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[RequireComponent(typeof(SpriteRenderer))]
public class MouseAvoider_EventDriven : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float avoidDistance = 2f;
    public float sineFrequency = 5f; // How fast it oscillates
    public float sineAmplitude = 0.5f; // How far it sways

    private SpriteRenderer spriteRenderer;
    private Camera mainCam;
    private Vector3 mouseWorldPos;
    private bool isAvoiding = false;
    private float sineTime = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;

        // Subscribe to mouse move events
        InputSystem.onEvent += OnInputEvent;
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        InputSystem.onEvent -= OnInputEvent;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (device is Mouse mouse && eventPtr.IsA<StateEvent>())
        {
            // Convert to world space
            Vector2 mouseScreenPos = mouse.position.ReadValue();
            mouseWorldPos = mainCam.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -mainCam.transform.position.z));

            // Keep same Z as this object
            mouseWorldPos.z = transform.position.z;

            // Flip sprite
            spriteRenderer.flipX = mouseWorldPos.x >= transform.position.x;

            // Check if we should start moving
            isAvoiding = Vector3.Distance(transform.position, mouseWorldPos) < avoidDistance;
            if (isAvoiding) sineTime = 0f; // Reset wave
        }
    }

    void Update()
    {
        if (!isAvoiding) return;

        sineTime += Time.deltaTime * sineFrequency;

        // Main direction away from mouse
        Vector3 directionAway = (transform.position - mouseWorldPos).normalized;

        // Perpendicular vector for sine offset
        Vector3 perpendicular = new Vector3(-directionAway.y, directionAway.x, 0);

        // Add sine wave motion
        Vector3 sineOffset = perpendicular * Mathf.Sin(sineTime) * sineAmplitude;

        // Move away + wave offset
        transform.position += (directionAway * moveSpeed * Time.deltaTime) + (sineOffset * Time.deltaTime);

        // Keep on screen
        ClampToScreen();
    }

    void ClampToScreen()
    {
        Vector3 pos = transform.position;
        Vector3 minBounds = mainCam.ScreenToWorldPoint(Vector3.zero);
        Vector3 maxBounds = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        float halfWidth = spriteRenderer.bounds.size.x / 2f;
        float halfHeight = spriteRenderer.bounds.size.y / 2f;

        pos.x = Mathf.Clamp(pos.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
        pos.y = Mathf.Clamp(pos.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);

        transform.position = pos;
    }
}
