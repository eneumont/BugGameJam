using UnityEngine;
using System.Collections;

namespace BossRoom
{
    /// <summary>
    /// Main setup manager for the Mr. Compliance boss fight
    /// Handles initialization, scene setup, and coordinates all boss room systems
    /// </summary>
    public class BossRoomSetupManager : MonoBehaviour
    {
        [Header("Boss Room References")]
        public BossController bossController;
        public BugManager bugManager;
        public UIBetrayalSystem uiSystem;
        public CollisionParadoxSystem collisionSystem;
        public InputDesyncSystem inputSystem;
        public GoalGaslightingSystem gaslightingSystem;
        public ExitDoorController exitDoor;

        [Header("Scene Setup")]
        public Transform playerSpawnPoint;
        public Transform bossSpawnPoint;
        public GameObject roomBoundaries;

        [Header("Audio")]
        public AudioSource ambientAudioSource;
        public AudioClip roomAmbientSound;
        public AudioClip bossIntroSound;

        [Header("Intro Sequence")]
        public bool playIntroSequence = true;
        public float introDelay = 2f;
        public GameObject introDialogPanel;

        [Header("Debug")]
        public bool skipIntro = false;
        public bool enableDebugOutput = true;

        private bool isSetupComplete = false;
        private bool isBossFightActive = false;
        private GameObject player;

        // Intro dialog lines
        private string[] introDialogLines = {
            "Please take a seat. We need to discuss your performance.",
            "Don't worry, this is just a routine compliance review.",
            "Let's begin the assessment...",
            "System initialization complete."
        };

        void Start()
        {
            StartCoroutine(InitializeBossRoom());
        }

        IEnumerator InitializeBossRoom()
        {
            DebugLog("Initializing Boss Room...");

            // Find required components if not assigned
            FindRequiredComponents();

            // Setup the room
            SetupRoomEnvironment();

            // Position player and boss
            PositionEntities();

            // Initialize all systems
            InitializeAllSystems();

            // Wait for everything to be ready
            yield return new WaitForSeconds(0.5f);

            // Play intro sequence
            if (playIntroSequence && !skipIntro)
            {
                yield return StartCoroutine(PlayIntroSequence());
            }
            else
            {
                StartBossFight();
            }

            isSetupComplete = true;
            DebugLog("Boss Room setup complete!");
        }

        void FindRequiredComponents()
        {
            // Find components automatically if not assigned
            if (bossController == null)
                bossController = FindObjectOfType<BossController>();

            if (bugManager == null)
                bugManager = FindObjectOfType<BugManager>();

            if (uiSystem == null)
                uiSystem = FindObjectOfType<UIBetrayalSystem>();

            if (collisionSystem == null)
                collisionSystem = FindObjectOfType<CollisionParadoxSystem>();

            if (inputSystem == null)
                inputSystem = FindObjectOfType<InputDesyncSystem>();

            if (gaslightingSystem == null)
                gaslightingSystem = FindObjectOfType<GoalGaslightingSystem>();

            if (exitDoor == null)
                exitDoor = FindObjectOfType<ExitDoorController>();

            // Find player
            player = GameObject.FindGameObjectWithTag("Player");
        }

        void SetupRoomEnvironment()
        {
            // Setup ambient audio
            if (ambientAudioSource != null && roomAmbientSound != null)
            {
                ambientAudioSource.clip = roomAmbientSound;
                ambientAudioSource.loop = true;
                ambientAudioSource.volume = 0.3f;
                ambientAudioSource.Play();
            }

            // Ensure room boundaries are active
            if (roomBoundaries != null)
                roomBoundaries.SetActive(true);

            // Setup lighting or other environmental effects here
            SetupLighting();
        }

        void SetupLighting()
        {
            // Find and adjust lighting for boss room atmosphere
            Light[] lights = FindObjectsOfType<Light>();

            foreach (Light light in lights)
            {
                if (light.name.Contains("Room") || light.name.Contains("Boss"))
                {
                    // Slightly dim lighting for atmosphere
                    light.intensity *= 0.8f;
                    light.color = Color.Lerp(light.color, new Color(0.9f, 0.95f, 1f), 0.3f);
                }
            }
        }

        void PositionEntities()
        {
            // Position player at spawn point
            if (player != null && playerSpawnPoint != null)
            {
                player.transform.position = playerSpawnPoint.position;
                player.transform.rotation = playerSpawnPoint.rotation;

                // Disable player movement temporarily during intro
                if (playIntroSequence && !skipIntro)
                {
                    var playerControllerComp = player.GetComponent<BossRoomPlayerController>();
                    if (playerControllerComp != null)
                        playerControllerComp.enabled = false;
                }
            }

            // Position boss at spawn point
            if (bossController != null && bossSpawnPoint != null)
            {
                bossController.transform.position = bossSpawnPoint.position;
                bossController.transform.rotation = bossSpawnPoint.rotation;
            }
        }

