using UnityEngine;

public static class GameProgress
{
    private const string ProgressKey = "HasProgressed";

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
}
