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

        private Dictionary<KeyCode, float> inputDelays = new Dictionary<KeyCode, float>();

        private SpriteRenderer spriteRenderer;
        private BugManager.BugIntensity currentIntensity = BugManager.BugIntensity.Mild;

        // Triple tap detection
        private float lastLeftTap = -1f;
        private float lastRightTap = -1f;
        private int leftTapCount = 0;
        private int rightTapCount = 0;
        private readonly float tapWindow = 0.3f;

        private bool isSwapped = false;
        public bool IsSwapped => isSwapped;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            playerController = FindObjectOfType<BossRoomPlayerController>();
            networkTimeoutIcon?.SetActive(false);
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

        public void SetPlayerController(BossRoomPlayerController player)
        {
            // Alias for SetPlayer to fix missing method error
            SetPlayer(player);
        }

        public bool ShouldProcessInputImmediately(KeyCode key)
        {
            if (!isDesyncActive)
                return true;

            if (!inputDelays.TryGetValue(key, out float readyTime))
                return true;

            return Time.time >= readyTime;
        }

        public void TriggerInputDesync(float duration)
        {
            if (isDesyncActive)
                return;

            isDesyncActive = true;
            desyncEndTime = Time.time + duration;

            inputDelays.Clear();

            KeyCode[] delayedKeys = { KeyCode.Mouse0, KeyCode.X, KeyCode.A, KeyCode.D, KeyCode.LeftArrow, KeyCode.RightArrow };
            foreach (var key in delayedKeys)
                inputDelays[key] = Time.time + Random.Range(0.3f, 1.0f);

            StartCoroutine(DesyncRoutine());
        }

        public void TriggerMinorDesync(float duration)
        {
            // Alias method to match missing method error
            TriggerInputDesync(duration);
        }

        IEnumerator DesyncRoutine()
        {
            while (Time.time < desyncEndTime)
                yield return null;

            ClearDesync();
        }

        public void ClearDesync()
        {
            isDesyncActive = false;
            inputDelays.Clear();
            networkTimeoutIcon?.SetActive(false);
        }

        public void DisablePlayerInput()
        {
            // Simple disable logic: clear desync and prevent further input
            ClearDesync();
            // Additional disabling logic could be added here if needed
        }

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

        public void SwapControlScheme()
        {
            isSwapped = true;
            Debug.Log("InputDesyncSystem: Control scheme swapped");
        }

        public void RestoreControlScheme()
        {
            isSwapped = false;
            Debug.Log("InputDesyncSystem: Control scheme restored");
        }

        public void SwapControlSchemeTemporarily(float duration)
        {
            if (!isSwapped)
            {
                SwapControlScheme();
                StartCoroutine(RestoreControlAfterDelay(duration));
            }
        }

        private IEnumerator RestoreControlAfterDelay(float duration)
        {
            yield return new WaitForSeconds(duration);
            RestoreControlScheme();
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