        void InitializeAllSystems()
        {
            DebugLog("Initializing all boss room systems...");

            // Initialize systems in order of dependency
            if (bugManager != null)
            {
                bugManager.SetBugIntensity(BugManager.BugIntensity.Mild);
                DebugLog("Bug Manager initialized");
            }

            if (uiSystem != null)
            {
                uiSystem.SetIntensity(BugManager.BugIntensity.Mild);
                DebugLog("UI Betrayal System initialized");
            }

            if (collisionSystem != null)
            {
                // Ensure player and boss references are bound properly
                Transform playerTransform = player != null ? player.transform : null;
                if (bossController != null || playerTransform != null)
                {
                    collisionSystem.Initialize(bossController, playerTransform);
                }
                collisionSystem.SetIntensity(BugManager.BugIntensity.Mild);
                DebugLog("Collision Paradox System initialized and wired to scene objects");
            }

            if (inputSystem != null)
            {
                inputSystem.SetIntensity(BugManager.BugIntensity.Mild);
                var pc = player != null ? player.GetComponent<BossRoomPlayerController>() : null;
                if (pc != null) inputSystem.SetPlayerController(pc);
                DebugLog("Input Desync System initialized");
            }

            if (gaslightingSystem != null)
            {
                gaslightingSystem.SetIntensity(BugManager.BugIntensity.Mild);
                if (player != null) gaslightingSystem.SetPlayer(player);
                DebugLog("Goal Gaslighting System initialized");
            }

            if (exitDoor != null)
            {
                // Door should start locked
                DebugLog("Exit Door initialized (locked)");
            }
        }

        IEnumerator PlayIntroSequence()
        {
            DebugLog("Starting intro sequence...");

            // Show intro dialog panel
            if (introDialogPanel != null)
                introDialogPanel.SetActive(true);

            // Play boss intro sound
            if (bossController != null && bossIntroSound != null)
                bossController.GetComponent<AudioSource>()?.PlayOneShot(bossIntroSound);

            // Display intro dialog lines
            for (int i = 0; i < introDialogLines.Length; i++)
            {
                if (uiSystem != null)
                    uiSystem.ShowFakeDialog(introDialogLines[i], 2.5f);

                yield return new WaitForSeconds(3f);
            }

            // Hide intro panel
            if (introDialogPanel != null)
                introDialogPanel.SetActive(false);

            yield return new WaitForSeconds(introDelay);

            // Re-enable player movement
            if (player != null)
            {
                var playerControllerComp = player.GetComponent<BossRoomPlayerController>();
                if (playerControllerComp != null)
                    playerControllerComp.enabled = true;
            }

            // Start boss fight
            StartBossFight();
        }

        void StartBossFight()
        {
            DebugLog("Starting boss fight!");

            isBossFightActive = true;

            if (bossController != null)
            {
                // Also ensure boss has the player reference bound for systems that depend on it
                if (collisionSystem != null && player != null)
                {
                    collisionSystem.SetPlayer(player.transform);
                }
                bossController.StartBossFight();
            }

            // Start monitoring boss fight progress
            StartCoroutine(MonitorBossFight());
        }

        IEnumerator MonitorBossFight()
        {
            while (isBossFightActive && bossController != null)
            {
                // Check boss health and phase transitions
                if (bossController.currentPhase == BossController.BossPhase.Defeated)
                {
                    yield return StartCoroutine(HandleBossDefeat());
                    break;
                }

                // Monitor system health
                MonitorSystemHealth();

                yield return new WaitForSeconds(1f);
            }
        }

        void MonitorSystemHealth()
        {
            // Check if any critical systems have failed
            if (bugManager == null || uiSystem == null)
            {
                DebugLog("Warning: Critical boss room system is missing!");
            }

            // Optional: Reset stuck systems
            if (inputSystem != null && Input.GetKeyDown(KeyCode.F1) && enableDebugOutput)
            {
                inputSystem.ClearDesync(); // Fixed method name
                DebugLog("Debug: Cleared all input desyncs");
            }
        }


        IEnumerator HandleBossDefeat()
        {
            DebugLog("Boss defeated! Starting victory sequence...");

            isBossFightActive = false;

            // Brief delay for boss defeat animation
            yield return new WaitForSeconds(1f);

            // Fade out ambient audio
            if (ambientAudioSource != null)
            {
                float fadeTime = 3f;
                float startVolume = ambientAudioSource.volume;

                for (float elapsed = 0; elapsed < fadeTime; elapsed += Time.deltaTime)
                {
                    ambientAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
                    yield return null;
                }

                ambientAudioSource.Stop();
            }

            // Clean up all systems
            CleanupBossRoom();

            DebugLog("Boss room cleanup complete!");
        }

