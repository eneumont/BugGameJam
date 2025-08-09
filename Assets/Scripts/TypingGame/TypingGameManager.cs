using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TypingGameManager : MonoBehaviour
{
    public WordSpawner wordSpawner;

    public TextMeshProUGUI typedTextDisplay;
    public TextMeshProUGUI timerDisplay;
    public TextMeshProUGUI warningsDisplay;

    public float wordTime = 30f;

    private string targetWord;
    private string typedWord = "";
    private float timer;
    private int warnings = 0;

    void Start()
    {
        timer = wordTime;
        targetWord = wordSpawner.GetTargetWord();
    }

    void Update()
    {
        HandleTyping();
        UpdateTimer();
    }

    void HandleTyping()
    {
        foreach (char raw in Input.inputString)
        {
            if (raw == '\b') // backspace
            {
                if (typedWord.Length > 0)
                {
                    typedWord = typedWord.Substring(0, typedWord.Length - 1);
                    typedTextDisplay.text = typedWord;
                }
                continue;
            }

            if (raw == '\n' || raw == '\r') continue;

            char c = char.ToLower(raw);
            if (!char.IsLetter(c)) continue;

            typedWord += c;
            typedTextDisplay.text = typedWord;

            if (typedWord == targetWord)
            {
                OnSuccess();
                return;
            }

            if (typedWord.Length >= targetWord.Length && typedWord != targetWord)
            {
                ClearTypedWithError();
                return;
            }
        }
    }

    void UpdateTimer()
    {
        timer -= Time.deltaTime;
        timerDisplay.text = Mathf.CeilToInt(timer).ToString();

        if (timer <= 0f)
        {
            OnFail();
        }
    }

    void OnSuccess()
    {
        typedWord = "";
        typedTextDisplay.text = "";
        timer = wordTime;

        wordSpawner.PickTargetBubble();
        targetWord = wordSpawner.GetTargetWord();
    }

    void OnFail()
    {
        warnings++;
        warningsDisplay.text = "Warnings: " + warnings;
        typedWord = "";
        typedTextDisplay.text = "";
        timer = wordTime;

        if (warnings >= 3)
        {
            SceneManager.LoadScene("BossYellingScene");
        }
        else
        {
            wordSpawner.PickTargetBubble();
            targetWord = wordSpawner.GetTargetWord();
        }
    }

    void ClearTypedWithError()
    {
        // Optional: play error sound or flash UI here
        typedWord = "";
        typedTextDisplay.text = "";
    }
}
