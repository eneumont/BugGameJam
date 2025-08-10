using UnityEngine;

[RequireComponent(typeof(TalkingController))]
public class GoToNextVisualNovelScene : MonoBehaviour
{
	private TalkingController talk;

	private void Awake()
	{
		talk = GetComponent<TalkingController>();
	}

	private void Update()
	{
		if (talk.done)
		{
			GoToNextScene();
		}
	}

	private void GoToNextScene()
	{
		// Load the next scene in the build settings
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
