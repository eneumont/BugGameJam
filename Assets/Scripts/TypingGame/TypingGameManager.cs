using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

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
    private string[] completedWords; // Array to store completed words in correct order
    private int totalWords; // Total number of words in the list

    // Game state
    private bool gameActive = true;
    private bool talkingToBoss = false;

    // Bug system variables
    private bool isLagging = false;
    private float lagEndTime = 0f;
    private bool isKeyboardRemapped = false;
    private float remapEndTime = 0f;
    private float nextLagTime = 0f;
    private float nextRemapTime = 0f;

    // Cursor blinking
    private bool showCursor = true;
    private float cursorBlinkTime = 0f;
    private const float CURSOR_BLINK_RATE = 0.5f;

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
        // Wait for WordSpawner to be ready before getting target word
        StartCoroutine(InitializeGame());

        // Initialize bug timers
        nextLagTime = Time.time + Random.Range(10f, 20f);
        nextRemapTime = Time.time + Random.Range(15f, 25f);
    }

    IEnumerator InitializeGame()
    {
        // Wait one frame to ensure WordSpawner has finished setup
        yield return null;

        // Initialize the completed words array
        totalWords = wordSpawner.GetTotalWordCount();
        completedWords = new string[totalWords];

        // Now get the first target word (random selection)
        wordSpawner.PickTargetBubble();
        targetWord = wordSpawner.GetTargetWord();
        Debug.Log("First target word: " + targetWord);
    }

    void Update()
    {
        if (!gameActive) return;

        UpdateBugSystem();
        HandleTyping();
        UpdateTimer();
        UpdateCursorBlink();
    }

    void UpdateCursorBlink()
    {
        cursorBlinkTime += Time.deltaTime;
        if (cursorBlinkTime >= CURSOR_BLINK_RATE)
        {
            showCursor = !showCursor;
            cursorBlinkTime = 0f;
            UpdateTypedDisplay();
        }
    }

    void UpdateTypedDisplay()
    {
        string displayText = typedWord;
        if (showCursor && gameActive && !isLagging)
        {
            displayText += "|";
        }
        typedTextDisplay.text = displayText;
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
        // Skip input processing if lagging or not active
        if (isLagging || !gameActive) return;

        foreach (char raw in Input.inputString)
        {
            if (raw == '\b') // backspace
            {
                if (typedWord.Length > 0)
                {
                    typedWord = typedWord.Substring(0, typedWord.Length - 1);
                    UpdateTypedDisplay();
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
            UpdateTypedDisplay();

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
        if (!gameActive || talkingToBoss) return; // Pause timer when talking to boss

        timer -= Time.deltaTime;
        timerDisplay.text = Mathf.CeilToInt(timer).ToString();

        if (timer <= 0f)
        {
            OnFail();
        }
    }

    void OnSuccess()
    {
        // Get the original index of the completed word
        int originalIndex = wordSpawner.GetTargetWordIndex();

        // Place the completed word in the correct position
        completedWords[originalIndex] = targetWord;

        // Build the sentence with completed words in order, showing gaps
        BuildSentenceDisplay();

        // Remove the current target bubble
        wordSpawner.RemoveTargetBubble();

        typedWord = "";
        UpdateTypedDisplay();
        timer = wordTime;

        // Check if there are more words to type
        if (wordSpawner.HasWordsRemaining())
        {
            wordSpawner.PickTargetBubble(); // Random selection
            targetWord = wordSpawner.GetTargetWord();
        }
        else
        {
            // All words completed!
            string finalSentence = string.Join(" ", completedWords);
            Debug.Log("Report completed: " + finalSentence);
            gameActive = false;
            typedTextDisplay.text = "REPORT COMPLETE!";
            // You can add completion logic here (scene transition, victory screen, etc.)
        }
    }

    void BuildSentenceDisplay()
    {
        string displaySentence = "";
        for (int i = 0; i < completedWords.Length; i++)
        {
            if (i > 0) displaySentence += " ";

            if (!string.IsNullOrEmpty(completedWords[i]))
            {
                displaySentence += completedWords[i];
            }
            else
            {
                displaySentence += "_____"; // Show gaps for incomplete words
            }
        }
        finalTypedSentence.text = displaySentence;
    }

    void OnFail()
    {
        warnings++;
        warningsDisplay.text = "Warnings: " + warnings;
        typedWord = "";
        UpdateTypedDisplay();
        timer = wordTime;

        if (warnings >= 3)
        {
            StartCoroutine(TalkToBoss());
        }
        else
        {
            // Stay on a random word - they need to complete one
            wordSpawner.PickTargetBubble();
            targetWord = wordSpawner.GetTargetWord();
        }
    }

    IEnumerator TalkToBoss()
    {
        talkingToBoss = true;
        gameActive = false;

        // Hide cursor and clear typed text
        typedTextDisplay.text = "Talking to boss...";

        // Wait a moment before transitioning
        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene("BossYellingScene");
    }

    void ClearTypedWithError()
    {
        // Optional: play error sound or flash UI here
        typedWord = "";
        UpdateTypedDisplay();
    }

    // Public method to resume game after boss conversation (if needed)
    public void ResumeGame()
    {
        talkingToBoss = false;
        gameActive = true;
        warnings = 0; // Reset warnings
        warningsDisplay.text = "Warnings: " + warnings;
    }
}