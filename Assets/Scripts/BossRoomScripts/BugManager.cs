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

        private BugIntensity currentIntensityEnum = BugIntensity.Mild; // Enum state
        private float bugIntensityValue = 0f; // Numeric intensity: 0 = Mild, 1 = Aggressive

        public BugIntensity intensity => currentIntensityEnum;
        public float intensityValue => bugIntensityValue;


        private bool bugsActive = true;
        private float lastBugTime;
        private float currentBugInterval;

        // Bug system references - cached at start
        private CollisionParadoxSystem collisionSystem;
        private InputDesyncSystem inputSystem;
        private GoalGaslightingSystem gaslightingSystem;
        private UIBetrayalSystem uiSystem;

        [Header("Bug Probabilities")]
        [Range(0f, 1f)] public float collisionBugChance = 0.3f;
        [Range(0f, 1f)] public float inputBugChance = 0.4f;
        [Range(0f, 1f)] public float gaslightBugChance = 0.5f;
        [Range(0f, 1f)] public float uiBugChance = 0.2f; // Phase 2 only

        private BugStats stats = new BugStats();

        void Start()
        {
            SetIntensityFromFloat(0f); // Initialize intensity to Mild
            CacheSystems();
            lastBugTime = Time.time;
        }

        void CacheSystems()
        {
            collisionSystem = GetComponent<CollisionParadoxSystem>() ?? FindObjectOfType<CollisionParadoxSystem>();
            inputSystem = GetComponent<InputDesyncSystem>() ?? FindObjectOfType<InputDesyncSystem>();
            gaslightingSystem = GetComponent<GoalGaslightingSystem>() ?? FindObjectOfType<GoalGaslightingSystem>();
            uiSystem = FindObjectOfType<UIBetrayalSystem>();
        }

        void Update()
        {
            if (!bugsActive || currentIntensityEnum == BugIntensity.Paused)
                return;

            if (Time.time - lastBugTime >= currentBugInterval)
            {
                TriggerRandomBug();
                lastBugTime = Time.time;
            }
        }

        public void IncreaseIntensity(float delta)
        {
            if (currentIntensityEnum == BugIntensity.Paused) return;

            float newIntensity = Mathf.Clamp01(bugIntensityValue + delta);
            SetIntensityFromFloat(newIntensity);
        }

        void SetIntensityFromFloat(float intensity)
        {
            bugIntensityValue = Mathf.Clamp01(intensity);

            if (bugIntensityValue <= 0f)
                currentIntensityEnum = BugIntensity.Mild;
            else if (bugIntensityValue >= 1f)
                currentIntensityEnum = BugIntensity.Aggressive;
            else
                currentIntensityEnum = BugIntensity.Mild; // Or add mid-level if you want

            currentBugInterval = Mathf.Lerp(mildBugInterval, aggressiveBugInterval, bugIntensityValue);

            SetIntensityToSystems(currentIntensityEnum);
        }

        public void SetBugIntensity(BugIntensity intensity)
        {
            currentIntensityEnum = intensity;

            bugIntensityValue = (intensity == BugIntensity.Mild) ? 0f : (intensity == BugIntensity.Aggressive) ? 1f : bugIntensityValue;

            currentBugInterval = Mathf.Lerp(mildBugInterval, aggressiveBugInterval, bugIntensityValue);

            SetIntensityToSystems(currentIntensityEnum);
        }

        void SetIntensityToSystems(BugIntensity intensity)
        {
            collisionSystem?.SetIntensity(intensity);
            inputSystem?.SetIntensity(intensity);
            gaslightingSystem?.SetIntensity(intensity);
            uiSystem?.SetIntensity(intensity);
        }

        void TriggerRandomBug()
        {
            var availableBugs = new List<System.Action>();

            if (Random.value < collisionBugChance)
                availableBugs.Add(() => { collisionSystem?.TriggerCollisionParadox(); IncrementBugStat("collision"); });

            if (Random.value < inputBugChance)
                availableBugs.Add(() => { inputSystem?.TriggerInputDesync(Random.Range(0.3f, 1.5f)); IncrementBugStat("input"); });

            if (Random.value < gaslightBugChance)
                availableBugs.Add(() => { gaslightingSystem?.TriggerGaslighting(); IncrementBugStat("gaslight"); });

            if (currentIntensityEnum == BugIntensity.Aggressive && Random.value < uiBugChance)
                availableBugs.Add(() => { uiSystem?.TriggerRandomUIBug(); IncrementBugStat("ui"); });

            if (availableBugs.Count == 0)
                return;

            int index = Random.Range(0, availableBugs.Count);
            availableBugs[index].Invoke();
        }

        public void PauseAllBugs()
        {
            bugsActive = false;
            currentIntensityEnum = BugIntensity.Paused;

            collisionSystem?.ResetCollision();
            inputSystem?.ClearDesync();
            gaslightingSystem?.ClearGaslighting();
            uiSystem?.ResetAllUI();
        }

        public void ResumeAllBugs()
        {
            bugsActive = true;
            if (currentIntensityEnum == BugIntensity.Paused)
                SetBugIntensity(BugIntensity.Mild);
        }

        // Manual triggers for external calls
        public void TriggerCollisionBug(float duration = 5f)
        {
            if (collisionSystem != null)
                StartCoroutine(collisionSystem.TriggerCollisionParadoxTimed(duration));
        }

        public void TriggerInputBug(float desyncAmount, float duration = 3f)
        {
            inputSystem?.TriggerInputDesync(duration); // only duration needed
        }

        public void TriggerGaslightBug(float duration = 4f)
        {
            if (gaslightingSystem != null)
                StartCoroutine(gaslightingSystem.TriggerGaslightingTimed(duration));
        }

        public void TriggerUIBug()
        {
            uiSystem?.TriggerRandomUIBug();
        }

        public BugStats GetBugStats() => stats;

        void IncrementBugStat(string bugType)
        {
            switch (bugType.ToLower())
            {
                case "collision": stats.collisionBugsTriggered++; break;
                case "input": stats.inputBugsTriggered++; break;
                case "gaslight": stats.gaslightBugsTriggered++; break;
                case "ui": stats.uiBugsTriggered++; break;
            }
        }

        [System.Serializable]
        public class BugStats
        {
            public int collisionBugsTriggered;
            public int inputBugsTriggered;
            public int gaslightBugsTriggered;
            public int uiBugsTriggered;
            public float totalBugTime;
        }
    }
}
