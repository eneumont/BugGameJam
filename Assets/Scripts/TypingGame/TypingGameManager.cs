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

    // Bad word system
    private bool isBadWordActive = false;
    private float badWordEndTime = 0f;
    private float nextBadWordTime = 0f;
    private string originalTargetWord = "";
    private string[] badWords = { "poopyhead", "BHE", "quiter!", "BigMeanie", "buttsniffer", "UGLY", "mmDrugs", "fThisJob" };

    // Cursor blinking
    private bool showCursor = true;
    private float cursorBlinkTime = 0f;
    private const float CURSOR_BLINK_RATE = 0.5f;

    public string introMessage = "Begin Typing Whenever";
    private bool introActive = true;


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
        typedTextDisplay.text = introMessage;

        // Hard mode adjustments
        if (GameProgress.HardMode)
        {
            // Less time to type words
            wordTime *= 0.7f; // 30% less time
            timer = wordTime;

            // Bugs happen more frequently
            nextLagTime = Time.time + Random.Range(5f, 10f);
            nextRemapTime = Time.time + Random.Range(10f, 20f);
            nextBadWordTime = Time.time + Random.Range(12f, 25f);
        }
        else
        {
            // Normal mode timings
            nextLagTime = Time.time + Random.Range(10f, 20f);
            nextRemapTime = Time.time + Random.Range(15f, 25f);
            nextBadWordTime = Time.time + Random.Range(20f, 40f);
        }

        // Wait for WordSpawner to be ready before getting target word
        StartCoroutine(InitializeGame());
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
        // While intro is active, keep the intro message visible and don't overwrite it
        if (introActive)
            return;

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

        // --- Bad word bug (highest priority - blocks other bugs) ---
        if (isBadWordActive)
        {
            // If the bad-word period ended naturally, restore everything and clear typed input once
            if (currentTime >= badWordEndTime)
            {
                // Reset timers when bad word ends naturally
                ResetLagAndRemapTimers();

                isBadWordActive = false;

                // FULL clear (internal + UI)
                typedWord = "";
                UpdateTypedDisplay();

                // Restore original target word and UI state
                targetWord = originalTargetWord;
                wordSpawner.RestoreTargetWord(originalTargetWord);
                Debug.Log("Bad word ended! Restored to: " + targetWord);

                // Schedule next bad word
                nextBadWordTime = currentTime + Random.Range(20f, 30f);
            }

            // While a bad word is active, skip other bugs.
            return;
        }

        // Time to activate a bad word
        if (currentTime >= nextBadWordTime)
        {
            isBadWordActive = true;

            // Cancel other bugs before bad word starts
            if (isLagging || isKeyboardRemapped)
            {
                isLagging = false;
                isKeyboardRemapped = false;
            }

            // FULL clear once, when the bad word *appears*
            typedWord = "";
            UpdateTypedDisplay();

            badWordEndTime = currentTime + Random.Range(10f, 22f);
            originalTargetWord = targetWord; // store the real word
            targetWord = badWords[Random.Range(0, badWords.Length)];
            wordSpawner.SetBadWord(targetWord);
            Debug.Log($"Bad word activated: {targetWord} for {badWordEndTime - currentTime} seconds!");

            // skip other bug handling this frame
            return;
        }

        // --- Lag bug ---
        if (isLagging)
        {
            if (currentTime >= lagEndTime)
            {
                isLagging = false;
                Debug.Log("Lag ended!");
                // Schedule next lag
                nextLagTime = currentTime + Random.Range(7.126f, 11.479f);
            }
        }
        else if (currentTime >= nextLagTime)
        {
            isLagging = true;
            lagEndTime = currentTime + Random.Range(3f, 5f);
            Debug.Log("Lag started for " + (lagEndTime - currentTime) + " seconds!");
        }

        // --- Keyboard remap bug ---
        if (isKeyboardRemapped)
        {
            if (currentTime >= remapEndTime)
            {
                isKeyboardRemapped = false;
                Debug.Log("Keyboard mapping returned to normal!");
                // Schedule next remap
                nextRemapTime = currentTime + Random.Range(30f, 40f);
            }
        }
        else if (currentTime >= nextRemapTime)
        {
            isKeyboardRemapped = true;
            remapEndTime = currentTime + Random.Range(12f, 18f);
            Debug.Log("Keyboard remapped to alphabetical for " + (remapEndTime - currentTime) + " seconds!");
        }
    }

    // Add this helper method somewhere in TypingGameManager
    private void ResetLagAndRemapTimers()
    {
        isLagging = false;
        isKeyboardRemapped = false;
        lagEndTime = 0f;
        remapEndTime = 0f;
        nextLagTime = Time.time + Random.Range(4.126f, 7.479f);
        nextRemapTime = Time.time + Random.Range(20f, 30f);
    }

    void HandleTyping()
    {
        // Skip input processing if lagging or not active
        if (isLagging || !gameActive) return;

        // If intro is showing, only dismiss it when there's a printable keypress.
        // (This ignores pure backspace/enter presses so the intro isn't removed by accident.)
        if (introActive && Input.inputString.Length > 0)
        {
            bool hasPrintable = false;
            foreach (char cCheck in Input.inputString)
            {
                if (cCheck != '\b' && cCheck != '\n' && cCheck != '\r')
                {
                    hasPrintable = true;
                    break;
                }
            }

            if (hasPrintable)
            {
                introActive = false;
                typedWord = ""; // ensure no leftovers
                UpdateTypedDisplay(); // now will render typedWord + cursor
                                      // continue — the upcoming foreach will process the characters
            }
        }

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
                // Check if this was a bad word
                if (isBadWordActive)
                {
                    OnBadWordTyped();
                    return;
                }
                else
                {
                    OnSuccess();
                    return;
                }
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

            //next scene
			int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
			int nextSceneIndex = currentSceneIndex + 1;
			// Check if the next scene index is within bounds
			if (nextSceneIndex < SceneManager.sceneCountInBuildSettings) {
				SceneManager.LoadScene(nextSceneIndex);
			}
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

    void OnBadWordTyped()
    {
        warnings++;
        warningsDisplay.text = "Warnings: " + warnings;
        Debug.Log("Player typed bad word! Warning given. Total warnings: " + warnings);

        // Clear typed text and reset
        typedWord = "";
        UpdateTypedDisplay();

        // Reset timers when bad word ends by typing
        ResetLagAndRemapTimers();


        // End the bad word period immediately
        isBadWordActive = false;
        targetWord = originalTargetWord;
        wordSpawner.RestoreTargetWord(originalTargetWord);

        // Schedule next bad word for later
        nextBadWordTime = Time.time + Random.Range(20f, 60f);

        if (warnings >= 3)
        {
            StartCoroutine(TalkToBoss());
        }
    }

    void OnFail()
    {
        // Handle bad words first
        if (isBadWordActive)
        {
            // Restore original target word before completion
            targetWord = originalTargetWord;
            isBadWordActive = false;

            // Reset lag/remap timers
            ResetLagAndRemapTimers();

            // Also clear typed word
            typedWord = "";
            UpdateTypedDisplay();
        }

        // Now complete the current (restored if needed) word
        int originalIndex = wordSpawner.GetTargetWordIndex();
        completedWords[originalIndex] = targetWord;
        BuildSentenceDisplay();
        wordSpawner.RemoveTargetBubble();

        // Reset timer
        timer = wordTime;

        // Give warning
        warnings++;
        warningsDisplay.text = "Warnings: " + warnings;

        if (warnings >= 3)
        {
            StartCoroutine(TalkToBoss());
            return;
        }

        // Move to next word if any left
        if (wordSpawner.HasWordsRemaining())
        {
            wordSpawner.PickTargetBubble();
            targetWord = wordSpawner.GetTargetWord();
        }
        else
        {
            string finalSentence = string.Join(" ", completedWords);
            Debug.Log("Report completed: " + finalSentence);
            gameActive = false;
            typedTextDisplay.text = "REPORT COMPLETE!";
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

        // Load the typing game scene again rather than the boss yelling... just have players start the typing game again
        // This can be replaced with the actual boss yelling scene if needed

        //This will be the logic to just reload the TypingGame scene

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);



        //SceneManager.LoadScene("BossYellingScene");
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