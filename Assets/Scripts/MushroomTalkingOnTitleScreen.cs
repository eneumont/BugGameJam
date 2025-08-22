using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MushroomTalkingOnTitleScreen : MonoBehaviour
{
    [SerializeField] private AudioClip[] mushroomAudioClips;

    public bool canTalk;

    private float talkCooldown = 10f;
    private float talkDuration = 30f;   // total time allowed to talk
    private float talkTimer;            // countdown

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        talkTimer = talkDuration;
    }

    void Update()
    {
        if (canTalk)
        {
            // countdown for the total talking duration
            if (talkTimer > 0f)
            {
                talkTimer -= Time.deltaTime;

                // cooldown between talks
                talkCooldown -= Time.deltaTime;
                if (talkCooldown <= 0f)
                {
                    int randomIndex = Random.Range(0, mushroomAudioClips.Length);
                    audioSource.PlayOneShot(mushroomAudioClips[randomIndex]);
                    talkCooldown = 10f;
                }
            }
            else
            {
                // time is up, no more new clips
                canTalk = false;
            }
        }
    }

    public void startTalking()
    {
        canTalk = true;
        talkTimer = talkDuration; // reset duration when triggered
        talkCooldown = 0f;        // trigger immediate first line if desired
    }
}
