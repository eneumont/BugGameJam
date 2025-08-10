using UnityEngine;

[RequireComponent(typeof(TalkingController))]
public class GoToNextTalking : MonoBehaviour
{
    [SerializeField]
    private StartNewTalking nextTalking;

	private TalkingController talk;

	private void Awake()
	{
		talk = GetComponent<TalkingController>();
	}

	// Update is called once per frame
	void Update()
    {
		if (talk.done && nextTalking)
		{
			nextTalking.startNewTalking();
			this.gameObject.SetActive(false);
		}
    }
}
