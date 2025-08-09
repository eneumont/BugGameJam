using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class WordBubble : MonoBehaviour
{
    public TextMeshProUGUI wordText;
    public float moveSpeed = 50f;
    public float collisionRadius = 25f;
    
    private RectTransform rt;
    private Vector2 velocity;
    private bool isTarget = false;
    private RectTransform parentArea;
    
    // Static list to track all bubbles for collision detection
    private static List<WordBubble> allBubbles = new List<WordBubble>();

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        parentArea = rt.parent as RectTransform;
        
        // Random initial velocity
        velocity = new Vector2(
            Random.Range(-moveSpeed, moveSpeed),
            Random.Range(-moveSpeed, moveSpeed)
        );
        
        allBubbles.Add(this);
    }

    void OnDestroy()
    {
        allBubbles.Remove(this);
    }

    void Update()
    {
        MoveBubble();
        CheckBoundaryCollisions();
        CheckBubbleCollisions();
    }
    
    void MoveBubble()
    {
        Vector2 currentPos = rt.anchoredPosition;
        Vector2 newPos = currentPos + velocity * Time.deltaTime;
        rt.anchoredPosition = newPos;
    }
    
    void CheckBoundaryCollisions()
    {
        if (parentArea == null) return;
        
        Vector2 pos = rt.anchoredPosition;
        Rect bounds = parentArea.rect;
        
        // Get half-extents of the bubble (assuming it's roughly square)
        float halfWidth = rt.rect.width * 0.5f;
        float halfHeight = rt.rect.height * 0.5f;
        
        // Check horizontal boundaries
        if (pos.x - halfWidth <= bounds.xMin || pos.x + halfWidth >= bounds.xMax)
        {
            velocity.x = -velocity.x;
            // Clamp position to stay within bounds
            pos.x = Mathf.Clamp(pos.x, bounds.xMin + halfWidth, bounds.xMax - halfWidth);
        }
        
        // Check vertical boundaries
        if (pos.y - halfHeight <= bounds.yMin || pos.y + halfHeight >= bounds.yMax)
        {
            velocity.y = -velocity.y;
            // Clamp position to stay within bounds
            pos.y = Mathf.Clamp(pos.y, bounds.yMin + halfHeight, bounds.yMax - halfHeight);
        }
        
        rt.anchoredPosition = pos;
    }
    
    void CheckBubbleCollisions()
    {
        Vector2 myPos = rt.anchoredPosition;
        
        foreach (WordBubble other in allBubbles)
        {
            if (other == this || other == null) continue;
            
            Vector2 otherPos = other.rt.anchoredPosition;
            Vector2 distance = myPos - otherPos;
            float distanceMag = distance.magnitude;
            
            // Check if bubbles are colliding
            if (distanceMag < collisionRadius * 2f && distanceMag > 0)
            {
                // Normalize the distance vector
                Vector2 collisionNormal = distance.normalized;
                
                // Simple elastic collision - bounce off each other
                Vector2 relativeVelocity = velocity - other.velocity;
                float speed = Vector2.Dot(relativeVelocity, collisionNormal);
                
                if (speed > 0) continue; // Objects separating, ignore
                
                // Apply collision response
                velocity -= speed * collisionNormal;
                other.velocity += speed * collisionNormal;
                
                // Separate overlapping bubbles
                float overlap = (collisionRadius * 2f - distanceMag) * 0.5f;
                Vector2 separation = collisionNormal * overlap;
                rt.anchoredPosition = myPos + separation;
                other.rt.anchoredPosition = otherPos - separation;
            }
        }
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
