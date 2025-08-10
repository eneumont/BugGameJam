using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class StartAnim : MonoBehaviour
{

	[SerializeField]
	private bool PlayerCanMove;

	private PlayableDirector anim;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		anim = GetComponent<PlayableDirector>();

		anim.played += AnimStarted;
	}

	private void AnimStarted(PlayableDirector director)
	{
		//playerInput.ChangePlayerInput(PlayerCanMove);
	}
}
