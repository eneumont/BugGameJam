using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour {
    public void nextScene(string nextScn) {
        SceneManager.LoadScene(nextScn);
    }
}