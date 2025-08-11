using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace BossRoom
{
    public class ExitDoorController : MonoBehaviour
    {
        [Header("Door Settings")]
        public GameObject doorObject;
        public Collider2D doorCollider;
        public SpriteRenderer doorRenderer;

        [Header("Visual Effects")]
        public Color lockedColor = Color.red;
        public Color unlockedColor = Color.green;
        public ParticleSystem unlockParticles;

        [Header("Audio")]
        public AudioSource doorAudioSource;
        public AudioClip doorUnlockSound;
        public AudioClip doorOpenSound;

        [Header("UI")]
        public GameObject interactionPrompt;
        public Text promptText;

        [Header("Scene Transition")]
        public string nextSceneName = "NextLevel";
        public float transitionDelay = 1f;

        private bool isDoorLocked = true;
        private bool isPlayerNearby = false;
        private bool isTransitioning = false;
        private Color originalDoorColor;

        // Reference to boss controller
        private BossController bossController;

        void Start()
        {
            InitializeDoor();
            FindBossController();
        }

        void InitializeDoor()
        {
            if (doorRenderer != null)
            {
                originalDoorColor = doorRenderer.color;
                doorRenderer.color = lockedColor;
            }

            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            if (doorCollider != null)
                doorCollider.isTrigger = true;

            // Ensure door starts locked
            SetDoorLocked(true);
        }

        void FindBossController()
        {
            bossController = FindObjectOfType<BossController>();
        }

        void Update()
        {
            UpdateDoorState();
            HandlePlayerInteraction();
        }

        void UpdateDoorState()
        {
            // Check if boss fight is complete
            if (isDoorLocked && bossController != null)
            {
                if (bossController.currentPhase == BossController.BossPhase.Defeated)
                {
                    // Door should unlock when boss enters defeat sequence
                    // But we'll wait for explicit EnableExit() call for timing
                }
            }
        }

        void HandlePlayerInteraction()
        {
            if (!isPlayerNearby || isDoorLocked || isTransitioning)
                return;

            // Check for interaction input (E key or similar)
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
            {
                StartCoroutine(ExitLevel());
            }
        }

        public void EnableExit()
        {
            StartCoroutine(UnlockDoorSequence());
        }

        IEnumerator UnlockDoorSequence()
        {
            // Brief delay for dramatic effect
            yield return new WaitForSeconds(0.5f);

            // Play unlock sound
            if (doorAudioSource != null && doorUnlockSound != null)
                doorAudioSource.PlayOneShot(doorUnlockSound);

            // Visual feedback
            if (doorRenderer != null)
            {
                // Flash effect
                float flashDuration = 0.1f;
                int flashCount = 3;

                for (int i = 0; i < flashCount; i++)
                {
                    doorRenderer.color = Color.white;
                    yield return new WaitForSeconds(flashDuration);
                    doorRenderer.color = lockedColor;
                    yield return new WaitForSeconds(flashDuration);
                }

                // Transition to unlocked color
                float transitionDuration = 1f;
                float elapsed = 0f;

                while (elapsed < transitionDuration)
                {
                    float t = elapsed / transitionDuration;
                    doorRenderer.color = Color.Lerp(lockedColor, unlockedColor, t);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                doorRenderer.color = unlockedColor;
            }

            // Play particles
            if (unlockParticles != null)
                unlockParticles.Play();

            // Actually unlock the door
            SetDoorLocked(false);

            // Show interaction prompt if player is nearby
            if (isPlayerNearby)
                ShowInteractionPrompt(true);
        }

        void SetDoorLocked(bool locked)
        {
            isDoorLocked = locked;

            if (doorRenderer != null)
            {
                doorRenderer.color = locked ? lockedColor : unlockedColor;
            }

            // Update interaction prompt
            if (!locked && isPlayerNearby)
                ShowInteractionPrompt(true);
            else
                ShowInteractionPrompt(false);
        }

        void ShowInteractionPrompt(bool show)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(show);

                if (show && promptText != null)
                {
                    if (isDoorLocked)
                        promptText.text = "Door is locked";
                    else
                        promptText.text = "Press E to exit";
                }
            }
        }

        IEnumerator ExitLevel()
        {
            if (isTransitioning) yield break;

            isTransitioning = true;

            // Play door open sound
            if (doorAudioSource != null && doorOpenSound != null)
                doorAudioSource.PlayOneShot(doorOpenSound);

            // Hide interaction prompt
            ShowInteractionPrompt(false);

            // Optional: Fade out or other transition effects
            yield return new WaitForSeconds(transitionDelay);

            // Load next scene
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                // Fallback: just reload current scene or quit
                Debug.Log("Boss fight completed! No next scene specified.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNearby = true;

                if (!isDoorLocked)
                    ShowInteractionPrompt(true);
                else
                    ShowInteractionPrompt(true); // Show "Door is locked" message
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNearby = false;
                ShowInteractionPrompt(false);
            }
        }

        // Alternative method for immediate exit (skip animations)
        public void ForceExit()
        {
            if (!isTransitioning)
                StartCoroutine(ExitLevel());
        }

        // Method to check door state
        public bool IsDoorLocked()
        {
            return isDoorLocked;
        }

        public bool IsPlayerNearby()
        {
            return isPlayerNearby;
        }

        // Method for debugging
        [System.Serializable]
        public class DoorDebugInfo
        {
            public bool doorLocked;
            public bool playerNearby;
            public bool transitioning;
            public string bossPhase;
        }

        public DoorDebugInfo GetDebugInfo()
        {
            return new DoorDebugInfo
            {
                doorLocked = isDoorLocked,
                playerNearby = isPlayerNearby,
                transitioning = isTransitioning,
                bossPhase = bossController?.currentPhase.ToString() ?? "No Boss Found"
            };
        }
    }
}