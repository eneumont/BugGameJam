using UnityEngine;
using UnityEngine.EventSystems;

public class CursorUpdate : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Texture2D hoverCursor;
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    // Assign your default cursor texture here if you have one
    [SerializeField] private Texture2D defaultCursor;

    private bool isHoveringUI = false;

    private void Start()
    {
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
    }

    private void Update()
    {
        if (!isHoveringUI)
        {
            UpdateCursorForWorldObjects();
        }
    }

    // UI Pointer events
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHoveringUI = true;
        Cursor.SetCursor(hoverCursor, hotspot, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHoveringUI = false;
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
    }

    private void UpdateCursorForWorldObjects()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);

        if (hit2D.collider != null && (hit2D.collider.CompareTag("Clickable") || hit2D.collider.CompareTag("Player")))
        {
            Cursor.SetCursor(hoverCursor, hotspot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        }

    }
}
