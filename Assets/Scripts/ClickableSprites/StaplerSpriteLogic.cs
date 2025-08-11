using UnityEngine;

public class StaplerSpriteLogic : ClickableSprite
{
    protected override void OnClick()
    {
        Debug.Log("The Stapler Bit your Physcic hand");
    }
}