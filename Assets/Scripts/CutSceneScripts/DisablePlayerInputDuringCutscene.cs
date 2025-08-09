using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class DisablePlayerInputDuringCutscene : MonoBehaviour
{
	[SerializeField]
	PlayerInput playerInput;

	private PlayableDirector Anim;

	private void Start()
	{
		Anim = GetComponent<PlayableDirector>();

		Anim.played += StartofAnim;
		Anim.stopped += EndofAnim;
	}

	private void StartofAnim(PlayableDirector director)
	{
		playerInput.enabled = false;
	}

	private void EndofAnim(PlayableDirector director)
	{
		playerInput.enabled = true;
	}
}
