using UnityEngine;
using TMPro;
using System.Collections;

namespace BossRoom
{
    public class BossUIManager : MonoBehaviour
    {
        [Header("Health Bar")]
        public RectTransform healthFill;
        public TextMeshProUGUI healthPercentageText;

        [Header("Dialog Panel")]
        public GameObject dialogPanel;
        public TextMeshProUGUI dialogText;

        private Coroutine dialogCoroutine;

        void Start()
        {
            HideDialog();
        }

        public void UpdateHealthBar(float healthPercent, bool isHonest)
        {
            healthPercent = Mathf.Clamp01(healthPercent);
            healthFill.localScale = new Vector3(healthPercent, 1f, 1f);

            // Optionally mark dishonest health with asterisk
            string honestyMark = isHonest ? "" : "*";
            healthPercentageText.text = $"{(healthPercent * 100f):F0}%{honestyMark}";
        }

        public void ShowDialog(string message, float duration)
        {
            if (dialogCoroutine != null)
                StopCoroutine(dialogCoroutine);

            dialogCoroutine = StartCoroutine(DialogRoutine(message, duration));
        }

        IEnumerator DialogRoutine(string message, float duration)
        {
            dialogPanel.SetActive(true);
            dialogText.text = message;

            yield return new WaitForSeconds(duration);

            dialogPanel.SetActive(false);
        }

        public void HideDialog()
        {
            dialogPanel.SetActive(false);
        }
    }
}
