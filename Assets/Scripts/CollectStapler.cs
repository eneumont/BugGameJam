using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
            // Replace this with your actual check for paper game completion
            if (GameProgress.hasCompletedPaperGame)
            {
				int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
				int nextSceneIndex = currentSceneIndex + 1;
				// Check if the next scene index is within bounds
				if (nextSceneIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
				{
					UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
				}
				else
				{
					Debug.LogWarning("No more scenes to load.");
				}
			}
        }
    }
}
