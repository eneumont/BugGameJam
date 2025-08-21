using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestManager : MonoBehaviour
{
    private int lives = 2;

    [SerializeField]
    Image heart1;
	[SerializeField]
	Image heart2;
    [SerializeField]
    Sprite EmptyHeart;

	[SerializeField]
	GameObject loseText;
	[SerializeField]
	SpriteRenderer background;

	[SerializeField]
	GameObject[] TurnItOff;

	public void LoseLife()
	{
		if (lives > 0)
		{
			lives--;
			UpdateHearts();
		}
	}

	private void UpdateHearts()
	{
		switch (lives)
		{
			case 1:
				heart1.sprite = EmptyHeart;
				break;
			case 0:
				heart1.sprite = EmptyHeart;
				heart2.sprite = EmptyHeart;
				StartCoroutine(Lose());
				break;
			default:
				Debug.LogError("Invalid number of lives: " + lives);
				break;
		}
	}

	private IEnumerator Lose()
	{
		loseText.SetActive(true);
		background.sprite = null;
		background.color = new Color(1f, 0f, 0f, 1f);

		foreach (GameObject go in TurnItOff)
		{
			go.SetActive(false);
		}

		yield return new WaitForSeconds(5f);
		SceneManager.LoadScene("Lose");

		//int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
		//int loseSceneIndex = currentSceneIndex - 3;
		//// Check if the next scene index is within bounds
		//if (loseSceneIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
		//{
		//	UnityEngine.SceneManagement.SceneManager.LoadScene(loseSceneIndex);
		//}
	}
}
