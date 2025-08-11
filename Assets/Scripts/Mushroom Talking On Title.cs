using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MushroomTalkingOnTitle : MonoBehaviour
{
	private AudioSource audioSource;

    [SerializeField]
	public AudioClip[] audioClips;

    private float NextAudioTime = 10f;

	public bool CanTalk = false;

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
	}

	// Update is called once per frame
	void Update()
    {
		if (!CanTalk)
		{
			return; // Exit if CanTalk is false
		}

		NextAudioTime -= Time.deltaTime;

        if(NextAudioTime < 0f)
        {
			if (audioClips.Length > 0)
			{
				int randomIndex = Random.Range(0, audioClips.Length);
				audioSource = GetComponent<AudioSource>();
				audioSource.clip = audioClips[randomIndex];
				audioSource.Play();
				NextAudioTime = audioSource.clip.length + Random.Range(5f, 20f); // Add a random delay after playing the clip
			}
		}
	}

	public void StartTalking()
	{
		CanTalk = true;
		NextAudioTime = 10f; // Reset the timer when starting to talk
	}
}
