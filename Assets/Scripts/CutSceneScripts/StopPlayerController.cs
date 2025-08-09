using UnityEngine;
using UnityEngine.InputSystem;

public class StopPlayerController : MonoBehaviour
{
    [SerializeField]
    PlayerInput PlayerInput;

	[SerializeField]
	Rigidbody2D PlayerBody;

	void Awake()
    {
        if (PlayerInput != null)
        {
            PlayerInput.enabled = false;
        }

        if (PlayerBody != null)
        {
            PlayerBody.bodyType = RigidbodyType2D.Kinematic;
        }
    }
}
