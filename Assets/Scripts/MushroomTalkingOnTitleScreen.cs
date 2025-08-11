using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MushroomTalkingOnTitleScreen : MonoBehaviour
{
    [SerializeField]
	private AudioClip[] mushroomAudioClips;

    public bool canTalk;

    private float talkCooldown = 10f;

    private AudioSource audioSource;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		audioSource = GetComponent<AudioSource>();
	}

    // Update is called once per frame
    void Update()
    {
        if (canTalk)
        {
            talkCooldown -= Time.deltaTime;
			if (talkCooldown <= 0f)
			{
				int randomIndex = Random.Range(0, mushroomAudioClips.Length);
				audioSource.clip = mushroomAudioClips[randomIndex];
				audioSource.Play();
				talkCooldown = 10f;
			}
		}
    }

	public void startTalking()
	{
		canTalk = true;
	}
}
