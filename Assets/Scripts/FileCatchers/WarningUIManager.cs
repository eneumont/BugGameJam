using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class WarningUIManager : MonoBehaviour
{
    [SerializeField] private Transform warningContainer;  // The Grid Layout object (assign in Inspector)
    [SerializeField] private Object gameObject;  // The Grid Layout object (assign in Inspector)
    [SerializeField] private GameObject warningPrefab;    // Prefab with Image component
    [SerializeField] private Sprite warningActive;        // Active warning sprite (optional if prefab already uses it)
    [SerializeField] private string sceneToLoad;
    [SerializeField] private int maxWarnings = 3;
    [SerializeField] private int winCondition = 10;

    private List<GameObject> instantiatedWarnings = new List<GameObject>();
    private int currentWarnings = 0;
    private int currentCollectedFiles = 0;

    private void Start()
    {
        ResetWarnings();
    }

    public void AddWarning()
    {
        if (currentWarnings < maxWarnings)
        {
            currentWarnings++;
            InstantiateWarningIcon();

            if (currentWarnings >= maxWarnings) GameOver();
        }
    }
    public void AddCollected()
    {
        if (currentCollectedFiles < winCondition)
        {
            currentCollectedFiles++;

            if (currentCollectedFiles >= winCondition) WinCondition();
        }
    }
    public void ResetWarnings()
    {
        currentWarnings = 0;

        // Destroy old icons
        foreach (var go in instantiatedWarnings) Destroy(go);
        instantiatedWarnings.Clear();
    }

    private void InstantiateWarningIcon()
    {
        GameObject newWarning = Instantiate(warningPrefab, warningContainer);

        // Optional: set sprite if needed
        if (warningActive != null)
        {
            Image img = newWarning.GetComponent<Image>();
            if (img != null)
                img.sprite = warningActive;
        }

        instantiatedWarnings.Add(newWarning);
    }

    private void GameOver()
    {
        Debug.Log("Game Over — player reached max warnings.");
        SceneManager.LoadScene(sceneToLoad);
    }
    private void WinCondition() 
    {
        Debug.Log("Game Over — player collected Files.");
        GameProgress.hasCompletedPaperGame = true;
        Destroy(gameObject);
    }
}
