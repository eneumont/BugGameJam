using UnityEngine;

public class ComputerClickable : ClickableSprite
{
	protected override void OnClick()
	{
		Debug.Log("You found a computer");
	}
}
