using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class TravelBox : MonoBehaviour
{
    [Tooltip("Minimum progress required to travel")]
    public int requiredProgress = 1;

    [Tooltip("Name of the scene to load when triggered")]
    public string sceneToLoad;

    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player in range. Press 'E' to travel.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
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
