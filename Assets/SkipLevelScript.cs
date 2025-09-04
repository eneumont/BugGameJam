using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipLevelScript : MonoBehaviour
{
    private static SkipLevelScript instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SkipLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int totalScenes = SceneManager.sceneCountInBuildSettings;
        int nextSceneIndex = (currentSceneIndex + 1) % totalScenes;

        SceneManager.LoadScene(nextSceneIndex);
    }
}
