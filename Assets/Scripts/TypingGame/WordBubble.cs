using UnityEngine;
using TMPro;
using System.Collections;

public class WordBubble : MonoBehaviour
{
    public TextMeshProUGUI wordText;
    public float bobAmplitude = 10f;
    public float bobSpeed = 1f;

    private RectTransform rt;
    private Vector2 startAnchoredPos;
    private float randPhase;
    private bool isTarget = false;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        startAnchoredPos = rt.anchoredPosition;
        randPhase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * bobSpeed + randPhase) * bobAmplitude;
        rt.anchoredPosition = startAnchoredPos + Vector2.up * yOffset;
    }

    public void SetWord(string word)
    {
        if (wordText != null)
            wordText.text = word;
    }

    public void SetTarget(bool target)
    {
        if (isTarget == target) return;
        isTarget = target;
        StopAllCoroutines();
        if (target)
        {
            wordText.color = Color.yellow;
            StartCoroutine(Pulse());
        }
        else
        {
            wordText.color = Color.white;
            transform.localScale = Vector3.one;
        }
    }

    private IEnumerator Pulse()
    {
        while (isTarget)
        {
            float scale = 1f + Mathf.Sin(Time.time * 6f) * 0.08f;
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}
