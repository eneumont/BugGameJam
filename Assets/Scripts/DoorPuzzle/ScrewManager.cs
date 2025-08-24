using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScrewManager : MonoBehaviour
{
    [Header("Screws & Sequence")]
    [SerializeField] private List<ScrewLogic> screws;
    [SerializeField] private List<int> screwSequence; // order of screwIDs to click
    private int currentIndex = 0;

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

    public bool IsCorrectScrew(int screwID)
    {
        if (currentIndex >= screwSequence.Count) return false;

        bool correct = screwID == screwSequence[currentIndex];

        string sequenceStr = string.Join(", ", screwSequence);
        Debug.Log($"Clicked: {screwID} | Expected: {screwSequence[currentIndex]} | Correct? {correct} | Sequence: [{sequenceStr}] | Current Index: {currentIndex}");

        return correct;
    }

    public void ProgressSequence()
    {
        currentIndex++;

        // Optional: clamp to avoid out-of-range
        if (currentIndex >= screwSequence.Count)
            currentIndex = screwSequence.Count; // sequence finished
    }

    public void PlayCorrectScrewSound(float pitch)
    {
        if (audioSource == null || correctScrewClip == null) return;

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(correctScrewClip, 2.5f);
        audioSource.pitch = 1f;
    }

    public void PenalizeWrongClick(ScrewLogic wrongScrew)
    {
        // Downgrade a random screw other than the one clicked
        List<ScrewLogic> others = new List<ScrewLogic>(screws);
        others.Remove(wrongScrew);

        if (others.Count > 0)
        {
            ScrewLogic randomScrew = others[Random.Range(0, others.Count)];
            randomScrew.DowngradeStage();
        }
    }

    public void CheckDoorUnlock()
    {
        foreach (var screw in screws)
            if (screw.GetStage() < 3)
                return;

        OpenDoor();
    }

    private void OpenDoor()
    {
        if (doorImage != null && openDoorSprite != null)
            doorImage.sprite = openDoorSprite;

        ScrewCleanUp();

        // Enable SceneButtonSpawner
        if (sceneButtonSpawner != null) sceneButtonSpawner.gameObject.SetActive(true);
        // Boss Talking Controller
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
