using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetButton : MonoBehaviour
{
    public void ResetLevel()
    {
        GameProgress.LoadPreviousScene();
    }
    public void ReturnToTitle() 
    {
        GameProgress.ResetProgress();
        SceneManager.LoadScene("TitleScn");
    }
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
    
}
