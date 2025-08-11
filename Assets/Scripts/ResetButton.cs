using UnityEngine;

public class ResetButton : MonoBehaviour
{
    public void ResetLevel()
    {
        GameProgress.LoadPreviousScene();
    }
}
