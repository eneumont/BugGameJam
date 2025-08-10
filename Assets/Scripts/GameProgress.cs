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

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(ProgressKey);
        PlayerPrefs.Save();
    }
}
