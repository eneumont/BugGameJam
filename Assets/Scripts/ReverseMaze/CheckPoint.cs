using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckPoint : MonoBehaviour {
    [SerializeField] bool exit = false;

    void Start() {
        
    }

    void Update() {
        
    }

	private void OnTriggerEnter2D(Collider2D collision) {
        if (!exit) {
            if (collision.GetComponent<MazeCharacter>()) {
                collision.GetComponent<MazeCharacter>().checkPoint(transform.position);
            }
        } else {
            if (collision.GetComponent<MazeCharacter>().done) {
                SceneManager.LoadScene("");
            } else {
                
            }
        }
	}
}