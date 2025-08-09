using UnityEngine;

public class FishSpriteLogic : ClickableSprite
{
    protected override void OnClick()
    {
        Debug.Log("You found fish");
    }
}