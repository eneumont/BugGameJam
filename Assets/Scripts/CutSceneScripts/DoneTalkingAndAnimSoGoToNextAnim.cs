using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(TalkingController))]
[RequireComponent(typeof(PlayableDirector))]
public class DoneTalkingAndAnimSoGoToNextAnim : MonoBehaviour
{
	[SerializeField]
	PlayableDirector newAnim;

	private TalkingController talk;
	private PlayableDirector oldAnim;

	private float oldAnimTime;
	private bool finsishAnim = false;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		talk = GetComponent<TalkingController>();
		oldAnim = GetComponent<PlayableDirector>();
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (talk.done && !finsishAnim)
		{
			oldAnimTime = (float) oldAnim.time;
			finsishAnim = true;
		}

		if (finsishAnim)
		{
			oldAnimTime += Time.deltaTime;
			oldAnim.time = oldAnimTime;

			if (oldAnimTime >= oldAnim.duration)
			{
				oldAnim.Stop();
				newAnim.gameObject.SetActive(true);
				newAnim.Play();
				gameObject.SetActive(false);
				finsishAnim = false;
				oldAnimTime = 0;
			}
		}
	}
}
