using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MazeCharacter : MonoBehaviour {
    [SerializeField] float speed = 5f;
    [SerializeField] SpriteRenderer[] heartImgs;
    
    Vector3 spawnPos;
    Rigidbody2D rb;
    Vector2 input;
    Control c;
    public bool done = false;
    int Lives = 0;

    public enum Control {
        Up, Down, Left, Right
    }

    void Start() {
        newControls();
        rb = GetComponent<Rigidbody2D>();
        spawnPos = transform.position;

        livesChange(3);
    }

    public void newControls() {
		c = (Control)Random.Range(0, 4);
	}

    void Update() {
        switch (c) {
            case Control.Left:
				input.x = Input.GetAxisRaw("Horizontal") * -1;
				input.y = Input.GetAxisRaw("Vertical") * -1;
				break;
            case Control.Right:
				input.x = Input.GetAxisRaw("Vertical") * -1;
				input.y = Input.GetAxisRaw("Horizontal") * -1;
				break;
            case Control.Up:
				input.x = Input.GetAxisRaw("Horizontal");
				input.y = Input.GetAxisRaw("Vertical");
				break;
            case Control.Down:
				input.x = Input.GetAxisRaw("Vertical");
				input.y = Input.GetAxisRaw("Horizontal");
				break;
        }

        input.Normalize();
    }

	private void FixedUpdate() {
		rb.linearVelocity = input * speed;
	}

    public void livesChange(int newlives) {
        Lives += newlives;

        for (int i = heartImgs.Length; i > 0; i--) {
            if (Lives >= i) {
                heartImgs[i - 1].color = Color.white;
                if (Lives > 3) Lives = 3; 
            } else {
                heartImgs[i - 1].color = Color.black;
            }
        }

        if (Lives <= 0) {
            StartCoroutine(Death());
        }
    }

    public void readyToExit() {
        done = true;
    }

    public void checkPoint(Vector3 newPos) {
        spawnPos = newPos;
    }

    IEnumerator Death() {
        yield return new WaitForSeconds(3f);
        livesChange(3);
        transform.position = spawnPos;
        //SceneManager.LoadScene("ReverseMazeScene");
    }
}