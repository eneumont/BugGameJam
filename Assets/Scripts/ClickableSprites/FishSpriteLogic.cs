using UnityEngine;

public class FishSpriteLogic : ClickableSprite
{
    [SerializeField] private ProgressMarker progressMarker;
    [SerializeField] private TalkingController talkingController;
    [SerializeField] private SpriteSwitcher spriteSwitcher;

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
        GameProgress.ResetProgress();
        progressMarker.MarkProgress();

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
