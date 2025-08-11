using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BossRoom
{
    public class CollisionParadoxSystem : MonoBehaviour
    {
        [Header("Collision Objects")]
        public List<GameObject> paradoxObjects = new List<GameObject>();

        [Header("Paradox Settings")]
        public bool reverseOnlyMode = false; // Only passable when moving backward
        public bool lookAwayMode = false; // Only passable when not looking at them

        private Dictionary<GameObject, CollisionParadoxObject> paradoxComponents = new Dictionary<GameObject, CollisionParadoxObject>();
        private BugManager.BugIntensity currentIntensity = BugManager.BugIntensity.Mild;
        private bool isParadoxActive = false;

        // Player reference
        private Transform playerTransform;
        private BossRoomPlayerController playerController;
        private Rigidbody2D playerRigidbody;

        void Start()
        {
            InitializeParadoxObjects();
        }

        public void SetPlayer(Transform player)
        {
            playerTransform = player;
            if (player != null)
            {
                playerController = player.GetComponent<BossRoomPlayerController>();
                playerRigidbody = player.GetComponent<Rigidbody2D>();
            }
        }

        void InitializeParadoxObjects()
        {
            // Auto-find paradox objects if none assigned
            if (paradoxObjects.Count == 0)
            {
                // Look for objects with specific tags or names
                GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
                foreach (var wall in walls)
                {
                    if (wall.name.ToLower().Contains("paradox") || wall.name.ToLower().Contains("bug"))
                    {
                        AddParadoxObject(wall);
                    }
                }

                // If still none found, create some
                if (paradoxObjects.Count == 0)
                {
                    Debug.LogWarning("No paradox objects found. Create some walls with 'Paradox' in the name or assign them manually.");
                }
            }

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
            if (paradoxComponents.Count == 0)
            {
                Debug.LogWarning("No paradox objects available for collision paradox!");
                return;
            }

            isParadoxActive = true;

            // Choose paradox mode based on intensity
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
            FindObjectOfType<UIBetrayalSystem>()?.ShowDebugMessage(GetParadoxDescription(), 4f);
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
                return "Debug: Multi-directional phase variance detected";
            else if (reverseOnlyMode)
                return "Debug: Reverse-motion bypass enabled";
            else if (lookAwayMode)
                return "Debug: Observer-dependent solidity active";
            else
                return "Debug: Paradox state undefined";
        }

        public bool IsPlayerMovingBackward()
        {
            if (playerTransform == null || playerRigidbody == null || playerController == null)
                return false;

            // Check if player is facing right but moving left, or facing left but moving right
            bool facingRight = playerController.IsFacingRight();
            float velocityX = playerRigidbody.linearVelocity.x;

            // Moving backward = moving opposite to facing direction
            if (facingRight && velocityX < -0.1f) return true; // Facing right, moving left
            if (!facingRight && velocityX > 0.1f) return true;  // Facing left, moving right

            return false;
        }

        public bool IsPlayerLookingAt(GameObject obj)
        {
            if (playerTransform == null || obj == null || playerController == null)
                return false;

            Vector2 directionToObject = (obj.transform.position - playerTransform.position).normalized;
            bool facingRight = playerController.IsFacingRight();

            // Player is looking at object if it's in the direction they're facing
            if (facingRight && directionToObject.x > 0.3f) return true;
            if (!facingRight && directionToObject.x < -0.3f) return true;

            return false;
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

}