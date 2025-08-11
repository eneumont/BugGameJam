using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BossRoom
{
    public class CollisionParadoxSystem : MonoBehaviour
    {
        [Header("Collision Objects")]
        public List<GameObject> paradoxObjects = new List<GameObject>();
        public LayerMask playerLayer = 1;

        [Header("Paradox Settings")]
        public bool reverseOnlyMode = false; // Only passable when moving backward
        public bool lookAwayMode = false; // Only passable when not looking at them

        private Dictionary<GameObject, CollisionParadoxObject> paradoxComponents = new Dictionary<GameObject, CollisionParadoxObject>();
        private BugManager.BugIntensity currentIntensity = BugManager.BugIntensity.Mild;
        private bool isParadoxActive = false;

        // Player reference
        private Transform playerTransform;
        private Rigidbody2D playerRigidbody;
        private Vector2 lastPlayerPosition;

        void Start()
        {
            InitializeParadoxObjects();
            FindPlayerReference();
        }

        public void SetPlayer(Transform playerTransform)
        {
            this.playerTransform = playerTransform;
        }


        void InitializeParadoxObjects()
        {
            foreach (GameObject obj in paradoxObjects)
            {
                if (obj != null)
                {
                    CollisionParadoxObject paradoxComp = obj.GetComponent<CollisionParadoxObject>();
                    if (paradoxComp == null)
                        paradoxComp = obj.AddComponent<CollisionParadoxObject>();

                    paradoxComponents[obj] = paradoxComp;
                    paradoxComp.Initialize(this);
                }
            }
        }

        void FindPlayerReference()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerRigidbody = player.GetComponent<Rigidbody2D>();
                lastPlayerPosition = playerTransform.position;
            }
        }

        void Update()
        {
            if (playerTransform != null)
            {
                lastPlayerPosition = playerTransform.position;
            }
        }

        public void SetIntensity(BugManager.BugIntensity intensity)
        {
            currentIntensity = intensity;

            foreach (var paradoxComp in paradoxComponents.Values)
            {
                if (paradoxComp != null)
                    paradoxComp.SetIntensity(intensity);
            }
        }

        public void TriggerCollisionParadox()
        {
            if (paradoxComponents.Count == 0) return;

            isParadoxActive = true;

            // Choose random paradox mode based on intensity
            if (currentIntensity == BugManager.BugIntensity.Mild)
            {
                // Simple reverse-only mode
                reverseOnlyMode = true;
                lookAwayMode = false;
            }
            else
            {
                // More complex modes for aggressive phase
                int mode = Random.Range(0, 3);
                reverseOnlyMode = mode == 0 || mode == 2;
                lookAwayMode = mode == 1 || mode == 2;
            }

            // Activate paradox on random objects
            int objectsToAffect = Mathf.Min(Random.Range(1, 4), paradoxComponents.Count);
            List<GameObject> objectsToActivate = new List<GameObject>(paradoxObjects);

            for (int i = 0; i < objectsToAffect; i++)
            {
                if (objectsToActivate.Count > 0)
                {
                    int randomIndex = Random.Range(0, objectsToActivate.Count);
                    GameObject obj = objectsToActivate[randomIndex];
                    objectsToActivate.RemoveAt(randomIndex);

                    if (paradoxComponents.ContainsKey(obj))
                    {
                        paradoxComponents[obj].ActivateParadox(reverseOnlyMode, lookAwayMode);
                    }
                }
            }

            // Show debug notification
            FindObjectOfType<UIBetrayalSystem>()?.ShowDebugMessage(GetParadoxDescription(), 3f);
        }

        public IEnumerator TriggerCollisionParadoxTimed(float duration)
        {
            TriggerCollisionParadox();
            yield return new WaitForSeconds(duration);
            ResetCollision();
        }

        public void ResetCollision()
        {
            isParadoxActive = false;
            reverseOnlyMode = false;
            lookAwayMode = false;

            foreach (var paradoxComp in paradoxComponents.Values)
            {
                if (paradoxComp != null)
                    paradoxComp.DeactivateParadox();
            }
        }

        string GetParadoxDescription()
        {
            if (reverseOnlyMode && lookAwayMode)
                return "Collision Debug: Multi-directional phase variance detected";
            else if (reverseOnlyMode)
                return "Collision Debug: Reverse-motion bypass enabled";
            else if (lookAwayMode)
                return "Collision Debug: Observer-dependent solidity active";
            else
                return "Collision Debug: Paradox state undefined";
        }

        public bool IsPlayerMovingBackward()
        {
            if (playerTransform == null || playerRigidbody == null)
                return false;

            Vector2 currentPos = playerTransform.position;
            Vector2 velocity = playerRigidbody.linearVelocity;

            // Consider backward if moving opposite to facing direction
            // This is a simplified check - you might need to adjust based on your player controller
            return velocity.x < -0.1f || (Mathf.Abs(velocity.x) < 0.1f && Input.GetKey(KeyCode.S));
        }

        public bool IsPlayerLookingAt(GameObject obj)
        {
            if (playerTransform == null || obj == null)
                return false;

            Vector2 directionToObject = (obj.transform.position - playerTransform.position).normalized;
            Vector2 playerFacing = playerTransform.right; // Assuming player faces right by default

            // You might need to adjust this based on your player's facing direction system
            float dot = Vector2.Dot(playerFacing, directionToObject);
            return dot > 0.5f; // Player is roughly looking at the object
        }

        // Add objects dynamically
        public void AddParadoxObject(GameObject obj)
        {
            if (!paradoxObjects.Contains(obj))
            {
                paradoxObjects.Add(obj);

                CollisionParadoxObject paradoxComp = obj.GetComponent<CollisionParadoxObject>();
                if (paradoxComp == null)
                    paradoxComp = obj.AddComponent<CollisionParadoxObject>();

                paradoxComponents[obj] = paradoxComp;
                paradoxComp.Initialize(this);
            }
        }

        public void RemoveParadoxObject(GameObject obj)
        {
            if (paradoxObjects.Contains(obj))
            {
                paradoxObjects.Remove(obj);

                if (paradoxComponents.ContainsKey(obj))
                {
                    CollisionParadoxObject comp = paradoxComponents[obj];
                    if (comp != null)
                        comp.DeactivateParadox();

                    paradoxComponents.Remove(obj);
                }
            }
        }
    }

    // Component that goes on individual paradox objects
    public class CollisionParadoxObject : MonoBehaviour
    {
        private Collider2D objectCollider;
        private CollisionParadoxSystem paradoxSystem;
        private bool isParadoxActive = false;
        private bool reverseOnlyMode = false;
        private bool lookAwayMode = false;
        private BugManager.BugIntensity intensity;

        // Visual feedback
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private bool isFlickering = false;

        public void Initialize(CollisionParadoxSystem system)
        {
            paradoxSystem = system;
            objectCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        public void SetIntensity(BugManager.BugIntensity newIntensity)
        {
            intensity = newIntensity;
        }

        public void ActivateParadox(bool reverseOnly, bool lookAway)
        {
            isParadoxActive = true;
            reverseOnlyMode = reverseOnly;
            lookAwayMode = lookAway;

            if (intensity == BugManager.BugIntensity.Aggressive)
                StartFlickering();
        }

        public void DeactivateParadox()
        {
            isParadoxActive = false;
            reverseOnlyMode = false;
            lookAwayMode = false;

            if (objectCollider != null)
                objectCollider.enabled = true;

            StopFlickering();
        }

        void Update()
        {
            if (!isParadoxActive || objectCollider == null || paradoxSystem == null)
                return;

            bool shouldBePassable = false;

            if (reverseOnlyMode && paradoxSystem.IsPlayerMovingBackward())
                shouldBePassable = true;

            if (lookAwayMode && !paradoxSystem.IsPlayerLookingAt(gameObject))
                shouldBePassable = true;

            // In combo mode, both conditions must be met
            if (reverseOnlyMode && lookAwayMode)
                shouldBePassable = paradoxSystem.IsPlayerMovingBackward() && !paradoxSystem.IsPlayerLookingAt(gameObject);

            objectCollider.enabled = !shouldBePassable;
        }

        void StartFlickering()
        {
            if (!isFlickering && spriteRenderer != null)
            {
                isFlickering = true;
                StartCoroutine(FlickerCoroutine());
            }
        }

        void StopFlickering()
        {
            isFlickering = false;
            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;
        }

        IEnumerator FlickerCoroutine()
        {
            while (isFlickering && spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(originalColor, Color.red, 0.3f);
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}