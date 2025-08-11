using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BossRoom
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BossController : MonoBehaviour
    {
        [Header("Stats")]
        public float maxRealHealth = 100f;     // actual health that determines defeat
        public float attackDamage = 4f;
        public float baseMoveSpeed = 2.5f;

        [Header("Attacks")]
        public Transform[] attackPositions;
        public GameObject projectilePrefab;
        public float projectileSpeed = 8f;
        public float baseMinAttackInterval = 2.5f;
        public float baseMaxAttackInterval = 5.0f;

        [Header("Phase / Timing")]
        public float phase1Duration = 150f; // ~2.5 minutes
        public float totalFightDuration = 420f; // ~7 minutes default
        public float escalationInterval = 30f; // every 30s escalate
        public float GetHealthPercentage() => realHealth / maxRealHealth;

        [Header("Projectile Patterns")]
        public Vector2 phase1ProjectileInterval = new Vector2(2.2f, 3.2f);
        public Vector2 phase2ProjectileInterval = new Vector2(1.0f, 1.6f);
        public int phase1BurstCount = 1;
        public int phase2BurstCount = 3;
        public float phase1ProjectileSpeed = 6f;
        public float phase2ProjectileSpeed = 9f;

        [Header("Debug / Control")]
        public bool autoStartFight = false;

        public enum BossPhase
        {
            Phase1,
            Phase2,
            Defeated
        }

        public BossPhase currentPhase
        {
            get
            {
                if (isDefeated) return BossPhase.Defeated;
                return isPhase2 ? BossPhase.Phase2 : BossPhase.Phase1;
            }
        }

        public float CurrentIntensity => bugManager != null ? bugManager.intensityValue : 0f;

        [Header("Audio / VFX")]
        public AudioSource bossAudio;
        public AudioClip updateSound;
        public AudioClip typingClip;
        public AudioClip errorClip;

        [Header("References (optional)")]
        public BossUIManager bossUIManager; // expects UpdateHealthBar(float fraction, bool isHonest)
        public Animator bossAnimator;
        public Transform debugConsole;

        [Header("Bug Systems")]
        public BugManager bugManager;
        public CollisionParadoxSystem collisionSystem;
        public InputDesyncSystem inputSystem;
        public GoalGaslightingSystem gaslightingSystem;
        public UIBetrayalSystem uiBetrayalSystem;

        // internal state
        private float realHealth;
        private float visualHealth; // what UI shows (can be false)
        private bool isFightActive = false;
        private bool isDefeated = false;
        private float fightTimer = 0f;
        private float nextAttackTime;
        private float nextEscalationTime;
        private Transform player;
        private Vector2 targetPosition;
        private bool isPhase2 = false;
        private float minAttackInterval;
        private float maxAttackInterval;
        private float moveSpeed;
        private Coroutine victoryCoroutine;
        private Coroutine playerGlitchRoutine;
        private Coroutine projectileLoopRoutine;

        // attack weights (phase-aware)
        private List<WeightedAttack> phase1Pool;
        private List<WeightedAttack> phase2Pool;
        private System.Random rng = new System.Random();

        void Awake()
        {
            // default in case not set in inspector
            if (bossUIManager == null) bossUIManager = FindObjectOfType<BossUIManager>();
            if (bugManager == null) bugManager = GetComponent<BugManager>();
            if (collisionSystem == null) collisionSystem = GetComponent<CollisionParadoxSystem>();
            if (inputSystem == null) inputSystem = FindObjectOfType<InputDesyncSystem>();
            if (gaslightingSystem == null) gaslightingSystem = GetComponent<GoalGaslightingSystem>();
            if (uiBetrayalSystem == null) uiBetrayalSystem = FindObjectOfType<UIBetrayalSystem>();

            EnsureProjectilePrefab();
        }

        void Start()
        {
            realHealth = maxRealHealth;
            visualHealth = maxRealHealth; // starts honest
            moveSpeed = baseMoveSpeed;
            minAttackInterval = baseMinAttackInterval;
            maxAttackInterval = baseMaxAttackInterval;

            FindPlayer();
            InitializeAttackPools();
            SetNextAttackTime();
            nextEscalationTime = escalationInterval;

            // initial target
            targetPosition = transform.position;

            if (autoStartFight)
            {
                StartBossFight();
            }
        }

        void Update()
        {
            // Not active until StartBossFight is called
            if (!isFightActive || isDefeated) return;

            fightTimer += Time.deltaTime;

            // Escalation over time
            if (fightTimer >= nextEscalationTime)
            {
                Escalate();
                nextEscalationTime += escalationInterval;
            }

            // Update visual health slowly toward real health but with opportunities for false jumps
            UpdateVisualHealth();

            // Update UI: pass whether boss is 'honest' about display this frame
            bool honestThisFrame = !(gaslightingSystem?.IsCurrentlyGaslighting() ?? false);
            bossUIManager?.UpdateHealthBar(visualHealth / maxRealHealth, honestThisFrame);

            // AI duties
            HandleMovement();
            HandleAttacking();

            // Phase check
            if (!isPhase2 && (fightTimer >= phase1Duration || realHealth <= maxRealHealth * 0.6f))
            {
                StartCoroutine(EnterPhase2());
            }

            // Safety: force defeat after total fight time
            if (fightTimer >= totalFightDuration)
            {
                realHealth = 0;
                TryDefeat();
            }

            AnimateConsoleOccasionally();
            UpdateAnimator();
        }

        #region Public control
        public void StartBossFight()
        {
            if (isFightActive) return;
            isFightActive = true;
            fightTimer = 0f;
            isDefeated = false;
            realHealth = maxRealHealth;
            visualHealth = maxRealHealth;
            minAttackInterval = baseMinAttackInterval;
            maxAttackInterval = baseMaxAttackInterval;
            moveSpeed = baseMoveSpeed;
            nextEscalationTime = escalationInterval;

            bugManager?.SetBugIntensity(BugManager.BugIntensity.Mild);
            bossAnimator?.SetTrigger("StartFight");
            StartCoroutine(BackgroundTyping());

            // Start projectile pacing loop (easy pace in Phase 1)
            if (projectileLoopRoutine != null) StopCoroutine(projectileLoopRoutine);
            projectileLoopRoutine = StartCoroutine(ProjectileLoop());
        }

        [ContextMenu("Start Boss Fight (Debug)")]
        void ContextStartFight()
        {
            StartBossFight();
        }

        public void StopBossFight()
        {
            isFightActive = false;
            StopAllCoroutines();
            if (playerGlitchRoutine != null)
            {
                StopCoroutine(playerGlitchRoutine);
                playerGlitchRoutine = null;
            }
            if (projectileLoopRoutine != null)
            {
                StopCoroutine(projectileLoopRoutine);
                projectileLoopRoutine = null;
            }
        }
        #endregion

        #region Movement / AI
        void HandleMovement()
        {
            if (player == null) return;

            // occasionally pick a new strategic target
            if (Random.value < 0.12f * Time.deltaTime && !IsMoving())
            {
                PickNewTargetPosition();
            }

            // move toward target
            if (Vector2.Distance(transform.position, targetPosition) > 0.5f)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }

        bool IsMoving()
        {
            return Vector2.Distance(transform.position, targetPosition) > 0.5f;
        }

        void PickNewTargetPosition()
        {
            if (attackPositions != null && attackPositions.Length > 0)
            {
                targetPosition = attackPositions[Random.Range(0, attackPositions.Length)].position;
            }
            else if (player != null)
            {
                // move to a point offset from player
                Vector2 p = player.position;
                Vector2 dir = ((Vector2)transform.position - p).normalized;
                targetPosition = p + dir * Random.Range(3f, 6f);
            }
            else
            {
                targetPosition = (Vector2)transform.position + Random.insideUnitCircle * 2f;
            }
        }
        #endregion

        #region Attacking / Pools
        void InitializeAttackPools()
        {
            phase1Pool = new List<WeightedAttack>
            {
                new WeightedAttack(AttackType.CollisionParadox, 30),
                new WeightedAttack(AttackType.InputDesync, 25),
                new WeightedAttack(AttackType.GoalGaslight, 20),
                new WeightedAttack(AttackType.ProjectileBurst, 25)
            };

            phase2Pool = new List<WeightedAttack>
            {
                new WeightedAttack(AttackType.CollisionParadox, 20),
                new WeightedAttack(AttackType.InputDesync, 20),
                new WeightedAttack(AttackType.GoalGaslight, 20),
                new WeightedAttack(AttackType.UIShuffle, 25),
                new WeightedAttack(AttackType.FakePopup, 20),
                new WeightedAttack(AttackType.ControlSwap, 20),
                new WeightedAttack(AttackType.ProjectileBurst, 25),
                new WeightedAttack(AttackType.AreaBurst, 20)
            };
        }

        void HandleAttacking()
        {
            if (Time.time >= nextAttackTime)
            {
                LaunchAttack();
                SetNextAttackTime();
            }
        }

        void SetNextAttackTime()
        {
            float interval = Random.Range(minAttackInterval, maxAttackInterval);
            if (isPhase2) interval *= Random.Range(0.7f, 0.95f); // faster in phase 2
            nextAttackTime = Time.time + interval;
        }

        void LaunchAttack()
        {
            // Choose weighted attack based on phase
            AttackType choice = ChooseWeightedAttack();
            // Allow combination: many attacks also trigger a projectile or area effect
            switch (choice)
            {
                case AttackType.CollisionParadox:
                    StartCoroutine(ExecuteCollisionParadox(Random.Range(4f, 8f)));
                    break;
                case AttackType.InputDesync:
                    StartCoroutine(ExecuteInputDesync(Random.Range(0.6f, 2.2f)));
                    break;
                case AttackType.GoalGaslight:
                    StartCoroutine(ExecuteGoalGaslight(Random.Range(3f, 6f)));
                    break;
                case AttackType.UIShuffle:
                    StartCoroutine(ExecuteUIShuffle(Random.Range(5f, 9f)));
                    break;
                case AttackType.FakePopup:
                    StartCoroutine(ExecuteFakePopup(Random.Range(4f, 8f)));
                    break;
                case AttackType.ControlSwap:
                    StartCoroutine(ExecuteControlSwap(Random.Range(6f, 12f)));
                    break;
                case AttackType.ProjectileBurst:
                    FireProjectileBurst(3, 0.12f);
                    break;
                case AttackType.AreaBurst:
                    StartCoroutine(ExecuteAreaBurst());
                    break;
            }

            // Slightly reduce pair chance in Phase 1 to keep things fair
            float pairRoll = Random.value;
            float pairChance = isPhase2 ? 0.7f : 0.4f;
            if (pairRoll < pairChance)
            {
                if (Random.value < 0.6f) FireSingleProjectileAtPlayer();
                else StartCoroutine(ExecuteAreaBurst());
            }
        }

        AttackType ChooseWeightedAttack()
        {
            var pool = isPhase2 ? phase2Pool : phase1Pool;
            int totalWeight = 0;
            for (int i = 0; i < pool.Count; i++) totalWeight += pool[i].weight;
            int pick = rng.Next(0, Mathf.Max(1, totalWeight));
            int cum = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                cum += pool[i].weight;
                if (pick < cum) return pool[i].attackType;
            }
            return pool[0].attackType;
        }

        #endregion

        #region Specific Attacks (coroutines)
        IEnumerator ExecuteCollisionParadox(float duration)
        {
            collisionSystem?.TriggerCollisionParadox(duration);
            // sometimes chain another minor bug
            if (Random.value < (isPhase2 ? 0.6f : 0.25f))
            {
                yield return new WaitForSeconds(Random.Range(0.5f, 1.2f));
                inputSystem?.TriggerMinorDesync(0.4f);
            }
            yield return new WaitForSeconds(duration);
            collisionSystem?.ResetCollision();
        }

        IEnumerator ExecuteInputDesync(float duration)
        {
            inputSystem?.TriggerInputDesync(duration);
            // little visual lie to sell it
            uiBetrayalSystem?.ShowFakeTooltip("Network Timeout");
            yield return new WaitForSeconds(duration);
            inputSystem?.RestoreControlScheme();
        }

        IEnumerator ExecuteGoalGaslight(float duration)
        {
            if (gaslightingSystem != null)
                yield return StartCoroutine(gaslightingSystem.TriggerGaslightingTimed(duration));
            else
                yield return null;

            // random fake heal sometimes during gaslighting
            if (Random.value < 0.3f)
            {
                float fakeHeal = Random.Range(5f, 12f);
                visualHealth = Mathf.Min(maxRealHealth, visualHealth + fakeHeal);
                uiBetrayalSystem?.ShowFakeDialog($"Auto-Heal Patch Applied (+{fakeHeal:F0} HP)");
            }

            gaslightingSystem?.StopGaslighting();
        }


        IEnumerator ExecuteUIShuffle(float duration)
        {
            uiBetrayalSystem?.ShuffleUIElements();
            // occasionally spawn fake windows that block gameplay
            uiBetrayalSystem?.SpawnFakePopup();
            yield return new WaitForSeconds(duration);
            uiBetrayalSystem?.ResetUIPositions();
        }

        IEnumerator ExecuteFakePopup(float duration)
        {
            uiBetrayalSystem?.SpawnFakePopup();
            yield return new WaitForSeconds(duration);
        }

        IEnumerator ExecuteControlSwap(float duration)
        {
            inputSystem?.SwapControlSchemeTemporarily(duration);
            uiBetrayalSystem?.ShowFakeDialog("Controls Updated to Optimize Workflow", 1.5f);
            yield return new WaitForSeconds(duration);
            inputSystem?.RestoreControlScheme();
        }


        IEnumerator ExecuteAreaBurst()
        {
            AreaAttack();
            yield return null;
        }

        #endregion

        #region Projectiles / Attacks
        void FireSingleProjectileAtPlayer()
        {
            if (player == null) return;
            EnsureProjectilePrefab();

            Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            proj.SetActive(true);
            var rb = proj.GetComponent<Rigidbody2D>() ?? proj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            rb.gravityScale = 0f;
            rb.linearVelocity = dir * projectileSpeed;
            var projScript = proj.GetComponent<BossProjectile>() ?? proj.AddComponent<BossProjectile>();
            projScript.damage = Mathf.CeilToInt(attackDamage);
            Destroy(proj, 5f);
        }

        void FireSingleProjectileAtPlayer(float customSpeed)
        {
            if (player == null) return;
            EnsureProjectilePrefab();

            Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            proj.SetActive(true);
            var rb = proj.GetComponent<Rigidbody2D>() ?? proj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            rb.gravityScale = 0f;
            rb.linearVelocity = dir * customSpeed;
            var projScript = proj.GetComponent<BossProjectile>() ?? proj.AddComponent<BossProjectile>();
            projScript.damage = Mathf.CeilToInt(attackDamage);
            Destroy(proj, 5f);
        }

        void FireProjectileBurst(int count, float spreadInterval)
        {
            StartCoroutine(ProjectileBurstCoroutine(count, spreadInterval));
        }

        void FireProjectileBurst(int count, float spreadInterval, float customSpeed)
        {
            StartCoroutine(ProjectileBurstCoroutine(count, spreadInterval, customSpeed));
        }

        IEnumerator ProjectileBurstCoroutine(int count, float delay)
        {
            for (int i = 0; i < count; i++)
            {
                FireSingleProjectileAtPlayer();
                yield return new WaitForSeconds(delay);
            }
        }

        IEnumerator ProjectileBurstCoroutine(int count, float delay, float customSpeed)
        {
            for (int i = 0; i < count; i++)
            {
                FireSingleProjectileAtPlayer(customSpeed);
                yield return new WaitForSeconds(delay);
            }
        }

        void AreaAttack()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 3f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    var pc = hit.GetComponent<BossRoomPlayerController>();
                    pc?.TakeDamage(Mathf.CeilToInt(attackDamage));
                }
            }
        }

        IEnumerator ProjectileLoop()
        {
            while (isFightActive && !isDefeated)
            {
                if (player == null)
                {
                    FindPlayer();
                }

                if (!isPhase2)
                {
                    float wait = Random.Range(phase1ProjectileInterval.x, phase1ProjectileInterval.y);
                    yield return new WaitForSeconds(wait);
                    if (!isFightActive || isDefeated) break;
                    if (phase1BurstCount <= 1)
                        FireSingleProjectileAtPlayer(phase1ProjectileSpeed);
                    else
                        FireProjectileBurst(phase1BurstCount, 0.2f, phase1ProjectileSpeed);
                }
                else
                {
                    float wait = Random.Range(phase2ProjectileInterval.x, phase2ProjectileInterval.y);
                    yield return new WaitForSeconds(wait);
                    if (!isFightActive || isDefeated) break;
                    FireProjectileBurst(phase2BurstCount, 0.15f, phase2ProjectileSpeed);
                }
            }
        }

        void EnsureProjectilePrefab()
        {
            if (projectilePrefab != null) return;

            // Create a basic projectile template on the fly (hidden, used only for cloning)
            var go = new GameObject("BossProjectile_Default");
            go.SetActive(false);
            go.hideFlags = HideFlags.HideAndDontSave;
            var sr = go.AddComponent<SpriteRenderer>();
            // simple white circle texture
            Texture2D tex = new Texture2D(16, 16, TextureFormat.ARGB32, false);
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    float dx = x - 8; float dy = y - 8;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    tex.SetPixel(x, y, d <= 7 ? Color.white : new Color(1,1,1,0));
                }
            }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
            sr.sortingOrder = 50;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            go.AddComponent<BossProjectile>();
            projectilePrefab = go;
        }
        #endregion

        #region Health / Damage / Visual Lies
        public void TakeDamage(float requestedDamage)
        {
            if (isDefeated || !isFightActive) return;

            // Gaslighting can intercept or make the damage appear to not apply
            if (gaslightingSystem != null && gaslightingSystem.ShouldBlockDamage())
            {
                gaslightingSystem?.ShowFakeDamage();
                // occasionally show visual damage without real effect
                if (Random.value < 0.6f)
                {
                    visualHealth = Mathf.Max(0f, visualHealth - requestedDamage * Random.Range(0.6f, 1.2f));
                }
                return;
            }

            // Apply damage to real health
            realHealth -= requestedDamage;
            realHealth = Mathf.Max(0f, realHealth);

            // Quick hit flash
            StartCoroutine(HitFlash());

            // Sometimes apply a small fake auto-heal visual
            if (Random.value < 0.22f && realHealth > 0f)
            {
                float heal = Random.Range(4f, 12f);
                StartCoroutine(DelayedFakeHeal(heal, Random.Range(0.8f, 2.5f)));
            }

            TryDefeat();
        }

        IEnumerator DelayedFakeHeal(float healAmount, float delay)
        {
            yield return new WaitForSeconds(delay);
            visualHealth = Mathf.Min(maxRealHealth, visualHealth + healAmount);
            uiBetrayalSystem?.ShowFakeDialog($"Auto-Heal Patch Applied (+{healAmount:F0} HP)");
        }

        void TryDefeat()
        {
            if (realHealth <= 0f && !isDefeated)
            {
                isDefeated = true;
                isFightActive = false;
                // ensure visual also indicates death (but we still want the 'stuck update' moment)
                visualHealth = 0f;
                bossUIManager?.UpdateHealthBar(0f, true);
                victoryCoroutine = StartCoroutine(DefeatSequence());
            }
        }

        IEnumerator HitFlash()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color original = sr.color;
                sr.color = Color.red;
                yield return new WaitForSeconds(0.08f);
                sr.color = original;
            }
        }

        void UpdateVisualHealth()
        {
            // Visual health lerps toward real health with a small jitter (gaslighting may add false jumps)
            float lerpSpeed = isPhase2 ? 0.6f : 0.35f;
            visualHealth = Mathf.Lerp(visualHealth, realHealth, lerpSpeed * Time.deltaTime);

            // Occasionally, when gaslighting, nudge the visual up or down to lie
            if (gaslightingSystem != null && gaslightingSystem.IsCurrentlyGaslighting() && Random.value < 0.01f)
            {
                visualHealth = Mathf.Clamp(visualHealth + Random.Range(-8f, 10f), 0f, maxRealHealth);
            }
        }
        #endregion

        #region Phase Transition / Escalation
        IEnumerator EnterPhase2()
        {
            isPhase2 = true;
            // audiovisual cue
            uiBetrayalSystem?.ShowFakeDialog("Installing Workplace Optimization Update...", 2.5f);
            bossAudio?.PlayOneShot(updateSound);
            yield return new WaitForSeconds(2.1f);

            // escalate bug manager & intensify behavior
            bugManager?.SetBugIntensity(BugManager.BugIntensity.Aggressive);

            // make phase2 changes
            minAttackInterval = Mathf.Max(0.9f, baseMinAttackInterval * 0.8f);
            maxAttackInterval = Mathf.Max(1.5f, baseMaxAttackInterval * 0.8f);
            moveSpeed = baseMoveSpeed * 1.15f;

            uiBetrayalSystem?.StartUIBetrayal();
            uiBetrayalSystem?.ShowFakeDialog("Update Complete! Optimizations Applied.", 1.7f);

            // Start intermittent player visual glitches to sell the patch gone wrong
            if (playerGlitchRoutine != null)
            {
                StopCoroutine(playerGlitchRoutine);
            }
            playerGlitchRoutine = StartCoroutine(PlayerVisualGlitchRoutine());

            yield return null;
        }

        void Escalate()
        {
            // tighten intervals and increase aggression little by little
            minAttackInterval = Mathf.Max(0.6f, minAttackInterval * 0.92f);
            maxAttackInterval = Mathf.Max(1.1f, maxAttackInterval * 0.92f);
            moveSpeed += 0.08f;

            // increase bug intensity gradually if present
            if (bugManager != null)
            {
                bugManager?.IncreaseIntensity(0.1f);
            }

            // occasional ambient noise to add pressure
            if (Random.value < 0.5f && bossAudio != null && typingClip != null)
                bossAudio.PlayOneShot(typingClip, 0.18f);
        }
        #endregion

        #region Victory / Defeat
        IEnumerator DefeatSequence()
        {
            // the boss tries to install emergency update and gets stuck
            uiBetrayalSystem?.ShowFakeDialog("Initiating Emergency Compliance Protocol...", 2.6f);
            yield return new WaitForSeconds(2.6f);

            uiBetrayalSystem?.ShowFakeDialog("Loading patch 1.8.2...", 1.6f);
            yield return new WaitForSeconds(1.2f);

            uiBetrayalSystem?.ShowInfiniteLoading();

            // freeze bug systems but leave UI obsessed
            bugManager?.PauseAllBugs();
            inputSystem?.DisablePlayerInput(); // temporarily lock inputs to simulate freeze
            yield return new WaitForSeconds(0.4f);

            // enable exit and let player walk away; behind the scenes, boss is 'paused'
            var exit = FindObjectOfType<ExitDoorController>();
            if (exit != null) exit.EnableExit();

            // final message after a beat
            yield return new WaitForSeconds(3f);
            uiBetrayalSystem?.ShowFakeDialog("System.exe has stopped responding.", 4f);

            // restore input and clean up after a little while
            yield return new WaitForSeconds(2f);
            inputSystem?.RestoreControlScheme();

            FindFirstObjectByType<BossRoomPlayerController>().beatBoss = true;
        }
        #endregion

        #region Helpers / Misc
        void FindPlayer()
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
            {
                player = go.transform;
                if (collisionSystem != null)
                {
                    // Ensure the paradox system has both the boss instance and player instance
                    collisionSystem.Initialize(this, player);
                }
                if (inputSystem != null)
                {
                    var pc = go.GetComponent<BossRoomPlayerController>();
                    if (pc != null) inputSystem.SetPlayerController(pc);
                }
                if (gaslightingSystem != null) gaslightingSystem.SetPlayer(go);
            }
        }

        IEnumerator PlayerVisualGlitchRoutine()
        {
            while (isPhase2 && isFightActive && !isDefeated)
            {
                if (player != null && Random.value < 0.35f)
                {
                    yield return StartCoroutine(TemporaryPlayerAssetSwap(Random.Range(0.8f, 1.8f)));
                }
                yield return new WaitForSeconds(Random.Range(3f, 6f));
            }
        }

        IEnumerator TemporaryPlayerAssetSwap(float duration)
        {
            if (player == null) yield break;
            var sr = player.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;

            var originalSprite = sr.sprite;
            var originalColor = sr.color;

            // Create a simple placeholder "office asset" (a beige folder-like rectangle)
            Texture2D tex = new Texture2D(32, 32, TextureFormat.ARGB32, false);
            Color folder = new Color(0.95f, 0.88f, 0.64f, 1f);
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    tex.SetPixel(x, y, folder);
                }
            }
            tex.Apply();
            var tempSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 16f);

            sr.sprite = tempSprite;
            sr.color = Color.white;

            yield return new WaitForSeconds(duration);

            // Restore
            sr.sprite = originalSprite;
            sr.color = originalColor;
        }

        IEnumerator BackgroundTyping()
        {
            while (isFightActive && !isDefeated)
            {
                if (bossAudio != null && typingClip != null && Random.value < 0.35f)
                    bossAudio.PlayOneShot(typingClip, 0.18f);
                yield return new WaitForSeconds(Random.Range(0.6f, 2.2f));
            }
        }

        void AnimateConsoleOccasionally()
        {
            if (debugConsole != null && Random.value < 0.08f * Time.deltaTime)
                debugConsole.Rotate(0, 0, Random.Range(-2f, 2f));
        }

        void UpdateAnimator()
        {
            if (bossAnimator == null) return;
            bossAnimator.SetBool("isMoving", IsMoving());
            bossAnimator.SetFloat("healthPercent", realHealth / maxRealHealth);
            bossAnimator.SetFloat("visualHealthPercent", visualHealth / maxRealHealth);
        }

        void FireProjectileAtPosition(Vector2 pos)
        {
            if (projectilePrefab == null) return;
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            proj.SetActive(true);
            Vector2 dir = (pos - (Vector2)transform.position).normalized;
            var rb = proj.GetComponent<Rigidbody2D>() ?? proj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            rb.gravityScale = 0f;
            rb.linearVelocity = dir * projectileSpeed;
            var ps = proj.GetComponent<BossProjectile>() ?? proj.AddComponent<BossProjectile>();
            ps.damage = Mathf.CeilToInt(attackDamage);
            Destroy(proj, 5f);
        }
        #endregion

        #region Debug Gizmos
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 3f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetPosition, 0.4f);
        }
        #endregion

        #region Supporting types
        enum AttackType
        {
            CollisionParadox,
            InputDesync,
            GoalGaslight,
            UIShuffle,
            FakePopup,
            ControlSwap,
            ProjectileBurst,
            AreaBurst
        }

        class WeightedAttack
        {
            public AttackType attackType;
            public int weight;
            public WeightedAttack(AttackType a, int w) { attackType = a; weight = w; }
        }
        #endregion
    }

    // --- Minimal / stubbable systems so this file is runnable and editable quickly ---
    public class BossProjectile : MonoBehaviour
    {
        public int damage = 1;
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                var p = other.GetComponent<BossRoomPlayerController>();
                p?.TakeDamage(damage);
                Destroy(gameObject);
            }
            else if (other.CompareTag("Wall") || other.CompareTag("Ground"))
            {
                Destroy(gameObject);
            }
        }
    }

}
