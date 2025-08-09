using UnityEngine;

public class StaplerSpriteLogic : ClickableSprite
{
    protected override void OnClick()
    {
        Debug.Log("You found stapler! +100 gold");
    }
}