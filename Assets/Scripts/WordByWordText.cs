using System.Collections;
using UnityEngine;
using TMPro;

public class WordByWordText : MonoBehaviour
{
    [SerializeField] private TMP_Text textDisplay;
    [SerializeField] private float wordDelay = 0.3f;
    [TextArea][SerializeField] private string fullText;

    [Header("Sounds")]
    [SerializeField] private AudioClip wordSound;   // Sound for each word
    [SerializeField] private AudioClip completeSound; // Sound after text is complete
    [SerializeField] private AudioSource audioSource; // Optional — assign in Inspector

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        if (textDisplay != null)
        {
            StartCoroutine(DisplayTextOneWordAtATime());
        }
    }

    private IEnumerator DisplayTextOneWordAtATime()
    {
        textDisplay.text = "";
        string[] words = fullText.Split(' ');

        foreach (string word in words)
        {
            textDisplay.text += word + " ";

            if (wordSound != null)
                audioSource.PlayOneShot(wordSound);

            yield return new WaitForSeconds(wordDelay);
        }

        if (completeSound != null)
            audioSource.PlayOneShot(completeSound);
    }
}
