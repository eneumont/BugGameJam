using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;  // Requires new Input System package

[RequireComponent(typeof(BoxCollider2D))]
public class TravelBox : MonoBehaviour
{
    [Tooltip("Minimum progress required to travel")]
    public int requiredProgress = 1;

    [Tooltip("Name of the scene to load when triggered")]
    public string sceneToLoad;

    private bool playerInRange = false;

    private PlayerInput playerInput;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // Try to get PlayerInput component from player
            playerInput = other.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.actions["Interact"].performed += OnInteractPerformed;
            }

            Debug.Log("Player in range. Press Interact to travel.");
        }
    }

    private void OnTriggerExit(Collider other)
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

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (playerInRange)
        {
            if (GameProgress.hasProgressed >= requiredProgress)
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.Log("You need more progress to travel!");
            }
        }
    }
}
