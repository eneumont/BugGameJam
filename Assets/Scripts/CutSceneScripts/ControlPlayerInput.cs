using UnityEngine;
using UnityEngine.InputSystem;

public class ControlPlayerInput : MonoBehaviour
{
    [SerializeField]
    PlayerInput playerInput;

    public void ChangePlayerInput(bool active)
    {
        playerInput.enabled = active;
    }
}
