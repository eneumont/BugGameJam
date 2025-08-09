using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class WordSpawner : MonoBehaviour
{
    public GameObject wordBubblePrefab;
    public RectTransform spawnArea;
    public string[] wordList;
    public int startCount = 5;

    private List<WordBubble> spawnedBubbles = new List<WordBubble>();
    private WordBubble targetBubble;

    void Start()
    {
        SpawnAllWords();
        PickTargetBubble();
    }

    void SpawnAllWords()
    {
        for (int i = 0; i < startCount; i++)
        {
            SpawnWord(wordList[Random.Range(0, wordList.Length)]);
        }
    }

    void SpawnWord(string word)
    {
        GameObject bubbleObj = Instantiate(wordBubblePrefab, spawnArea);
        var wb = bubbleObj.GetComponent<WordBubble>();
        wb.SetWord(word);

        Vector2 randomPos = new Vector2(
            Random.Range(0f, spawnArea.rect.width) - spawnArea.rect.width / 2f,
            Random.Range(0f, spawnArea.rect.height) - spawnArea.rect.height / 2f
        );
        bubbleObj.GetComponent<RectTransform>().anchoredPosition = randomPos;

        spawnedBubbles.Add(wb);
    }

    public void PickTargetBubble()
    {
        if (targetBubble != null)
            targetBubble.SetTarget(false);

        targetBubble = spawnedBubbles[Random.Range(0, spawnedBubbles.Count)];
        targetBubble.SetTarget(true);

        Debug.Log("Target word: " + targetBubble.wordText.text);
    }

    // Optional: expose the target word text for other scripts
    public string GetTargetWord()
    {
        return targetBubble != null ? targetBubble.wordText.text : "";
    }
}
