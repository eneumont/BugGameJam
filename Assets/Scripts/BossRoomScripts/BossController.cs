using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace BossRoom
{
    public class BossController : MonoBehaviour
    {
        [Header("Boss Stats")]
        public float maxHealth = 100f;
        public float attackDamage = 1f;
        public float moveSpeed = 3f;

        [Header("Attack Patterns")]
        public Transform[] attackPositions;
        public GameObject projectilePrefab;
        public float minAttackInterval = 2f;
        public float maxAttackInterval = 5f;

        [Header("Phase Timing")]
        public float phase1Duration = 180f; // 3 minutes
        public float totalFightDuration = 480f; // 8 minutes max

        [Header("References")]
        public Transform debugConsole;
        public Animator bossAnimator;
        public BossUIManager bossUIManager;
        public AudioSource bossAudioSource;
        public AudioClip[] typingClips;
        public AudioClip updateSound;
        public AudioClip errorSound;

        [Header("Bug Systems")]
        public BugManager bugManager;
        public UIBetrayalSystem uiBetrayalSystem;
        public CollisionParadoxSystem collisionSystem;
        public InputDesyncSystem inputSystem;
        public GoalGaslightingSystem gaslightingSystem;

        private float currentHealth;
        private bool isPhase2 = false;
        private bool isFightActive = false;
        private bool isDefeated = false;
        private float fightTimer = 0f;
        private float nextAttackTime;
        private Transform player;
        private Vector2 targetPosition;
        private bool isMovingToPosition = false;

        public enum BossPhase { Phase1, Phase2, Defeated }
        public BossPhase currentPhase = BossPhase.Phase1;

        void Start()
        {
            currentHealth = maxHealth;
            InitializeBugSystems();
            FindPlayer();
            SetNextAttackTime();

            // Set initial target position
            targetPosition = transform.position;
        }

        void Update()
        {
            if (!isFightActive || isDefeated) return;

            fightTimer += Time.deltaTime;

            // Update UI
            bossUIManager?.UpdateHealthBar(currentHealth / maxHealth);

            // Boss AI
            HandleMovement();
            HandleAttacking();
            UpdateBossAnimations();
            CheckPhaseTransition();

            // Console typing animation
            AnimateConsole();

            // Auto-defeat after max time (emergency win condition)
            if (fightTimer >= totalFightDuration)
            {
                currentHealth = 0;
                DefeatBoss();
            }
        }

        void InitializeBugSystems()
        {
            // Find systems if not assigned
            if (bugManager == null) bugManager = GetComponent<BugManager>();
            if (uiBetrayalSystem == null) uiBetrayalSystem = FindObjectOfType<UIBetrayalSystem>();
            if (collisionSystem == null) collisionSystem = GetComponent<CollisionParadoxSystem>();
            if (inputSystem == null) inputSystem = FindObjectOfType<InputDesyncSystem>();
            if (gaslightingSystem == null) gaslightingSystem = GetComponent<GoalGaslightingSystem>();
            if (bossUIManager == null) bossUIManager = FindObjectOfType<BossUIManager>();
        }

        void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                player = playerObj.transform;

                // Set up systems with player reference
                if (collisionSystem != null)
                    collisionSystem.SetPlayer(player);

                if (inputSystem != null)
                {
                    var playerController = playerObj.GetComponent<BossRoomPlayerController>();
                    if (playerController != null)
                        inputSystem.SetPlayer(playerController);
                }

                if (gaslightingSystem != null)
                    gaslightingSystem.SetPlayer(playerObj);
            }
        }

        public void StartBossFight()
        {
            isFightActive = true;
            fightTimer = 0f;

            // Start with mild bugs
            if (bugManager != null)
                bugManager.SetBugIntensity(BugManager.BugIntensity.Mild);

            // Boss entrance animation
            if (bossAnimator != null)
                bossAnimator.SetTrigger("StartFight");

            StartCoroutine(PlayTypingSounds());
        }

        void HandleMovement()
        {
            if (player == null) return;

            // Simple AI: Move to strategic positions
            if (!isMovingToPosition)
            {
                // Pick a new position occasionally
                if (Random.Range(0f, 1f) < 0.1f * Time.deltaTime) // 10% chance per second
                {
                    PickNewTargetPosition();
                }
            }

            // Move towards target position
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            if (Vector2.Distance(transform.position, targetPosition) > 0.5f)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                isMovingToPosition = true;
            }
            else
            {
                isMovingToPosition = false;
            }
        }

        void PickNewTargetPosition()
        {
            if (attackPositions.Length > 0)
            {
                Transform randomPos = attackPositions[Random.Range(0, attackPositions.Length)];
                targetPosition = randomPos.position;
            }
            else
            {
                // Fallback: move towards or away from player
                Vector2 playerPos = player.position;
                Vector2 awayFromPlayer = ((Vector2)transform.position - playerPos).normalized;
                targetPosition = playerPos + awayFromPlayer * Random.Range(3f, 6f);
            }
        }

        void HandleAttacking()
        {
            if (Time.time >= nextAttackTime)
            {
                PerformAttack();
                SetNextAttackTime();
            }
        }

        void PerformAttack()
        {
            if (player == null) return;

            // Trigger bug system attack
            ExecuteRandomAttack();

            // Visual attack
            if (bossAnimator != null)
            {
                bossAnimator.SetTrigger("Attack");
            }

            // Actual attack logic (projectile or area damage)
            if (projectilePrefab != null)
                FireProjectile();
            else
                AreaAttack();
        }

        void FireProjectile()
        {
            Vector2 direction = (player.position - transform.position).normalized;
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * 8f; // Projectile speed
            }

            // Add damage component
            BossProjectile projScript = projectile.GetComponent<BossProjectile>();
            if (projScript == null)
                projScript = projectile.AddComponent<BossProjectile>();
            projScript.damage = (int)attackDamage;

            // Destroy after 5 seconds
            Destroy(projectile, 5f);
        }

        void AreaAttack()
        {
            // Simple area damage around boss
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 3f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    BossRoomPlayerController playerController = hit.GetComponent<BossRoomPlayerController>();
                    if (playerController != null)
                    {
                        playerController.TakeDamage((int)attackDamage);
                    }
                }
            }
        }

        void SetNextAttackTime()
        {
            float interval = Random.Range(minAttackInterval, maxAttackInterval);
            if (isPhase2) interval *= 0.7f; // More frequent attacks in phase 2
            nextAttackTime = Time.time + interval;
        }

        void UpdateBossAnimations()
        {
            if (bossAnimator == null) return;

            bossAnimator.SetBool("isMoving", isMovingToPosition);
            bossAnimator.SetFloat("healthPercent", currentHealth / maxHealth);
        }

        void AnimateConsole()
        {
            if (debugConsole != null && Random.Range(0f, 1f) < 0.1f * Time.deltaTime)
            {
                debugConsole.Rotate(0, 0, Random.Range(-2f, 2f));
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
            uiBetrayalSystem?.ShowFakeDialog("Installing Workplace Optimization Update...", 3f);

            if (updateSound != null && bossAudioSource != null)
                bossAudioSource.PlayOneShot(updateSound);

            yield return new WaitForSeconds(2f);

            // Increase bug intensity
            if (bugManager != null)
                bugManager.SetBugIntensity(BugManager.BugIntensity.Aggressive);

            // Start UI betrayal
            if (uiBetrayalSystem != null)
                uiBetrayalSystem.StartUIBetrayal();

            // More aggressive attacks
            minAttackInterval = 1f;
            maxAttackInterval = 3f;

            uiBetrayalSystem?.ShowFakeDialog("Update Complete! Optimizations Applied.", 2f);
        }

        void ExecuteRandomAttack()
        {
            if (currentPhase == BossPhase.Defeated) return;

            int attackType = Random.Range(0, isPhase2 ? 6 : 3);

            switch (attackType)
            {
                case 0:
                    StartCoroutine(CollisionParadoxAttack());
                    break;
                case 1:
                    StartCoroutine(InputDesyncAttack());
                    break;
                case 2:
                    StartCoroutine(GoalGaslightAttack());
                    break;
                case 3: // Phase 2 only
                    StartCoroutine(UIShuffleAttack());
                    break;
                case 4: // Phase 2 only
                    StartCoroutine(FakePopupAttack());
                    break;
                case 5: // Phase 2 only
                    StartCoroutine(ControlSchemeSwapAttack());
                    break;
            }
        }

        IEnumerator CollisionParadoxAttack()
        {
            collisionSystem?.TriggerCollisionParadox();
            yield return new WaitForSeconds(5f);
            collisionSystem?.ResetCollision();
        }

        IEnumerator InputDesyncAttack()
        {
            inputSystem?.TriggerInputDesync(Random.Range(0.5f, 2f));
            yield return new WaitForSeconds(3f);
        }

        IEnumerator GoalGaslightAttack()
        {
            gaslightingSystem?.TriggerGaslighting();
            yield return new WaitForSeconds(4f);
        }

        IEnumerator UIShuffleAttack()
        {
            uiBetrayalSystem?.ShuffleUIElements();
            yield return new WaitForSeconds(6f);
            uiBetrayalSystem?.ResetUIPositions();
        }

        IEnumerator FakePopupAttack()
        {
            uiBetrayalSystem?.SpawnFakePopup();
            yield return new WaitForSeconds(6f);
        }

        IEnumerator ControlSchemeSwapAttack()
        {
            inputSystem?.SwapControlScheme();
            uiBetrayalSystem?.ShowFakeDialog("Controls Updated to Optimize Workflow", 2f);
            yield return new WaitForSeconds(Random.Range(8f, 15f));
            inputSystem?.RestoreControlScheme();
        }

        public void TakeDamage(float damage)
        {
            if (isDefeated) return;

            // Sometimes gaslighting prevents actual damage
            if (gaslightingSystem != null && gaslightingSystem.ShouldPreventDamage())
            {
                gaslightingSystem.ShowFakeDamage();
                return;
            }

            currentHealth -= damage;

            // Flash red when hit
            StartCoroutine(HitFlash());

            // Sometimes add fake auto-heal
            if (Random.Range(0f, 1f) < 0.25f && currentHealth > 0 && currentHealth < maxHealth)
            {
                StartCoroutine(FakeAutoHeal());
            }

            if (currentHealth <= 0 && !isDefeated)
            {
                DefeatBoss();
            }
        }

        IEnumerator HitFlash()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color original = sr.color;
                sr.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                sr.color = original;
            }
        }

        IEnumerator FakeAutoHeal()
        {
            yield return new WaitForSeconds(Random.Range(1f, 3f));

            float healAmount = Random.Range(5f, 15f);
            currentHealth += healAmount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            uiBetrayalSystem?.ShowFakeDialog($"Auto-Heal Patch Applied (+{healAmount:F0} HP)", 2f);
        }

        void DefeatBoss()
        {
            if (isDefeated) return;

            isDefeated = true;
            currentPhase = BossPhase.Defeated;
            isFightActive = false;

            StartCoroutine(DefeatSequence());
        }

        IEnumerator DefeatSequence()
        {
            // Boss tries to spawn final update
            uiBetrayalSystem?.ShowFakeDialog("Initiating Emergency Compliance Protocol...", 3f);
            yield return new WaitForSeconds(3f);

            uiBetrayalSystem?.ShowFakeDialog("Loading patch 1.8.2...", 2f);
            yield return new WaitForSeconds(2f);

            // Get stuck in infinite loading
            uiBetrayalSystem?.ShowInfiniteLoading();

            // Stop all systems
            bugManager?.PauseAllBugs();
            StopCoroutine(PlayTypingSounds());

            // Enable door exit
            var exitDoor = FindObjectOfType<ExitDoorController>();
            if (exitDoor != null)
                exitDoor.EnableExit();

            yield return new WaitForSeconds(3f);

            // Final victory message
            uiBetrayalSystem?.ShowFakeDialog("System.exe has stopped responding.", 5f);
        }

        IEnumerator PlayTypingSounds()
        {
            while (isFightActive && !isDefeated)
            {
                if (typingClips.Length > 0 && Random.Range(0f, 1f) < 0.3f && bossAudioSource != null)
                {
                    AudioClip clip = typingClips[Random.Range(0, typingClips.Length)];
                    bossAudioSource.PlayOneShot(clip, 0.2f);
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

        void OnDrawGizmos()
        {
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 3f);

            // Draw movement target
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
        }
    }

    // Simple projectile component
    public class BossProjectile : MonoBehaviour
    {
        public int damage = 1;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                BossRoomPlayerController player = other.GetComponent<BossRoomPlayerController>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }

                Destroy(gameObject);
            }
            else if (other.CompareTag("Wall") || other.CompareTag("Ground"))
            {
                Destroy(gameObject);
            }
        }
    }
}