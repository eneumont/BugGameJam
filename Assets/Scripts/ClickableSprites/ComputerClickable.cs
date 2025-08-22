using UnityEngine;

public class ComputerClickable : ClickableSprite
{
    private void Awake()
    {
        if(!GameProgress.hasCompletedPaperGame) this.enabled = false;
    }
    protected override void OnClick()
	{
		Debug.Log("You found a computer");

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
