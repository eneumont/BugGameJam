using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerFileCatcher : MonoBehaviour
{
    [SerializeField] private AudioClip dingSound;
    [SerializeField] private AudioSource audioSource; // Optional, assign in Inspector
    [SerializeField] private Character2DMovement playerMovement;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private WarningUIManager warningUI; // Drag your LifeSystem object in the Inspector


    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        FileData fileData = other.GetComponent<FileData>();
        if (fileData == null)
            return;

        if (fileData.isGoodFile)
        {
            Debug.Log("✅ Player caught a good file!");
            if (dingSound != null)
            {
                audioSource.PlayOneShot(dingSound);
            }
            Destroy(other.gameObject);
            warningUI.AddCollected();
        }
        else
        {
            OnCatchBadFile();
            Destroy(other.gameObject);
        }
    }

    private void OnCatchBadFile()
    {
        // Increase speed every time
        playerMovement.IncreaseSpeed(5);

        float roll = Random.value; // 0 to 1 float

        if (roll < 0.1f) // 10% chance - invisible
        {
            StartCoroutine(BecomeInvisibleTemporarily());
        }
        else if (roll < 0.1f + 0.2f) // next 20% chance - reverse controls
        {
            playerMovement.SetReverseControls(true);
            StartCoroutine(ResetControlsAfterDelay(5f));
        }
        else if (roll < 0.1f + 0.2f + 0.1f) // next 10% chance - pause game
        {
            float pauseDuration = Random.Range(2f, 10f);
            StartCoroutine(PauseGameTemporarily(pauseDuration));
        }
        else
        {
            // No extra effect
            Debug.Log("No extra effect this time.");
        }
    }

    // Coroutine to make player invisible temporarily
    private IEnumerator BecomeInvisibleTemporarily()
    {
        if (sr != null)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(5f); // invisible for 5 seconds
            sr.enabled = true;
        }
    }

    // Coroutine to pause game for a duration
    private IEnumerator PauseGameTemporarily(float duration)
    {
        Time.timeScale = 0f; // pause game
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f; // resume game
    }


    private IEnumerator ResetControlsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        playerMovement.SetReverseControls(false);
    }
}
