using UnityEngine;
using UnityEngine.UI;

public class HardModeSetting : MonoBehaviour
{
    [SerializeField] private Toggle hardModeToggle;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] toggleClips; // assign multiple clips in inspector

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

        // Play a random audio clip
        if(isOn) PlayRandomClip();
    }

    private void PlayRandomClip()
    {
        if (toggleClips == null || toggleClips.Length == 0 || audioSource == null) return;

        int randomIndex = Random.Range(0, toggleClips.Length);
        audioSource.PlayOneShot(toggleClips[randomIndex], 1.5f);
    }

    private void OnDestroy()
    {
        // Remove listener to avoid memory leaks
        if (hardModeToggle != null) hardModeToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}
