using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteSwitcher : MonoBehaviour
{
    [Header("Renderer References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image uiImage;

    [Header("Sprites to cycle through")]
    [SerializeField] private List<Sprite> spritesToCycle;

    [Header("Settings")]
    [SerializeField] private float switchDelay = 0.5f; // seconds between changes

    private Coroutine switchCoroutine;

    private void Awake()
    {
        // Auto-get components if not set
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (uiImage == null)
            uiImage = GetComponent<Image>();

        if (spriteRenderer == null && uiImage == null)
            Debug.LogError("SpriteSwitcher requires at least one SpriteRenderer or UI Image assigned.");
    }

    public void StartSwitching()
    {
        if (spritesToCycle == null || spritesToCycle.Count == 0)
        {
            Debug.LogWarning("No sprites assigned to cycle.");
            return;
        }

        if (switchCoroutine != null)
            StopCoroutine(switchCoroutine);

        switchCoroutine = StartCoroutine(SwitchSpritesCoroutine());
    }

    public void StopSwitching()
    {
        if (switchCoroutine != null)
        {
            StopCoroutine(switchCoroutine);
            switchCoroutine = null;
        }
    }

    private IEnumerator SwitchSpritesCoroutine()
    {
        int currentIndex = 0;
        int direction = 1;

        while (true)
        {
            // Update both if they exist
            if (spriteRenderer != null)
                spriteRenderer.sprite = spritesToCycle[currentIndex];

            if (uiImage != null)
                uiImage.sprite = spritesToCycle[currentIndex];

            // Move index
            currentIndex += direction;

            // Reverse direction at ends (ping-pong)
            if (currentIndex >= spritesToCycle.Count)
            {
                currentIndex = spritesToCycle.Count - 2;
                direction = -1;
            }
            else if (currentIndex < 0)
            {
                currentIndex = 1;
                direction = 1;
            }

            yield return new WaitForSeconds(switchDelay); // wait before next change
        }
    }
}
