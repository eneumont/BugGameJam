using UnityEngine;

[RequireComponent(typeof(TalkingController))]
public class EndAnimSequenceWithTalking : MonoBehaviour
{
    [SerializeField]
    ControlPlayerInput playerInput;

    [SerializeField]
    bool PlayerCanMove;

    private TalkingController talk;

	private void Start()
	{
		talk = GetComponent<TalkingController>();
	}

	void Update()
    {
        if (talk.done)
        {
            playerInput.ChangePlayerInput(PlayerCanMove);
            this.enabled = false;
            //gameObject.SetActive(false);
        }
    }
}
