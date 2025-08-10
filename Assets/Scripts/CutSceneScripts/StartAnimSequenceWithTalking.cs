using UnityEngine;

[RequireComponent(typeof(TalkingController))]
public class StartAnimSequenceWithTalking : MonoBehaviour
{
    [SerializeField]
    private ControlPlayerInput playerInput;

	[SerializeField]
	private bool PlayerCanMove;

    private TalkingController talk;

	private void Start()
	{
		talk = GetComponent<TalkingController>();
	}

	private void Update()
	{
		if (talk.started)
		{
			//playerInput.ChangePlayerInput(PlayerCanMove);
			this.enabled = false;
		}
	}
}
