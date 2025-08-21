using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckPoint : MonoBehaviour {
    [SerializeField] bool exit = false;
    [SerializeField] bool control = false;

    int notDone = 11;

	private void OnTriggerEnter2D(Collider2D collision) {
        collision.gameObject.GetComponent<MazeCharacter>().newControls();
        if (!exit) {
            if (GameProgress.HardMode) return;
            if (collision.gameObject.GetComponent<MazeCharacter>()) {
                collision.gameObject.GetComponent<MazeCharacter>().checkPoint(transform.position);
            }
        } else {
            if (collision.gameObject.GetComponent<MazeCharacter>().done) {
				int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
				int nextSceneIndex = currentSceneIndex + 1;
				// Check if the next scene index is within bounds
				if (nextSceneIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
				{
					UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
				}
			} else {
				FindFirstObjectByType<HintSystem>().talk(notDone);
                notDone++;
                if (notDone > 15) notDone = 15;
            }
        }
	}
}