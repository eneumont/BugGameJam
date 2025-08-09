using UnityEngine;

public static class GameProgress
{
    private const string ProgressKey = "HasProgressed";

    public static bool hasProgressed
    {
        get => PlayerPrefs.GetInt(ProgressKey, 0) == 1;
        set => PlayerPrefs.SetInt(ProgressKey, value ? 1 : 0);
    }
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(ProgressKey);
        PlayerPrefs.Save();
    }
}
