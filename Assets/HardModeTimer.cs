using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HardModeTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;           // UI Text for timer
    [SerializeField] private GameObject popupPrefab;   // Prefab for the "+2s" popup
    [SerializeField] private Transform popupParent;    // Parent for popups
    [SerializeField] private WarningUIManager warningUI; // Reference to WarningUIManager
    [SerializeField] private int resetTimeAfterWarning = 30; // Time to reset after warning

    private int currentTime = 100;
    private float elapsedTime = 0f;

    private void Start()
    {
        if (!GameProgress.HardMode)
        {
            if (timerText != null)
                timerText.gameObject.SetActive(false);
            return;
        }

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            UpdateTimerText();
        }
    }

    private void Update()
    {
        if (!GameProgress.HardMode || currentTime <= 0)
            return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= 1f)
        {
            currentTime -= 5;

            if (currentTime <= 0)
            {
                currentTime = 0;

                // Trigger warning
                if (warningUI != null)
                    warningUI.AddWarning();

                // Reset timer to partial value
                currentTime = resetTimeAfterWarning;
            }

            UpdateTimerText();
            elapsedTime -= 1f;
        }
    }

    private void UpdateTimerText()
    {
        if (timerText == null) return;

        int minutes = currentTime / 60;
        int seconds = currentTime % 60;
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void AddTime(int seconds)
    {
        if (!GameProgress.HardMode) return;

        currentTime += seconds;
        if (currentTime > 999) currentTime = 999;
        UpdateTimerText();

        ShowTimePopup(seconds);
    }

    private void ShowTimePopup(int seconds)
    {
        if (popupPrefab == null || popupParent == null) return;

        GameObject popupInstance = Instantiate(popupPrefab);
        popupInstance.transform.SetParent(popupParent, false);
        popupInstance.transform.position = timerText.transform.position;

        // Update any TMP_Text in children
        TMPro.TMP_Text[] texts = popupInstance.GetComponentsInChildren<TMPro.TMP_Text>(true);
        foreach (var txt in texts)
        {
            txt.text = $"+{seconds}s";
        }

        // Add animation
        if (popupInstance.GetComponent<PopupAnimation>() == null)
            popupInstance.AddComponent<PopupAnimation>();
    }
}
