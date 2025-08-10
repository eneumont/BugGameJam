using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class WordSpawner : MonoBehaviour
{
    public GameObject wordBubblePrefab;
    public RectTransform spawnArea;
    public string[] wordList;

    private List<WordBubble> spawnedBubbles = new List<WordBubble>();
    private WordBubble targetBubble;

    void Start()
    {
        SpawnAllWords();
        // Don't pick target here - let TypingGameManager do it after initialization
    }

    void SpawnAllWords()
    {
        // Spawn each word from the list exactly once
        for (int i = 0; i < wordList.Length; i++)
        {
            SpawnWord(wordList[i], i);
        }
    }

    void SpawnWord(string word, int originalIndex)
    {
        GameObject bubbleObj = Instantiate(wordBubblePrefab, spawnArea);
        var wb = bubbleObj.GetComponent<WordBubble>();
        wb.SetWord(word);
        wb.SetOriginalIndex(originalIndex); // Set the original index for ordering

        Vector2 randomPos = new Vector2(
            Random.Range(0f, spawnArea.rect.width) - spawnArea.rect.width / 2f,
            Random.Range(0f, spawnArea.rect.height) - spawnArea.rect.height / 2f
        );
        bubbleObj.GetComponent<RectTransform>().anchoredPosition = randomPos;

        spawnedBubbles.Add(wb);
    }

    public void PickTargetBubble()
    {
        if (spawnedBubbles.Count == 0) return;

        if (targetBubble != null)
            targetBubble.SetTarget(false);

        targetBubble = spawnedBubbles[Random.Range(0, spawnedBubbles.Count)];
        targetBubble.SetTarget(true);

        Debug.Log("Target word: " + targetBubble.wordText.text);
    }

    // New method to pick target bubble by original word list index
    public void PickTargetBubbleByIndex(int wordIndex)
    {
        if (spawnedBubbles.Count == 0) return;

        if (targetBubble != null)
            targetBubble.SetTarget(false);

        // Find the bubble with the matching original index
        foreach (WordBubble bubble in spawnedBubbles)
        {
            if (bubble.GetOriginalIndex() == wordIndex)
            {
                targetBubble = bubble;
                targetBubble.SetTarget(true);
                Debug.Log("Target word (index " + wordIndex + "): " + targetBubble.wordText.text);
                return;
            }
        }

        // Fallback: if we can't find the specific index, just pick the first available
        if (spawnedBubbles.Count > 0)
        {
            targetBubble = spawnedBubbles[0];
            targetBubble.SetTarget(true);
            Debug.Log("Fallback target word: " + targetBubble.wordText.text);
        }
    }

    // Optional: expose the target word text for other scripts
    public string GetTargetWord()
    {
        return targetBubble != null ? targetBubble.wordText.text : "";
    }

    // Get the original index of the current target word
    public int GetTargetWordIndex()
    {
        return targetBubble != null ? targetBubble.GetOriginalIndex() : -1;
    }

    // Get total number of words in the list
    public int GetTotalWordCount()
    {
        return wordList.Length;
    }

    public void RemoveTargetBubble()
    {
        if (targetBubble != null)
        {
            spawnedBubbles.Remove(targetBubble);
            Destroy(targetBubble.gameObject);
            targetBubble = null;
        }
    }

    public bool HasWordsRemaining()
    {
        return spawnedBubbles.Count > 0;
    }
}