        void CleanupBossRoom()
        {
            // Reset all bug systems
            if (bugManager != null)
                bugManager.PauseAllBugs();

            if (uiSystem != null)
                uiSystem.ResetAllUI();

            if (collisionSystem != null)
                collisionSystem.ResetCollision();

            if (inputSystem != null)
                inputSystem.ClearDesync(); // Fixed method name

            if (gaslightingSystem != null)
                gaslightingSystem.ClearGaslighting();
        }

        // Public methods for external control
        public void ForceStartBossFight()
        {
            if (!isBossFightActive)
                StartBossFight();
        }

        public void ForceEndBossFight()
        {
            if (isBossFightActive)
            {
                isBossFightActive = false;
                StartCoroutine(HandleBossDefeat());
            }
        }

        public void SkipToPhase2()
        {
            if (bossController != null && isBossFightActive)
            {
                // This would need to be implemented in the boss controller
                // bossController.ForcePhase2();
                DebugLog("Skipping to Phase 2...");
            }
        }

        public void EnableDebugMode()
        {
            enableDebugOutput = true;

            // Show debug info for all systems
            if (bugManager != null)
            {
                var stats = bugManager.GetBugStats();
                DebugLog($"Bug Stats - Collision: {stats.collisionBugsTriggered}, Input: {stats.inputBugsTriggered}, Gaslight: {stats.gaslightBugsTriggered}, UI: {stats.uiBugsTriggered}");
            }

            if (exitDoor != null)
            {
                var doorInfo = exitDoor.GetDebugInfo();
                DebugLog($"Door State - Locked: {doorInfo.doorLocked}, Player Nearby: {doorInfo.playerNearby}, Boss Phase: {doorInfo.bossPhase}");
            }
        }

        // Utility methods
        void DebugLog(string message)
        {
            if (enableDebugOutput)
                Debug.Log($"[BossRoom] {message}");
        }

        public bool IsSetupComplete()
        {
            return isSetupComplete;
        }

        public bool IsBossFightActive()
        {
            return isBossFightActive;
        }

        // Method to handle player death (if applicable)
        public void OnPlayerDeath()
        {
            DebugLog("Player died! Restarting boss fight...");

            // Reset all systems
            CleanupBossRoom();

            // Restart the fight after a delay
            StartCoroutine(RestartBossFight());
        }

        IEnumerator RestartBossFight()
        {
            yield return new WaitForSeconds(2f);

            // Reinitialize systems
            InitializeAllSystems();

            // Reposition entities
            PositionEntities();

            // Restart fight
            StartBossFight();
        }

        // Scene management helpers
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && isBossFightActive)
            {
                // Pause all systems
                Time.timeScale = 0f;
                DebugLog("Boss fight paused");
            }
            else if (!pauseStatus && isBossFightActive)
            {
                // Resume all systems
                Time.timeScale = 1f;
                DebugLog("Boss fight resumed");
            }
        }

        void OnDestroy()
        {
            // Cleanup on scene unload
            CleanupBossRoom();
        }

        // Validation method for scene setup
        [System.Serializable]
        public class BossRoomValidation
        {
            public bool hasBossController;
            public bool hasBugManager;
            public bool hasUISystem;
            public bool hasAllBugSystems;
            public bool hasExitDoor;
            public bool hasPlayerSpawn;
            public bool hasBossSpawn;
            public string[] missingComponents;
        }

        public BossRoomValidation ValidateSetup()
        {
            var validation = new BossRoomValidation();
            var missing = new System.Collections.Generic.List<string>();

            validation.hasBossController = bossController != null;
            if (!validation.hasBossController) missing.Add("BossController");

            validation.hasBugManager = bugManager != null;
            if (!validation.hasBugManager) missing.Add("BugManager");

            validation.hasUISystem = uiSystem != null;
            if (!validation.hasUISystem) missing.Add("UIBetrayalSystem");

            validation.hasAllBugSystems = collisionSystem != null && inputSystem != null && gaslightingSystem != null;
            if (!validation.hasAllBugSystems)
            {
                if (collisionSystem == null) missing.Add("CollisionParadoxSystem");
                if (inputSystem == null) missing.Add("InputDesyncSystem");
                if (gaslightingSystem == null) missing.Add("GoalGaslightingSystem");
            }

            validation.hasExitDoor = exitDoor != null;
            if (!validation.hasExitDoor) missing.Add("ExitDoorController");

            validation.hasPlayerSpawn = playerSpawnPoint != null;
            if (!validation.hasPlayerSpawn) missing.Add("Player Spawn Point");

            validation.hasBossSpawn = bossSpawnPoint != null;
            if (!validation.hasBossSpawn) missing.Add("Boss Spawn Point");

            validation.missingComponents = missing.ToArray();

            return validation;
        }
    }
}