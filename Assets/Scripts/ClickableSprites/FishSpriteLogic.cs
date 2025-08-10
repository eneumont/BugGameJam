using UnityEngine;

public class FishSpriteLogic : ClickableSprite
{
    [SerializeField] private ProgressMarker progressMarker;
    [SerializeField] private TalkingController talkingController;
    [SerializeField] private SpriteSwitcher spriteSwitcher;

    private bool hasInteracted = false; // Local check to prevent re-triggering

    private void OnEnable()
    {
        if (talkingController != null)
            talkingController.OnTextEnded += OnDialogEnd;
    }

    private void OnDisable()
    {
        if (talkingController != null)
            talkingController.OnTextEnded -= OnDialogEnd;
    }

    protected override void OnClick()
    {
        // Prevent running if already interacted
        if (hasInteracted)
            return;

        hasInteracted = true;

        GameProgress.ResetProgress();
        progressMarker.MarkProgress();
        Debug.Log(GameProgress.hasProgressed);

        if (spriteSwitcher != null)
            spriteSwitcher.StartSwitching();

        talkingController.StartText();
    }

    private void OnDialogEnd()
    {
        if (spriteSwitcher != null)
            spriteSwitcher.StopSwitching();
    }
}
