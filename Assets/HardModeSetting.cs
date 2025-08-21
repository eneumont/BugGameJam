using UnityEngine;
using UnityEngine.UI;

public class HardModeSetting : MonoBehaviour
{
    [SerializeField] private Toggle hardModeToggle;

    private void Start()
    {
        if (hardModeToggle == null)
        {
            Debug.LogError("HardMode Toggle is not assigned in the inspector!");
            return;
        }

        // Initialize the toggle based on the current GameProgress.HardMode value
        hardModeToggle.isOn = GameProgress.HardMode;

        // Add listener to handle changes
        hardModeToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        // Set the hard mode value
        GameProgress.HardMode = isOn;
    }

    private void OnDestroy()
    {
        // Remove listener to avoid memory leaks
        if (hardModeToggle != null)
            hardModeToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}
