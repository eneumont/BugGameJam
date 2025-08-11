using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckPoint : MonoBehaviour {
    [SerializeField] bool exit = false;
    [SerializeField] bool control = false;

    int notDone = 10;
    Dictionary<int, float> notDoneTalk;

    void Start() {
        notDoneTalk = new Dictionary<int, float> {
            { 9, 3 },
            { 10, 3 }
        };
    }

    void Update() {
        
    }

	private void OnTriggerEnter2D(Collider2D collision) {
        collision.GetComponent<MazeCharacter>().newControls();
        if (!exit) {
            if (collision.GetComponent<MazeCharacter>()) {
                collision.GetComponent<MazeCharacter>().checkPoint(transform.position);
            }
        } else {
            if (collision.GetComponent<MazeCharacter>().done) {
                SceneManager.LoadScene("");
            } else {
                collision.GetComponent<HintSystem>().talk(notDone, 3f);
                notDone++;
                if (notDone > 14) notDone = 14;
            }
        }
	}
}