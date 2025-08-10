using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MazeCharacter : MonoBehaviour {
    [SerializeField] float speed = 5f;
    [SerializeField] SpriteRenderer[] heartImgs;
    
    Rigidbody2D rb;
    Vector2 input;
    Control c;

    int Lives = 0;

    public enum Control {
        Up, Down, Left, Right
    }

    void Start() {
        c = (Control)Random.Range(0, 4);
        rb = GetComponent<Rigidbody2D>();

        livesChange(3);
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
                heartImgs[i - 1].color = Color.red;
            } else {
                heartImgs[i - 1].color = Color.black;
            }
        }

        if (newlives <= 0) {
            StartCoroutine(Death());
        }
    }

    IEnumerator Death() {
        
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("ReverseMazeScene");
    }
}