using UnityEngine;

public class GoToNextSceneFromHitbox : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
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
