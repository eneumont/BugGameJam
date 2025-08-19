using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Motion Toggles")]
    public bool spinOnHover = true;
    public bool floatUpDown = false;

    [Header("Spin Settings")]
    [SerializeField] private float rotationSpeed = 180f; // Degrees per second
    private bool isHovered = false;

    [Header("Float Settings")]
    [SerializeField] private float floatAmplitude = 10f; // How high it moves (pixels/units)
    [SerializeField] private float floatFrequency = 2f;  // Speed of the float
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        // Spin only when hovered
        if (spinOnHover && isHovered)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        // Float up/down continuously
        if (floatUpDown)
        {
            float yOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            transform.localPosition = startPos + new Vector3(0, yOffset, 0);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (spinOnHover)
            isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (spinOnHover)
            isHovered = false;
    }
}
