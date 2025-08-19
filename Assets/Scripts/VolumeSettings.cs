using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;

    private const string VolumeKey = "MasterVolume";

    void Start()
    {
        // Load saved volume
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 0f); // default 0 dB
        audioMixer.SetFloat("MasterVolume", savedVolume);

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", volume);
        PlayerPrefs.SetFloat(VolumeKey, volume);
    }
}
