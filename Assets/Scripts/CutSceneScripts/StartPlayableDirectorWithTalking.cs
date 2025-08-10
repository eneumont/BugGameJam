using UnityEngine;
using UnityEngine.Playables;

[RequireComponent (typeof(PlayableDirector))]
[RequireComponent (typeof(TalkingController))]
public class StartPlayableDirectorWithTalking : MonoBehaviour
{
    private PlayableDirector anim;
    private TalkingController talk;

	private void Start()
	{
		anim = GetComponent<PlayableDirector>();
        talk = GetComponent<TalkingController>();
	}

	void Update()
    {
        if (talk.started)
        {
            anim.Play();
        }
    }
}
