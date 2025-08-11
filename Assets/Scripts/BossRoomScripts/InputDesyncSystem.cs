using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace BossRoom
{
    public class InputDesyncSystem : MonoBehaviour
    {
        [Header("Desync Settings")]
        public float maxDesyncDelay = 2f;
        public float minDesyncDelay = 0.3f;

        [Header("UI Elements")]
        public GameObject networkTimeoutIcon;
        public Canvas uiCanvas;

        // Input queuing system
        private Queue<DelayedInput> inputQueue = new Queue<DelayedInput>();
        private Dictionary<KeyCode, bool> desyncedKeys = new Dictionary<KeyCode, bool>();
        private Dictionary<string, KeyCode> controlSchemeMap = new Dictionary<string, KeyCode>();
        private Dictionary<string, KeyCode> originalControlScheme = new Dictionary<string, KeyCode>();
        private bool isControlSchemeSwapped = false;

        private BugManager.BugIntensity currentIntensity = BugManager.BugIntensity.Mild;

        // Network timeout icon management
        private Coroutine timeoutIconCoroutine;

        private BossRoomPlayerController playerController;


        [System.Serializable]
        public class DelayedInput
        {
            public KeyCode key;
            public bool isKeyDown;
            public float executeTime;
            public string inputType; // "movement", "action", "jump", etc.

            public DelayedInput(KeyCode k, bool down, float time, string type = "generic")
            {
                key = k;
                isKeyDown = down;
                executeTime = time;
                inputType = type;
            }
        }

        void Start()
        {
            InitializeControlScheme();
            FindPlayerController();

            if (networkTimeoutIcon != null)
                networkTimeoutIcon.SetActive(false);
        }

        public void SetPlayer(BossRoomPlayerController playerController)
        {
            this.playerController = playerController;
        }


        void InitializeControlScheme()
        {
            // Store original control scheme
            originalControlScheme["moveLeft"] = KeyCode.A;
            originalControlScheme["moveRight"] = KeyCode.D;
            originalControlScheme["moveUp"] = KeyCode.W;
            originalControlScheme["moveDown"] = KeyCode.S;
            originalControlScheme["jump"] = KeyCode.Space;
            originalControlScheme["attack"] = KeyCode.Mouse0;
            originalControlScheme["interact"] = KeyCode.E;

            // Initialize current scheme as original
            controlSchemeMap = new Dictionary<string, KeyCode>(originalControlScheme);
        }

        void FindPlayerController()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Try to find common player controller components
                playerController = player.GetComponent<BossRoomPlayerController>();
            }
        }

        void Update()
        {
            ProcessDelayedInputs();
        }

        void ProcessDelayedInputs()
        {
            while (inputQueue.Count > 0 && inputQueue.Peek().executeTime <= Time.time)
            {
                DelayedInput delayedInput = inputQueue.Dequeue();
                ExecuteDelayedInput(delayedInput);
            }
        }

        void ExecuteDelayedInput(DelayedInput input)
        {
            // This would integrate with your actual input system
            // For now, we'll simulate the delayed execution
            Debug.Log($"Executing delayed input: {input.key} ({input.inputType}) - {(input.isKeyDown ? "Down" : "Up")}");

            // You would call your actual player controller methods here
            // Example: playerController.HandleInput(input.key, input.isKeyDown);
        }

        public void SetIntensity(BugManager.BugIntensity intensity)
        {
            currentIntensity = intensity;
        }

        public void TriggerInputDesync(float desyncAmount, float duration = -1f)
        {
            if (duration > 0)
            {
                StartCoroutine(InputDesyncTimed(desyncAmount, duration));
            }
            else
            {
                ApplyInputDesync(desyncAmount);
            }

            ShowNetworkTimeoutIcon();
        }

        IEnumerator InputDesyncTimed(float desyncAmount, float duration)
        {
            ApplyInputDesync(desyncAmount);
            yield return new WaitForSeconds(duration);
            ClearSpecificDesync();
        }

        void ApplyInputDesync(float desyncAmount)
        {
            // Randomly select which input types to desync
            List<string> inputTypes = new List<string> { "movement", "action", "jump" };
            int typesToDesync = currentIntensity == BugManager.BugIntensity.Mild ? 1 : Random.Range(1, inputTypes.Count + 1);

            for (int i = 0; i < typesToDesync; i++)
            {
                if (inputTypes.Count > 0)
                {
                    int randomIndex = Random.Range(0, inputTypes.Count);
                    string inputType = inputTypes[randomIndex];
                    inputTypes.RemoveAt(randomIndex);

                    ApplyDesyncToInputType(inputType, desyncAmount);
                }
            }
        }

        void ApplyDesyncToInputType(string inputType, float desyncAmount)
        {
            List<KeyCode> keysToDesync = new List<KeyCode>();

            switch (inputType)
            {
                case "movement":
                    keysToDesync.AddRange(new KeyCode[] {
                        controlSchemeMap["moveLeft"],
                        controlSchemeMap["moveRight"],
                        controlSchemeMap["moveUp"],
                        controlSchemeMap["moveDown"]
                    });
                    break;
                case "action":
                    keysToDesync.Add(controlSchemeMap["attack"]);
                    keysToDesync.Add(controlSchemeMap["interact"]);
                    break;
                case "jump":
                    keysToDesync.Add(controlSchemeMap["jump"]);
                    break;
            }

            foreach (KeyCode key in keysToDesync)
            {
                desyncedKeys[key] = true;
            }

            StartCoroutine(ProcessDesyncedInputs(desyncAmount));
        }

        IEnumerator ProcessDesyncedInputs(float desyncAmount)
        {
            float endTime = Time.time + desyncAmount + 2f; // Extra buffer time

            while (Time.time < endTime)
            {
                foreach (KeyCode key in desyncedKeys.Keys)
                {
                    if (desyncedKeys[key] && Input.GetKeyDown(key))
                    {
                        // Queue the input with delay
                        float delay = Random.Range(minDesyncDelay, Mathf.Min(maxDesyncDelay, desyncAmount));
                        string inputType = GetInputTypeForKey(key);

                        DelayedInput delayedInput = new DelayedInput(key, true, Time.time + delay, inputType);
                        inputQueue.Enqueue(delayedInput);
                    }

                    if (desyncedKeys[key] && Input.GetKeyUp(key))
                    {
                        float delay = Random.Range(minDesyncDelay, Mathf.Min(maxDesyncDelay, desyncAmount));
                        string inputType = GetInputTypeForKey(key);

                        DelayedInput delayedInput = new DelayedInput(key, false, Time.time + delay, inputType);
                        inputQueue.Enqueue(delayedInput);
                    }
                }

                yield return null;
            }
        }

        string GetInputTypeForKey(KeyCode key)
        {
            if (key == controlSchemeMap["moveLeft"] || key == controlSchemeMap["moveRight"] ||
                key == controlSchemeMap["moveUp"] || key == controlSchemeMap["moveDown"])
                return "movement";
            else if (key == controlSchemeMap["jump"])
                return "jump";
            else if (key == controlSchemeMap["attack"] || key == controlSchemeMap["interact"])
                return "action";

            return "generic";
        }

        void ShowNetworkTimeoutIcon()
        {
            if (timeoutIconCoroutine != null)
                StopCoroutine(timeoutIconCoroutine);

            timeoutIconCoroutine = StartCoroutine(NetworkTimeoutIconSequence());
        }

        IEnumerator NetworkTimeoutIconSequence()
        {
            if (networkTimeoutIcon != null)
            {
                networkTimeoutIcon.SetActive(true);

                // Flash the icon
                float flashDuration = 0.2f;
                int flashCount = 3;

                for (int i = 0; i < flashCount; i++)
                {
                    yield return new WaitForSeconds(flashDuration);
                    networkTimeoutIcon.SetActive(false);
                    yield return new WaitForSeconds(flashDuration);
                    networkTimeoutIcon.SetActive(true);
                }

                yield return new WaitForSeconds(1f);
                networkTimeoutIcon.SetActive(false);
            }
        }

        // Control scheme swapping for Phase 2
        public void SwapControlScheme()
        {
            if (isControlSchemeSwapped) return;

            isControlSchemeSwapped = true;

            // Common control scheme swaps that are confusing but not game-breaking
            var swapOptions = new List<System.Action>
            {
                () => {
                    // WASD to Arrow Keys
                    controlSchemeMap["moveLeft"] = KeyCode.LeftArrow;
                    controlSchemeMap["moveRight"] = KeyCode.RightArrow;
                    controlSchemeMap["moveUp"] = KeyCode.UpArrow;
                    controlSchemeMap["moveDown"] = KeyCode.DownArrow;
                },
                () => {
                    // Jump and crouch swap
                    controlSchemeMap["jump"] = KeyCode.S;
                    controlSchemeMap["moveDown"] = KeyCode.Space;
                },
                () => {
                    // Attack and interact swap
                    var temp = controlSchemeMap["attack"];
                    controlSchemeMap["attack"] = controlSchemeMap["interact"];
                    controlSchemeMap["interact"] = temp;
                }
            };

            int randomSwap = Random.Range(0, swapOptions.Count);
            swapOptions[randomSwap].Invoke();

            // Clear any existing desyncs to avoid confusion
            ClearAllDesyncs();
        }

        public void RestoreControlScheme()
        {
            if (!isControlSchemeSwapped) return;

            isControlSchemeSwapped = false;
            controlSchemeMap = new Dictionary<string, KeyCode>(originalControlScheme);
        }

        public void ClearSpecificDesync()
        {
            // Clear current desynced keys but keep control scheme if swapped
            desyncedKeys.Clear();
        }

        public void ClearAllDesyncs()
        {
            desyncedKeys.Clear();
            inputQueue.Clear();

            if (timeoutIconCoroutine != null)
            {
                StopCoroutine(timeoutIconCoroutine);
                timeoutIconCoroutine = null;
            }

            if (networkTimeoutIcon != null)
                networkTimeoutIcon.SetActive(false);
        }

        // Public methods for player controller integration
        public bool IsKeyCurrentlyDesynced(KeyCode key)
        {
            return desyncedKeys.ContainsKey(key) && desyncedKeys[key];
        }

        public KeyCode GetMappedKey(string inputName)
        {
            if (controlSchemeMap.ContainsKey(inputName))
                return controlSchemeMap[inputName];

            return KeyCode.None;
        }

        public bool IsControlSchemeSwapped()
        {
            return isControlSchemeSwapped;
        }

        // Method to be called by player controller to check if input should be processed immediately
        public bool ShouldProcessInputImmediately(KeyCode key)
        {
            return !IsKeyCurrentlyDesynced(key);
        }
    }
}