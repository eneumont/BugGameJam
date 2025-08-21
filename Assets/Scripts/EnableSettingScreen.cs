using UnityEngine;

public class EnableSettingsScreen : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;

    // Call this from a UI Button's OnClick
    public void EnableCanvas()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(true);
            Debug.Log($"[CanvasToggle] {targetObject.name} is now ENABLED.");
        }
    }

    // Call this from another button or script
    public void DisableCanvas()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false);
            Debug.Log($"[CanvasToggle] {targetObject.name} is now DISABLED.");
        }
    }

    // Optional toggle method
    public void ToggleCanvas()
    {
        if (targetObject != null)
        {
            bool newState = !targetObject.activeSelf;
            targetObject.SetActive(newState);
            Debug.Log($"[CanvasToggle] {targetObject.name} is now {(newState ? "ENABLED" : "DISABLED")}.");
        }
    }
}
