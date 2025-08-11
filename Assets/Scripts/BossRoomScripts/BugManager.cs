using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BossRoom
{
    public class BugManager : MonoBehaviour
    {
        public enum BugIntensity { Mild, Aggressive, Paused }

        [Header("Bug Frequency Settings")]
        public float mildBugInterval = 5f;
        public float aggressiveBugInterval = 2f;

        private BugIntensity currentIntensity = BugIntensity.Mild;
        private bool bugsActive = true;
        private float lastBugTime;

        // Bug system references
        private CollisionParadoxSystem collisionSystem;
        private InputDesyncSystem inputSystem;
        private GoalGaslightingSystem gaslightingSystem;
        private UIBetrayalSystem uiSystem;

        // Bug probability weights
        [Header("Bug Probabilities")]
        [Range(0f, 1f)] public float collisionBugChance = 0.3f;
        [Range(0f, 1f)] public float inputBugChance = 0.4f;
        [Range(0f, 1f)] public float gaslightBugChance = 0.5f;
        [Range(0f, 1f)] public float uiBugChance = 0.2f; // Phase 2 only

        void Start()
        {
            InitializeBugSystems();
            lastBugTime = Time.time;
        }

        void Update()
        {
            if (!bugsActive || currentIntensity == BugIntensity.Paused)
                return;

            float bugInterval = currentIntensity == BugIntensity.Mild ? mildBugInterval : aggressiveBugInterval;

            if (Time.time - lastBugTime >= bugInterval)
            {
                TriggerRandomBug();
                lastBugTime = Time.time;
            }
        }

        void InitializeBugSystems()
        {
            collisionSystem = GetComponent<CollisionParadoxSystem>();
            if (collisionSystem == null)
                collisionSystem = FindObjectOfType<CollisionParadoxSystem>();

            inputSystem = FindObjectOfType<InputDesyncSystem>();

            gaslightingSystem = GetComponent<GoalGaslightingSystem>();
            if (gaslightingSystem == null)
                gaslightingSystem = FindObjectOfType<GoalGaslightingSystem>();

            uiSystem = FindObjectOfType<UIBetrayalSystem>();
        }

        public void SetBugIntensity(BugIntensity intensity)
        {
            currentIntensity = intensity;

            if (collisionSystem != null)
                collisionSystem.SetIntensity(intensity);
            if (inputSystem != null)
                inputSystem.SetIntensity(intensity);
            if (gaslightingSystem != null)
                gaslightingSystem.SetIntensity(intensity);
            if (uiSystem != null)
                uiSystem.SetIntensity(intensity);
        }

        void TriggerRandomBug()
        {
            List<System.Action> availableBugs = new List<System.Action>();

            if (Random.Range(0f, 1f) < collisionBugChance)
                availableBugs.Add(() => collisionSystem?.TriggerCollisionParadox());

            if (Random.Range(0f, 1f) < inputBugChance)
                availableBugs.Add(() => inputSystem?.TriggerInputDesync(Random.Range(0.3f, 1.5f)));

            if (Random.Range(0f, 1f) < gaslightBugChance)
                availableBugs.Add(() => gaslightingSystem?.TriggerGaslighting());

            if (currentIntensity == BugIntensity.Aggressive)
            {
                if (Random.Range(0f, 1f) < uiBugChance)
                    availableBugs.Add(() => uiSystem?.TriggerRandomUIBug());
            }

            if (availableBugs.Count > 0)
            {
                int randomIndex = Random.Range(0, availableBugs.Count);
                availableBugs[randomIndex].Invoke();
            }
        }

        public void PauseAllBugs()
        {
            bugsActive = false;
            currentIntensity = BugIntensity.Paused;

            collisionSystem?.ResetCollision();
            inputSystem?.ClearDesync();          // FIXED HERE
            gaslightingSystem?.ClearGaslighting();
            uiSystem?.ResetAllUI();
        }

        public void ResumeAllBugs()
        {
            bugsActive = true;
            if (currentIntensity == BugIntensity.Paused)
                currentIntensity = BugIntensity.Mild;
        }

        // Manual bug triggers for specific attacks
        public void TriggerCollisionBug(float duration = 5f)
        {
            if (collisionSystem != null)
                StartCoroutine(collisionSystem.TriggerCollisionParadoxTimed(duration));
        }

        public void TriggerInputBug(float desyncAmount, float duration = 3f)
        {
            if (inputSystem != null)
                inputSystem.TriggerInputDesync(duration); // Pass only duration, matching InputDesyncSystem's signature
        }

        public void TriggerGaslightBug(float duration = 4f)
        {
            if (gaslightingSystem != null)
                StartCoroutine(gaslightingSystem.TriggerGaslightingTimed(duration));
        }

        public void TriggerUIBug()
        {
            if (uiSystem != null)
                uiSystem.TriggerRandomUIBug();
        }

        // Debug methods
        [System.Serializable]
        public class BugStats
        {
            public int collisionBugsTriggered;
            public int inputBugsTriggered;
            public int gaslightBugsTriggered;
            public int uiBugsTriggered;
            public float totalBugTime;
        }

        private BugStats stats = new BugStats();

        public BugStats GetBugStats()
        {
            return stats;
        }

        public void IncrementBugStat(string bugType)
        {
            switch (bugType.ToLower())
            {
                case "collision":
                    stats.collisionBugsTriggered++;
                    break;
                case "input":
                    stats.inputBugsTriggered++;
                    break;
                case "gaslight":
                    stats.gaslightBugsTriggered++;
                    break;
                case "ui":
                    stats.uiBugsTriggered++;
                    break;
            }
        }
    }
}
