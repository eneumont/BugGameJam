using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class ClickableSprite : MonoBehaviour
{
    private void OnMouseDown()
    {
        OnClick();
    }

    protected abstract void OnClick();
}
