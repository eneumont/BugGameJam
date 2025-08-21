using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameProgress
{
    private const string ProgressKey = "HasProgressed";
    private const string PreviousSceneKey = "PreviousScene";


    public static int hasProgressed
    {
        get => PlayerPrefs.GetInt(ProgressKey, 0);
        set
        {
            PlayerPrefs.SetInt(ProgressKey, value);
            PlayerPrefs.Save();
        }
    }
    public static bool hasCompletedPaperGame
    {
        get => _completedPaperGame;
        set => _completedPaperGame = value;
    }

    // Backing field for the property
    private static bool _completedPaperGame = false;
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(ProgressKey);
        PlayerPrefs.Save();
    }

    // Save the current scene name as the previous scene
    public static void SaveCurrentSceneAsPrevious()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString(PreviousSceneKey, currentSceneName);
        PlayerPrefs.Save();
    }

	public static void SaveCurrentSceneAsPreviousByName(string SceneName)
	{
		PlayerPrefs.SetString(PreviousSceneKey, SceneName);
		PlayerPrefs.Save();
	}

	// Load the stored previous scene
	public static void LoadPreviousScene()
    {
        if (PlayerPrefs.HasKey(PreviousSceneKey))
        {
            string previousSceneName = PlayerPrefs.GetString(PreviousSceneKey);
            SceneManager.LoadScene(previousSceneName);
        }
        else
        {
            Debug.LogWarning("No previous scene saved!");
        }
    }
}
