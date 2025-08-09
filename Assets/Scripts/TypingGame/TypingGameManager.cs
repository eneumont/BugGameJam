using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TypingGameManager : MonoBehaviour
{
    public WordSpawner wordSpawner;

    public TextMeshProUGUI typedTextDisplay;
    public TextMeshProUGUI timerDisplay;
    public TextMeshProUGUI warningsDisplay;
    public TextMeshProUGUI finalTypedSentence;

    public float wordTime = 30f;

    private string targetWord;
    private string typedWord = "";
    private float timer;
    private int warnings = 0;
    private string builtSentence = "";
    
    // Bug system variables
    private bool isLagging = false;
    private float lagEndTime = 0f;
    private bool isKeyboardRemapped = false;
    private float remapEndTime = 0f;
    private float nextLagTime = 0f;
    private float nextRemapTime = 0f;
    
    // Keyboard mapping for alphabetical bug (QWERTY to alphabetical)
    private Dictionary<char, char> qwertyToAlpha = new Dictionary<char, char>()
    {
        {'q', 'a'}, {'w', 'b'}, {'e', 'c'}, {'r', 'd'}, {'t', 'e'}, {'y', 'f'}, {'u', 'g'}, {'i', 'h'}, {'o', 'i'}, {'p', 'j'},
        {'a', 'k'}, {'s', 'l'}, {'d', 'm'}, {'f', 'n'}, {'g', 'o'}, {'h', 'p'}, {'j', 'q'}, {'k', 'r'}, {'l', 's'},
        {'z', 't'}, {'x', 'u'}, {'c', 'v'}, {'v', 'w'}, {'b', 'x'}, {'n', 'y'}, {'m', 'z'},
        // Uppercase versions
        {'Q', 'A'}, {'W', 'B'}, {'E', 'C'}, {'R', 'D'}, {'T', 'E'}, {'Y', 'F'}, {'U', 'G'}, {'I', 'H'}, {'O', 'I'}, {'P', 'J'},
        {'A', 'K'}, {'S', 'L'}, {'D', 'M'}, {'F', 'N'}, {'G', 'O'}, {'H', 'P'}, {'J', 'Q'}, {'K', 'R'}, {'L', 'S'},
        {'Z', 'T'}, {'X', 'U'}, {'C', 'V'}, {'V', 'W'}, {'B', 'X'}, {'N', 'Y'}, {'M', 'Z'}
    };

    void Start()
    {
        timer = wordTime;
        targetWord = wordSpawner.GetTargetWord();
        
        // Initialize bug timers
        nextLagTime = Time.time + Random.Range(10f, 20f);
        nextRemapTime = Time.time + Random.Range(15f, 25f);
    }

    void Update()
    {
        UpdateBugSystem();
        HandleTyping();
        UpdateTimer();
    }

    void UpdateBugSystem()
    {
        float currentTime = Time.time;
        
        // Handle lag bug
        if (isLagging)
        {
            if (currentTime >= lagEndTime)
            {
                isLagging = false;
                Debug.Log("Lag ended!");
                // Schedule next lag
                nextLagTime = currentTime + Random.Range(10f, 30f);
            }
        }
        else if (currentTime >= nextLagTime)
        {
            isLagging = true;
            lagEndTime = currentTime + Random.Range(0.5f, 2f);
            Debug.Log("Lag started for " + (lagEndTime - currentTime) + " seconds!");
        }
        
        // Handle keyboard remap bug
        if (isKeyboardRemapped)
        {
            if (currentTime >= remapEndTime)
            {
                isKeyboardRemapped = false;
                Debug.Log("Keyboard mapping returned to normal!");
                // Schedule next remap
                nextRemapTime = currentTime + Random.Range(15f, 45f);
            }
        }
        else if (currentTime >= nextRemapTime)
        {
            isKeyboardRemapped = true;
            remapEndTime = currentTime + Random.Range(10f, 15f);
            Debug.Log("Keyboard remapped to alphabetical for " + (remapEndTime - currentTime) + " seconds!");
        }
    }

    void HandleTyping()
    {
        // Skip input processing if lagging
        if (isLagging) return;
        
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

            // Allow letters, numbers, and common punctuation
            if (!char.IsLetterOrDigit(raw) && !"!@#$%^&*()-_=+[]{}|;:'\",.<>?/`~".Contains(raw)) continue;

            char processedChar = raw;
            
            // Apply keyboard remapping bug if active
            if (isKeyboardRemapped && qwertyToAlpha.ContainsKey(raw))
            {
                processedChar = qwertyToAlpha[raw];
            }

            typedWord += processedChar; // Keep original case for display
            typedTextDisplay.text = typedWord;

            // Compare case-insensitively
            if (string.Equals(typedWord, targetWord, System.StringComparison.OrdinalIgnoreCase))
            {
                OnSuccess();
                return;
            }

            if (typedWord.Length >= targetWord.Length && !string.Equals(typedWord, targetWord, System.StringComparison.OrdinalIgnoreCase))
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
        // Add the completed word to the final sentence
        if (!string.IsNullOrEmpty(builtSentence))
            builtSentence += " ";
        builtSentence += targetWord;
        finalTypedSentence.text = builtSentence;
        
        // Remove the current target bubble
        wordSpawner.RemoveTargetBubble();
        
        typedWord = "";
        typedTextDisplay.text = "";
        timer = wordTime;

        // Check if there are more words to type
        if (wordSpawner.HasWordsRemaining())
        {
            wordSpawner.PickTargetBubble();
            targetWord = wordSpawner.GetTargetWord();
        }
        else
        {
            // All words completed!
            Debug.Log("Report completed: " + builtSentence);
            // You can add completion logic here (scene transition, victory screen, etc.)
        }
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
