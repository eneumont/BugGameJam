using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider2D))]
public class CollectStapler : MonoBehaviour
{
    private bool playerInRange = false;
    private PlayerInput playerInput;

    void Reset()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("BoxCollider2D is not set to Trigger. Setting isTrigger = true.");
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerInput = other.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.actions["Interact"].performed += OnInteractPerformed;
            }
            Debug.Log("Press Interact to collect stapler.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (playerInput != null)
            {
                playerInput.actions["Interact"].performed -= OnInteractPerformed;
                playerInput = null;
            }
        }
    }

    private void OnInteractPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (playerInRange)
        {
            GameProgress.hasProgressed += 1;
            Debug.Log("Stapler collected! Progress increased to: " + GameProgress.hasProgressed);
            Destroy(gameObject);  // Optional: remove this object after collecting
        }
    }
}
