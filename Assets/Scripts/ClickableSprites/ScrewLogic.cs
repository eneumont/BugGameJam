using UnityEngine;
using UnityEngine.EventSystems;

public class ScrewLogic : MonoBehaviour, IPointerClickHandler
{
    [Header("Screw Settings")]
    [SerializeField] private int screwID;
    [SerializeField] private int stage = 0;
    [SerializeField] private float rotationPerClick = 15f;
    [SerializeField] private Vector3 stageScaleStep = new Vector3(0.3f, 0.3f, 0f);

    [Header("Hand & UI")]
    [SerializeField] private Transform handTip; // fingertip transform
    [SerializeField] private Camera uiCamera;   // assign your Canvas camera

    [Header("Audio")]
    [SerializeField] private float screwPitch = 1f; // unique pitch for this screw

    private ScrewManager manager;
    private RectTransform rectTransform;

    private void Start()
    {
        manager = FindFirstObjectByType<ScrewManager>();
        rectTransform = GetComponent<RectTransform>();
        transform.localScale = Vector3.one;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager == null || handTip == null) return;

        // Convert handTip position to screen space
        Vector2 handScreenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, handTip.position);

        // Check if hand is over this screw
        bool isHandOver = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, handScreenPos, uiCamera);
        Debug.Log($"OnPointerClick for ScrewID {screwID}. Hand over screw? {isHandOver}");

        if (!isHandOver) return;

        // Send screw press to manager
        manager.RegisterScrewPress(screwID);

        // Play pitched sound (feedback for click, regardless of correctness)
        manager.PlayCorrectScrewSound(screwPitch);

        // Rotate + stage visuals
        AdvanceStage();
        transform.Rotate(Vector3.forward, rotationPerClick);
    }

    public void AdvanceStage()
    {
        if (stage < 3)
        {
            stage++;
            ApplyScale();
        }
    }

    public void DowngradeStage()
    {
        if (stage > 0)
        {
            stage--;
            ApplyScale();
        }
    }

    private void ApplyScale()
    {
        transform.localScale = Vector3.one + (stageScaleStep * stage);
    }

    public int GetStage() => stage;
    public int GetID() => screwID;

    // Reset stage (for sequence reset)
    public void ResetStage()
    {
        stage = 0;
        ApplyScale();
        transform.rotation = Quaternion.identity;
    }
}
