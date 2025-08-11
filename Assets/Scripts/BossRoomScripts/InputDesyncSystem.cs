using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BossRoom
{
    public class InputDesyncSystem : MonoBehaviour
    {
        [Header("UI")]
        public GameObject networkTimeoutIcon;

        private BossRoomPlayerController playerController;

        private bool isDesyncActive = false;
        private float desyncEndTime = 0f;

        // Key -> timestamp after which input is processed (delay per key)
        private Dictionary<KeyCode, float> inputDelays = new Dictionary<KeyCode, float>();

        private SpriteRenderer spriteRenderer;
        private BugManager.BugIntensity currentIntensity = BugManager.BugIntensity.Mild;

        // Triple-tap detection state
        private float lastLeftTap = -1f;
        private float lastRightTap = -1f;
        private int leftTapCount = 0;
        private int rightTapCount = 0;
        private readonly float tapWindow = 0.3f;

        // Phase 2 control swap flag (placeholder)
        private bool isSwapped = false;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            playerController = FindObjectOfType<BossRoomPlayerController>();

            if (networkTimeoutIcon != null)
                networkTimeoutIcon.SetActive(false);
        }

        void Update()
        {
            if (!isDesyncActive)
            {
                if (networkTimeoutIcon != null && networkTimeoutIcon.activeSelf)
                    networkTimeoutIcon.SetActive(false);
                return;
            }

            if (Time.time > desyncEndTime)
            {
                ClearDesync();
                return;
            }

            if (networkTimeoutIcon != null && !networkTimeoutIcon.activeSelf)
                networkTimeoutIcon.SetActive(true);
        }

        public void SetPlayer(BossRoomPlayerController player)
        {
            playerController = player;
        }

        /// <summary>
        /// Returns true if the given input key should be processed immediately (not delayed by desync)
        /// </summary>
        public bool ShouldProcessInputImmediately(KeyCode key)
        {
            if (!isDesyncActive)
                return true;

            if (!inputDelays.TryGetValue(key, out float readyTime))
                return true;

            return Time.time >= readyTime;
        }

        /// <summary>
        /// Starts input desync with random delays for specific keys lasting for the given duration
        /// </summary>
        public void TriggerInputDesync(float duration)
        {
            if (isDesyncActive)
                return;

            isDesyncActive = true;
            desyncEndTime = Time.time + duration;

            inputDelays.Clear();

            // Keys to delay; assign consistent random delay per key for desync duration
            KeyCode[] delayedKeys = { KeyCode.Mouse0, KeyCode.X, KeyCode.A, KeyCode.D, KeyCode.LeftArrow, KeyCode.RightArrow };
            foreach (var key in delayedKeys)
            {
                inputDelays[key] = Time.time + Random.Range(0.3f, 1.0f);
            }

            StartCoroutine(DesyncRoutine());
        }

        private IEnumerator DesyncRoutine()
        {
            while (Time.time < desyncEndTime)
                yield return null;

            ClearDesync();
        }

        /// <summary>
        /// Clears the input desync state immediately
        /// </summary>
        public void ClearDesync()
        {
            isDesyncActive = false;
            inputDelays.Clear();

            if (networkTimeoutIcon != null && networkTimeoutIcon.activeSelf)
                networkTimeoutIcon.SetActive(false);
        }

        /// <summary>
        /// Must be called externally on KeyDown events of left/right keys to detect triple taps.
        /// Triggers forced turning of player on triple tap.
        /// </summary>
        public void HandleDirectionalTap(bool isRight)
        {
            float currentTime = Time.time;

            if (isRight)
            {
                if (currentTime - lastRightTap > tapWindow)
                    rightTapCount = 0;

                rightTapCount++;
                lastRightTap = currentTime;

                if (rightTapCount >= 3)
                {
                    ForceTurnRight();
                    rightTapCount = 0;
                }
            }
            else
            {
                if (currentTime - lastLeftTap > tapWindow)
                    leftTapCount = 0;

                leftTapCount++;
                lastLeftTap = currentTime;

                if (leftTapCount >= 3)
                {
                    ForceTurnLeft();
                    leftTapCount = 0;
                }
            }
        }

        public void ForceTurnRight()
        {
            if (playerController != null && !playerController.IsFacingRight())
            {
                playerController.SetForcedFacing(true);
                Debug.Log("InputDesyncSystem: Forced turn right due to triple tap");
            }
        }

        public void ForceTurnLeft()
        {
            if (playerController != null && playerController.IsFacingRight())
            {
                playerController.SetForcedFacing(false);
                Debug.Log("InputDesyncSystem: Forced turn left due to triple tap");
            }
        }

        /// <summary>
        /// Placeholder for swapping control schemes in phase 2
        /// </summary>
        public void SwapControlScheme()
        {
            isSwapped = true;
            Debug.Log("InputDesyncSystem: Control scheme swapped");
            // TODO: Implement actual swap logic if needed
        }

        /// <summary>
        /// Restore controls back to default
        /// </summary>
        public void RestoreControlScheme()
        {
            isSwapped = false;
            Debug.Log("InputDesyncSystem: Control scheme restored");
        }

        private void SetVisuals(Color color)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = color;
        }

        public void SetIntensity(BugManager.BugIntensity intensity)
        {
            currentIntensity = intensity;

            switch (intensity)
            {
                case BugManager.BugIntensity.Mild:
                    SetVisuals(Color.white);
                    break;
                case BugManager.BugIntensity.Aggressive:
                    SetVisuals(Color.red);
                    break;
            }
        }
    }
}
