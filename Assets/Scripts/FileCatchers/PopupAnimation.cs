using TMPro;
using UnityEngine;

public class PopupAnimation : MonoBehaviour
{
    private float duration = 1f;
    private float speed = 50f;
    private TMP_Text[] texts;

    private void Awake()
    {
        // Get all TMP_Text components in this object and children
        texts = GetComponentsInChildren<TMP_Text>(true); // true includes inactive objects
    }

    private void Update()
    {
        // Move upward
        transform.position += Vector3.up * speed * Time.deltaTime;

        // Fade out all texts
        if (texts != null)
        {
            foreach (TMP_Text text in texts)
            {
                if (text != null)
                {
                    Color c = text.color;
                    c.a -= Time.deltaTime / duration;
                    text.color = c;
                }
            }
        }

        // Destroy after duration
        duration -= Time.deltaTime;
        if (duration <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
