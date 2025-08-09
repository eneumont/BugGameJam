using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class EndAnim : MonoBehaviour
{
	[SerializeField]
	private Rigidbody2D PlayerBody;

	[SerializeField]
	private ControlPlayerInput PlayerInputs;

	[SerializeField]
	private bool PlayerCanMove;

	private PlayableDirector anim;

	public bool started = false;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		anim = GetComponent<PlayableDirector>();
		anim.stopped += OnPlayableDirectorHasStopped;
	}

	void OnPlayableDirectorHasStopped(PlayableDirector director)
	{
		started = true;
		PlayerBody.linearVelocity = Vector2.zero;
		PlayerBody.bodyType = RigidbodyType2D.Dynamic;
		PlayerInputs.ChangePlayerInput(PlayerCanMove);
		anim.Stop();
		gameObject.SetActive(false);
	}
}
