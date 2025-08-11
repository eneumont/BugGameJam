using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace BossRoom
{
    public class UIBetrayalSystem : MonoBehaviour
    {
        [Header("UI Elements to Manipulate")]
        public RectTransform healthBarPanel;
        public RectTransform inventoryPanel;
        public RectTransform minimapPanel;
        public RectTransform actionButtonsPanel;

        [Header("Popup System")]
        public GameObject fakeDialogPrefab;
        public GameObject fakePopupPrefab;
        public Transform popupParent;
        public Canvas mainCanvas;

        [Header("Debug Messages")]
        public TextMeshProUGUI debugMessageText;
        public GameObject debugMessagePanel;

        [Header("Loading System")]
        public GameObject infiniteLoadingPanel;
        public TextMeshProUGUI loadingText;
        public Image loadingSpinner;

        [SerializeField]
        private string[] loadingMessages = new string[]
        {
            "Loading...",
            "Initializing Compliance Protocol...",
            "Connecting to Management Server...",
            "Validating Employee Status...",
            "Loading... (This may take a while)",
            "System.exe has stopped responding"
        };

        private Dictionary<RectTransform, Vector2> originalPositions = new Dictionary<RectTransform, Vector2>();
        private BugManager.BugIntensity currentIntensity = BugManager.BugIntensity.Mild;
        private bool isUIBetrayalActive = false;
        private List<GameObject> activePopups = new List<GameObject>();

        private Coroutine infiniteLoadingCoroutine;

        private readonly List<string> misleadingTooltips = new List<string>
        {
            "Press X to deal more damage",
            "Hit the glowing weak point",
            "Jump to avoid the next attack",
            "Hold Shift to charge your attack",
            "Click repeatedly for combo damage",
            "Move backwards to dodge better",
            "Press Alt to enter focus mode"
        };

        private readonly List<string> fakeErrors = new List<string>
        {
            "Error: Missing texture for boss_weakness.png",
            "Warning: Memory leak detected in combat system",
            "Notice: Combat module requires restart",
            "Error: Animation sync failed - retrying...",
            "Warning: Audio driver mismatch",
            "Notice: Updating combat balance patch 1.2.3"
        };

        void Start()
        {
            InitializeUI();
            StoreOriginalPositions();

            if (infiniteLoadingPanel != null)
                infiniteLoadingPanel.SetActive(false);
            if (debugMessagePanel != null)
                debugMessagePanel.SetActive(false);
        }

        void InitializeUI()
        {
            if (mainCanvas == null)
                mainCanvas = FindObjectOfType<Canvas>();

            if (popupParent == null && mainCanvas != null)
                popupParent = mainCanvas.transform;
        }

        void StoreOriginalPositions()
        {
            StorePosition(healthBarPanel);
            StorePosition(inventoryPanel);
            StorePosition(minimapPanel);
            StorePosition(actionButtonsPanel);
        }

        void StorePosition(RectTransform rect)
        {
            if (rect != null)
                originalPositions[rect] = rect.anchoredPosition;
        }

        public void SetIntensity(BugManager.BugIntensity intensity) => currentIntensity = intensity;

        public void StartUIBetrayal() => isUIBetrayalActive = true;

        public void TriggerRandomUIBug()
        {
            if (!isUIBetrayalActive) return;

            switch (Random.Range(0, 4))
            {
                case 0:
                    ShuffleUIElements();
                    break;
                case 1:
                    SpawnFakePopup();
                    break;
                case 2:
                    ShowMisleadingTooltip();
                    break;
                case 3:
                    ShowFakeError();
                    break;
            }
        }

        public void ShuffleUIElements() => StartCoroutine(ShuffleUICoroutine());

        IEnumerator ShuffleUICoroutine()
        {
            var uiElements = new List<RectTransform>();
            var positions = new List<Vector2>();

            foreach (var kvp in originalPositions)
            {
                if (kvp.Key != null)
                {
                    uiElements.Add(kvp.Key);
                    positions.Add(kvp.Key.anchoredPosition);
                }
            }

            if (uiElements.Count < 2)
                yield break;

            // Fisher-Yates shuffle of positions
            for (int i = positions.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var temp = positions[i];
                positions[i] = positions[j];
                positions[j] = temp;
            }

            float duration = 0.5f;
            float elapsed = 0f;

            var startPositions = new List<Vector2>();
            foreach (var elem in uiElements)
                startPositions.Add(elem.anchoredPosition);

            while (elapsed < duration)
            {
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                for (int i = 0; i < uiElements.Count; i++)
                {
                    if (uiElements[i] != null)
                        uiElements[i].anchoredPosition = Vector2.Lerp(startPositions[i], positions[i], t);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            for (int i = 0; i < uiElements.Count; i++)
            {
                if (uiElements[i] != null)
                    uiElements[i].anchoredPosition = positions[i];
            }
        }

        public void ResetUIPositions() => StartCoroutine(ResetUICoroutine());

        IEnumerator ResetUICoroutine()
        {
            float duration = 0.3f;
            float elapsed = 0f;

            var startPositions = new Dictionary<RectTransform, Vector2>();
            foreach (var kvp in originalPositions)
            {
                if (kvp.Key != null)
                    startPositions[kvp.Key] = kvp.Key.anchoredPosition;
            }

            while (elapsed < duration)
            {
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                foreach (var kvp in originalPositions)
                {
                    if (kvp.Key != null && startPositions.ContainsKey(kvp.Key))
                        kvp.Key.anchoredPosition = Vector2.Lerp(startPositions[kvp.Key], kvp.Value, t);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            foreach (var kvp in originalPositions)
            {
                if (kvp.Key != null)
                    kvp.Key.anchoredPosition = kvp.Value;
            }
        }

        public void SpawnFakePopup()
        {
            if (popupParent == null) return;

            GameObject popup = fakePopupPrefab != null
                ? Instantiate(fakePopupPrefab, popupParent)
                : CreateBasicPopup();

            if (popup != null)
            {
                activePopups.Add(popup);
                SetupPopupBehavior(popup);
            }
        }

        GameObject CreateBasicPopup()
        {
            var popup = new GameObject("FakePopup");
            popup.transform.SetParent(popupParent, false);

            popup.AddComponent<CanvasGroup>();

            var bg = popup.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            var rect = popup.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 150);
            rect.anchoredPosition = new Vector2(Random.Range(-200, 200), Random.Range(-100, 100));

            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(popup.transform, false);
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = GetRandomFakeError();
            titleText.fontSize = 14;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;

            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.6f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(10, 0);
            titleRect.offsetMax = new Vector2(-10, 0);

            var buttonObj = new GameObject("CloseButton");
            buttonObj.transform.SetParent(popup.transform, false);
            var closeButton = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);

            var buttonTextObj = new GameObject("ButtonText");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            var buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "X";
            buttonText.fontSize = 16;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;

            var buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(30, 30);
            buttonRect.anchoredPosition = new Vector2(135, 60);

            var buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;

            closeButton.onClick.AddListener(() =>
            {
                activePopups.Remove(popup);
                Destroy(popup);
            });

            return popup;
        }

        void SetupPopupBehavior(GameObject popup)
        {
            var rect = popup.GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector2[] annoyingPositions = {
                    new Vector2(0, 100),    // Center-top
                    new Vector2(-150, 0),   // Left-center
                    new Vector2(150, 0),    // Right-center
                    new Vector2(0, -50)     // Center-bottom
                };
                rect.anchoredPosition = annoyingPositions[Random.Range(0, annoyingPositions.Length)];
            }

            StartCoroutine(AutoClosePopup(popup, Random.Range(8f, 15f)));
        }

        IEnumerator AutoClosePopup(GameObject popup, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (popup != null && activePopups.Contains(popup))
            {
                activePopups.Remove(popup);
                Destroy(popup);
            }
        }

        public void ShowFakeTooltip(string message)
        {
            ShowDebugMessage(message, 4f);
        }

        public void ShowMisleadingTooltip()
        {
            if (misleadingTooltips.Count > 0)
                ShowFakeTooltip(misleadingTooltips[Random.Range(0, misleadingTooltips.Count)]);
        }

        public void ShowFakeError()
        {
            ShowFakeDialog(GetRandomFakeError(), Random.Range(3f, 6f));
        }

        string GetRandomFakeError()
        {
            return fakeErrors.Count > 0 ? fakeErrors[Random.Range(0, fakeErrors.Count)] : "System Error: Unknown issue detected";
        }

        // Add this method to fix missing call with duration parameter:
        public void TriggerGaslighting(float duration)
        {
            Debug.Log($"TriggerGaslighting triggered for {duration} seconds");
            // Implement your gaslighting effect logic here if needed.
        }

        // Add this overload to fix calls without duration:
        public void ShowFakeDialog(string message)
        {
            ShowFakeDialog(message, 4f); // default 4 seconds duration
        }

        public void ShowFakeDialog(string message, float duration)
        {
            if (popupParent == null) return;
            StartCoroutine(FakeDialogCoroutine(message, duration));
        }

        IEnumerator FakeDialogCoroutine(string message, float duration)
        {
            GameObject dialog = null;

            if (fakeDialogPrefab != null)
            {
                dialog = Instantiate(fakeDialogPrefab, popupParent);
                var dialogText = dialog.GetComponentInChildren<TextMeshProUGUI>();
                if (dialogText != null)
                    dialogText.text = message;
            }
            else
            {
                dialog = CreateBasicDialog(message);
            }

            if (dialog != null)
            {
                yield return new WaitForSeconds(duration);
                Destroy(dialog);
            }
        }

        GameObject CreateBasicDialog(string message)
        {
            var dialog = new GameObject("FakeDialog");
            dialog.transform.SetParent(popupParent, false);

            var bg = dialog.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.7f);

            var rect = dialog.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 80);
            rect.anchoredPosition = new Vector2(0, -200);

            var textObj = new GameObject("DialogText");
            textObj.transform.SetParent(dialog.transform, false);
            var dialogText = textObj.AddComponent<TextMeshProUGUI>();
            dialogText.text = message;
            dialogText.fontSize = 16;
            dialogText.color = Color.white;
            dialogText.alignment = TextAlignmentOptions.Center;

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);

            return dialog;
        }

        public void ShowDebugMessage(string message, float duration)
        {
            if (debugMessagePanel == null || debugMessageText == null)
                return;

            StartCoroutine(DebugMessageCoroutine(message, duration));
        }

        IEnumerator DebugMessageCoroutine(string message, float duration)
        {
            debugMessageText.text = message;
            debugMessagePanel.SetActive(true);

            yield return new WaitForSeconds(duration);

            debugMessagePanel.SetActive(false);
        }

        public void ShowInfiniteLoading()
        {
            if (infiniteLoadingPanel == null)
                return;

            infiniteLoadingPanel.SetActive(true);
            if (infiniteLoadingCoroutine == null)
                infiniteLoadingCoroutine = StartCoroutine(InfiniteLoadingAnimation());
        }

        public void HideInfiniteLoading()
        {
            if (infiniteLoadingPanel == null)
                return;

            infiniteLoadingPanel.SetActive(false);
            if (infiniteLoadingCoroutine != null)
            {
                StopCoroutine(infiniteLoadingCoroutine);
                infiniteLoadingCoroutine = null;
            }
        }

        IEnumerator InfiniteLoadingAnimation()
        {
            int messageIndex = 0;
            float messageChangeInterval = 2f;
            float lastMessageChange = Time.time;

            while (infiniteLoadingPanel != null && infiniteLoadingPanel.activeInHierarchy)
            {
                if (loadingText != null && Time.time - lastMessageChange >= messageChangeInterval)
                {
                    loadingText.text = loadingMessages[messageIndex];
                    messageIndex = (messageIndex + 1) % loadingMessages.Length;
                    lastMessageChange = Time.time;

                    if (messageIndex > 3)
                        messageChangeInterval = 5f;
                }

                if (loadingSpinner != null)
                    loadingSpinner.transform.Rotate(0, 0, -90 * Time.deltaTime);

                yield return null;
            }
        }

        public void ResetAllUI()
        {
            foreach (var popup in activePopups)
            {
                if (popup != null)
                    Destroy(popup);
            }

            activePopups.Clear();
            ResetUIPositions();
            HideInfiniteLoading();

            if (debugMessagePanel != null)
                debugMessagePanel.SetActive(false);

            isUIBetrayalActive = false;
        }

        public void AddMisleadingTooltip(string tooltip)
        {
            if (!misleadingTooltips.Contains(tooltip))
                misleadingTooltips.Add(tooltip);
        }

        public void AddFakeError(string error)
        {
            if (!fakeErrors.Contains(error))
                fakeErrors.Add(error);
        }

        public bool IsUIBetrayalActive() => isUIBetrayalActive;

        public int GetActivePopupCount()
        {
            activePopups.RemoveAll(p => p == null);
            return activePopups.Count;
        }
    }
}
