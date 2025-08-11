using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TalkingController))]
public class Glitchend : MonoBehaviour
{
	private TalkingController talk;

    [SerializeField]
    SpriteRenderer backgound;
    [SerializeField]
    GameObject sadFace;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		talk = GetComponent<TalkingController>();
	}

    // Update is called once per frame
    void Update()
    {
        if (talk.done)
        {
            backgound.color = new Color(0.08f, 0.05f, .97f, 1f);
            backgound.sprite = null;
			sadFace.SetActive(true);
			StartCoroutine(end());
		}
    }

    private IEnumerator end()
    {
		yield return new WaitForSeconds(3f);

		// Load the next scene in the build settings
		int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
		int nextSceneIndex = currentSceneIndex + 1;
		// Check if the next scene index is within bounds
		if (nextSceneIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
		}
		else
		{
			Debug.LogWarning("No more scenes to load.");
		}
	}
}
