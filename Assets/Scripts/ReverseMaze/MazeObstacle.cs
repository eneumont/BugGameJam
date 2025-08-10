using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MazeObstacle : MonoBehaviour {
    [SerializeField] bool person;
    [SerializeField] Vector3 startPos;
    [SerializeField] Vector3 endPos;
	[SerializeField] bool supervisor = false;

    [SerializeField] float speed = 5;
	[SerializeField] string talking;
    
    Vector3 targetPos;
    bool heal;

    void Start() {
        targetPos = endPos;
		setUp();
    }

    void Update() {
		if (person) {
			transform.position = Vector3.MoveTowards(transform.position, targetPos, 5 * Time.deltaTime);

			if (Vector3.Distance(transform.position, targetPos) < 0.01f) {
				targetPos = (targetPos == endPos) ? startPos : endPos;
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<MazeCharacter>()) {
			if (supervisor) {

				return;
			}

            int healing = 0;
			healing = heal ?  1 : -1;
            collision.gameObject.GetComponent<MazeCharacter>().livesChange(healing);
		}
	}

	public void setUp() {
		heal = Random.value < 0.5f;
	}
}