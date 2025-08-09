using System.Collections;
using UnityEngine;

public class FishSpriteLogic : ClickableSprite
{
    [SerializeField] private GameObject staplerPrefab;
    [SerializeField] private ProgressMarker progressMarker;
    [SerializeField] private Sprite yourTestSprite; // assign in inspector for testing

    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("FishSpriteLogic requires a SpriteRenderer component!");
        }
    }

    protected override void OnClick()
    {
        if (staplerPrefab != null)
        {
            progressMarker.MarkProgress();
        }
        else
        {
            Debug.LogWarning("SpawnController or StaplerPrefab is not assigned!");
        }

        ChangeSprite(yourTestSprite);
    }
    public void ChangeSprite(Sprite newSprite)
    {
        if (spriteRenderer != null && newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
        }
        else
        {
            Debug.LogWarning("Cannot change sprite: SpriteRenderer or newSprite is null.");
        }

        // Change sprite after the current click event finishes
        StartCoroutine(ChangeSpriteNextFrame(yourTestSprite));
    }

    private IEnumerator ChangeSpriteNextFrame(Sprite newSprite)
    {
        yield return null; // wait for next frame
        ChangeSprite(newSprite);
    }

}
