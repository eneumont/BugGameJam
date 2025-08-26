using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ExitGameHandler : MonoBehaviour
{
    [Header("UI Prefab")]
    [SerializeField] private GameObject confirmUIPrefab; // prefab with its own Canvas

    private GameObject currentPopup;
    private bool awaitingConfirm = false;
    private InputAction escAction;
    private Coroutine resetCoroutine;

    private void OnEnable()
    {
        escAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/escape");
        escAction.performed += HandleEscape;
        escAction.Enable();
    }

    private void OnDisable()
    {
        escAction.performed -= HandleEscape;
        escAction.Disable();
    }

    private void HandleEscape(InputAction.CallbackContext ctx)
    {
        if (!awaitingConfirm)
        {
            // First press -> spawn popup
            currentPopup = Instantiate(confirmUIPrefab);
            awaitingConfirm = true;

            // start/reset auto-cancel timer
            if (resetCoroutine != null) StopCoroutine(resetCoroutine);
            resetCoroutine = StartCoroutine(AutoCancel());
        }
        else
        {
            // Second press -> quit game
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    private IEnumerator AutoCancel()
    {
        yield return new WaitForSeconds(1.5f);
        CancelExit();
    }

    public void CancelExit()
    {
        if (resetCoroutine != null) StopCoroutine(resetCoroutine);
        if (currentPopup != null) Destroy(currentPopup);

        awaitingConfirm = false;
        resetCoroutine = null;
    }
}
