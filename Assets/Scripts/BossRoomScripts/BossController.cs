using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace BossRoom
{
    public class BossController : MonoBehaviour
    {
        [Header("Boss Settings")]
        public float maxHealth = 100f;
        public float phase1Duration = 180f; // 3 minutes
        public float phase2Duration = 300f; // 5 minutes
        public Transform debugConsole;
        public Animator bossAnimator;
        public BossUIManager bossUIManager;

        [Header("Bug Systems")]
        public BugManager bugManager;
        public UIBetrayalSystem uiBetrayalSystem;
        public CollisionParadoxSystem collisionSystem;
        public InputDesyncSystem inputSystem;
        public GoalGaslightingSystem gaslightingSystem;

        [Header("Audio")]
        public AudioSource bossAudioSource;
        public AudioClip[] typingClips;
        public AudioClip updateSound;
        public AudioClip errorSound;

        private float currentHealth;
        private bool isPhase2 = false;
        private bool isFightActive = false;
        private bool isDefeated = false;
        private float fightTimer = 0f;
        private Coroutine currentAttackCoroutine;
        private bool isMoving = false;
        private bool isJumping = false;


        [Header("Player Settings")]
        public GameObject player;
        public BossRoomPlayerController playerController; // optional, for convenience

        // Attack patterns
        private float lastAttackTime;
        private float attackInterval = 3f;

        public enum BossPhase { Phase1, Phase2, Defeated }
        public BossPhase currentPhase = BossPhase.Phase1;

        void Start()
        {
            currentHealth = maxHealth;
            InitializeBugSystems();
        }

        void Update()
        {
            if (!isFightActive || isDefeated) return;

            fightTimer += Time.deltaTime;

            bossUIManager?.UpdateHealthBar(currentHealth / maxHealth);
            UpdateBossLogic();
            CheckPhaseTransition();
            HandleAttackPatterns();
        }

        void UpdateMovementAnimation(bool moving)
        {
            if (bossAnimator != null)
                bossAnimator.SetBool("isMoving", moving);
        }

        void UpdateJumpAnimation(bool jumping)
        {
            if (bossAnimator != null)
                bossAnimator.SetBool("isJumping", jumping);
        }

        void TriggerRangedAttackAnimation()
        {
            if (bossAnimator != null)
                bossAnimator.SetTrigger("attackRanged");
        }


        void InitializeBugSystems()
        {
            if (bugManager == null) bugManager = GetComponent<BugManager>();
            if (uiBetrayalSystem == null) uiBetrayalSystem = FindObjectOfType<UIBetrayalSystem>();
            if (collisionSystem == null) collisionSystem = GetComponent<CollisionParadoxSystem>();
            if (inputSystem == null) inputSystem = FindObjectOfType<InputDesyncSystem>();
            if (gaslightingSystem == null) gaslightingSystem = GetComponent<GoalGaslightingSystem>();
            if (bossUIManager == null) bossUIManager = FindObjectOfType<BossUIManager>();

            if (player != null)
            {
                if (collisionSystem != null)
                    collisionSystem.SetPlayer(player.transform);

                if (inputSystem != null)
                    inputSystem.SetPlayer(playerController != null ? playerController : player.GetComponent<BossRoomPlayerController>());

                if (gaslightingSystem != null)
                    gaslightingSystem.SetPlayer(player);

                // Add more if needed
            }
        }


        public void StartBossFight()
        {
            isFightActive = true;
            fightTimer = 0f;

            // Start with mild bugs
            bugManager.SetBugIntensity(BugManager.BugIntensity.Mild);

            // Play boss entrance
            if (bossAnimator != null)
                bossAnimator.SetTrigger("StartFight");

            StartCoroutine(PlayTypingSounds());
        }

        void UpdateBossLogic()
        {
            // Simulate movement state (replace with real AI/movement later)
            bool moving = Random.value < 0.5f; // 50% chance to move
            bool jumping = Random.value < 0.1f; // 10% chance to jump

            isMoving = moving;
            isJumping = jumping;

            UpdateMovementAnimation(isMoving);
            UpdateJumpAnimation(isJumping);

            // Boss "typing" at console behavior
            if (debugConsole != null && Random.Range(0f, 1f) < 0.1f * Time.deltaTime)
            {
                debugConsole.Rotate(0, 0, Random.Range(-1f, 1f));
            }
        }


        void CheckPhaseTransition()
        {
            if (!isPhase2 && (fightTimer >= phase1Duration || currentHealth <= maxHealth * 0.6f))
            {
                StartPhase2();
            }
        }

        void StartPhase2()
        {
            isPhase2 = true;
            currentPhase = BossPhase.Phase2;

            StartCoroutine(Phase2Transition());
        }

        IEnumerator Phase2Transition()
        {
            // Show update notification
            uiBetrayalSystem.ShowFakeDialog("Installing Workplace Optimization Update...", 2f);

            if (updateSound != null)
                bossAudioSource.PlayOneShot(updateSound);

            yield return new WaitForSeconds(1f);

            // Increase bug intensity
            bugManager.SetBugIntensity(BugManager.BugIntensity.Aggressive);

            // Start UI betrayal
            uiBetrayalSystem.StartUIBetrayal();

            // Reduce attack interval (more frequent attacks)
            attackInterval = 2f;

            uiBetrayalSystem.ShowFakeDialog("Update Complete! Optimizations Applied.", 1.5f);
        }

        void HandleAttackPatterns()
        {
            if (Time.time - lastAttackTime >= attackInterval)
            {
                ExecuteRandomAttack();
                lastAttackTime = Time.time;

                // Vary attack intervals with some randomness
                attackInterval = Random.Range(2f, 4f) * (isPhase2 ? 0.7f : 1f);
            }
        }

        void ExecuteRandomAttack()
        {
            if (currentAttackCoroutine != null)
                StopCoroutine(currentAttackCoroutine);

            int attackType = Random.Range(0, isPhase2 ? 6 : 3);

            switch (attackType)
            {
                case 0:
                    currentAttackCoroutine = StartCoroutine(CollisionParadoxAttack());
                    break;
                case 1:
                    currentAttackCoroutine = StartCoroutine(InputDesyncAttack());
                    break;
                case 2:
                    TriggerRangedAttackAnimation();   // <---- Add this
                    currentAttackCoroutine = StartCoroutine(GoalGaslightAttack());
                    break;
                case 3: // Phase 2 only
                    currentAttackCoroutine = StartCoroutine(UIShuffleAttack());
                    break;
                case 4: // Phase 2 only
                    currentAttackCoroutine = StartCoroutine(FakePopupAttack());
                    break;
                case 5: // Phase 2 only
                    currentAttackCoroutine = StartCoroutine(ControlSchemeSwapAttack());
                    break;
            }
        }


        IEnumerator CollisionParadoxAttack()
        {
            collisionSystem.TriggerCollisionParadox();
            yield return new WaitForSeconds(5f);
            collisionSystem.ResetCollision();
        }

        IEnumerator InputDesyncAttack()
        {
            inputSystem.TriggerInputDesync(Random.Range(0.5f, 2f));
            yield return new WaitForSeconds(3f);
        }

        IEnumerator GoalGaslightAttack()
        {
            gaslightingSystem.TriggerGaslighting();
            yield return new WaitForSeconds(4f);
        }

        IEnumerator UIShuffleAttack()
        {
            uiBetrayalSystem.ShuffleUIElements();
            yield return new WaitForSeconds(8f);
            uiBetrayalSystem.ResetUIPositions();
        }

        IEnumerator FakePopupAttack()
        {
            uiBetrayalSystem.SpawnFakePopup();
            yield return new WaitForSeconds(6f);
        }

        IEnumerator ControlSchemeSwapAttack()
        {
            inputSystem.SwapControlScheme();
            uiBetrayalSystem.ShowFakeDialog("Controls Updated to Optimize Workflow", 1f);
            yield return new WaitForSeconds(Random.Range(5f, 10f));
            inputSystem.RestoreControlScheme();
        }

        public void TakeDamage(float damage)
        {
            if (isDefeated) return;

            // Sometimes gaslighting prevents actual damage
            if (gaslightingSystem.ShouldPreventDamage())
            {
                gaslightingSystem.ShowFakeDamage();
                return;
            }

            currentHealth -= damage;

            // Sometimes add fake auto-heal
            if (Random.Range(0f, 1f) < 0.3f && currentHealth > 0)
            {
                StartCoroutine(FakeAutoHeal());
            }

            if (currentHealth <= 0)
            {
                DefeatBoss();
            }
        }

        IEnumerator FakeAutoHeal()
        {
            yield return new WaitForSeconds(Random.Range(1f, 3f));

            float healAmount = Random.Range(5f, 15f);
            currentHealth += healAmount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            uiBetrayalSystem.ShowFakeDialog($"Auto-Heal Patch Applied (+{healAmount:F0} HP)", 2f);
        }

        void DefeatBoss()
        {
            if (isDefeated) return;

            isDefeated = true;
            currentPhase = BossPhase.Defeated;

            StartCoroutine(DefeatSequence());
        }

        IEnumerator DefeatSequence()
        {
            // Boss tries to spawn final update
            uiBetrayalSystem.ShowFakeDialog("Initiating Emergency Compliance Protocol...", 2f);
            yield return new WaitForSeconds(2f);

            uiBetrayalSystem.ShowFakeDialog("Loading...", 1f);
            yield return new WaitForSeconds(1f);

            // Get stuck in infinite loading
            uiBetrayalSystem.ShowInfiniteLoading();

            // Pause all bug systems
            bugManager.PauseAllBugs();

            // Enable door exit
            FindObjectOfType<ExitDoorController>()?.EnableExit();

            yield return new WaitForSeconds(2f);

            // Final victory message
            uiBetrayalSystem.ShowFakeDialog("System.exe has stopped responding.", 5f);
        }

        IEnumerator PlayTypingSounds()
        {
            while (isFightActive && !isDefeated)
            {
                if (typingClips.Length > 0 && Random.Range(0f, 1f) < 0.3f)
                {
                    AudioClip clip = typingClips[Random.Range(0, typingClips.Length)];
                    bossAudioSource.PlayOneShot(clip, 0.3f);
                }

                yield return new WaitForSeconds(Random.Range(0.5f, 2f));
            }
        }

        public float GetHealthPercentage()
        {
            return currentHealth / maxHealth;
        }

        public bool IsFightActive()
        {
            return isFightActive && !isDefeated;
        }
    }
}