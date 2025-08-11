using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MazeObstacle : MonoBehaviour {
    [SerializeField] bool person;
    [SerializeField] Vector3 startPos;
    [SerializeField] Vector3 endPos;
	[SerializeField] bool supervisor = false;
	[SerializeField] bool boss = false;
	[SerializeField] TextMeshProUGUI talkText;

    [SerializeField] float speed = 5;
	[SerializeField] float talkTime = 1;
	[SerializeField] int talkInt = 0;
	[SerializeField] string talking;
    
    Vector3 targetPos;
    bool heal;
	AudioSource audioSource;

    void Start() {
        targetPos = endPos;
		setUp();
		if (!person) audioSource = GetComponent<AudioSource>();
		talkText.transform.parent.gameObject.SetActive(false);
    }

    void Update() {
		if (person && !supervisor) {
			transform.position = Vector3.MoveTowards(transform.position, targetPos, 5 * Time.deltaTime);

			if (Vector3.Distance(transform.position, targetPos) < 0.01f) {
				targetPos = (targetPos == endPos) ? startPos : endPos;
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<MazeCharacter>()) {
			if (!person) {
				audioSource.Play();
				talkTime = audioSource.clip.length;
				FindFirstObjectByType<HintSystem>().justTalk(talkInt, talkTime);
			}

			talkText.text = talking;
			talkText.transform.parent.gameObject.SetActive(true);
			StartCoroutine(interaction(talkTime));

			if (boss) {
				return;
			}

			if (supervisor) {
				collision.gameObject.GetComponent<MazeCharacter>().readyToExit();
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

	IEnumerator interaction(float time) {
		yield return new WaitForSeconds(time);
		talkText.transform.parent.gameObject.SetActive(false);
		if (!person) Destroy(gameObject);
	}
}