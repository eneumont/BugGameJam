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
            if (collision.gameObject.GetComponent<MazeCharacter>()) {
                collision.gameObject.GetComponent<MazeCharacter>().checkPoint(transform.position);
            }
        } else {
            if (collision.gameObject.GetComponent<MazeCharacter>().done) {
                SceneManager.LoadScene("");
            } else {
				FindFirstObjectByType<HintSystem>().talk(notDone);
                notDone++;
                if (notDone > 15) notDone = 15;
            }
        }
	}
}