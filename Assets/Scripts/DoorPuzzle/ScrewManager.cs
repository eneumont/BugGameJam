using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScrewManager : MonoBehaviour
{
    [Header("Screws & Sequence")]
    [SerializeField] private List<ScrewLogic> screws;
    [SerializeField] private List<int> screwSequence; // order of screwIDs to click

    [Header("Door Settings")]
    [SerializeField] private Image doorImage;
    [SerializeField] private Sprite closedDoorSprite;
    [SerializeField] private Sprite openDoorSprite;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip correctScrewClip;

    [Header("Next Scene")]
    [SerializeField] private SceneButtonSpawner sceneButtonSpawner; // assign in Inspector
    [SerializeField] private TalkingController talkingController;

    private List<int> userInput = new List<int>();

    private void Start()
    {
        // Ensure door starts closed
        if (doorImage != null && closedDoorSprite != null)
            doorImage.sprite = closedDoorSprite;

        // Default sequence if none provided
        if (screwSequence == null || screwSequence.Count == 0)
        {
            screwSequence = new List<int>();
            foreach (var screw in screws)
                screwSequence.Add(screw.GetID());
        }
    }

    // Called whenever a screw is clicked
    public void RegisterScrewPress(int screwID)
    {
        userInput.Add(screwID);
        Debug.Log($"Click detected: {screwID}, Progress: [{string.Join(", ", userInput)}]");

        // Check the current step immediately
        int currentIndex = userInput.Count - 1;
        if (userInput[currentIndex] != screwSequence[currentIndex])
        {
            Debug.Log("? Wrong input early! Resetting...");
            ResetSequence();
            return;
        }

        // If full sequence entered correctly, open door
        if (userInput.Count == screwSequence.Count)
        {
            Debug.Log("? Correct sequence entered! Opening door...");
            OpenDoor();
        }
    }

    private void ResetSequence()
    {
        userInput.Clear();

        // Reset screw visuals
        foreach (var screw in screws)
        {
            screw.ResetStage(); // implement in ScrewLogic
        }
    }

    public void PlayCorrectScrewSound(float pitch)
    {
        if (audioSource == null || correctScrewClip == null) return;

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(correctScrewClip, 2.5f);
        audioSource.pitch = 1f;
    }

    private void OpenDoor()
    {
        if (doorImage != null && openDoorSprite != null)
            doorImage.sprite = openDoorSprite;

        ScrewCleanUp();

        if (sceneButtonSpawner != null) sceneButtonSpawner.gameObject.SetActive(true);
        if (talkingController != null) talkingController.StartText();
    }

    private void ScrewCleanUp()
    {
        foreach (var screw in screws)
            if (screw != null)
                Destroy(screw.gameObject);

        screws.Clear();
    }
}
