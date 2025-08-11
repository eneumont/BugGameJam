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
        public bool reverseOnlyMode = false;
        public bool lookAwayMode = false;

        private Dictionary<GameObject, CollisionParadoxObject> paradoxComponents = new Dictionary<GameObject, CollisionParadoxObject>();
        private bool isParadoxActive = false;

        // Player references
        private Transform playerTransform;
        private BossRoomPlayerController playerController;
        private Rigidbody2D playerRigidbody;

        private BossController bossController;

        private BugManager.BugIntensity currentIntensity = BugManager.BugIntensity.Mild;

        public void Initialize(BossController boss, Transform player)
        {
            bossController = boss;
            SetPlayer(player);
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
            if (paradoxObjects.Count == 0)
            {
                GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
                foreach (var wall in walls)
                {
                    if (wall.name.ToLower().Contains("paradox") || wall.name.ToLower().Contains("bug"))
                    {
                        AddParadoxObject(wall);
                    }
                }

                if (paradoxObjects.Count == 0)
                {
                    Debug.LogWarning("No paradox objects found. Create walls with 'Paradox' in the name or assign manually.");
                }
            }

            foreach (var obj in paradoxObjects)
            {
                if (obj != null)
                {
                    var paradoxComp = obj.GetComponent<CollisionParadoxObject>() ?? obj.AddComponent<CollisionParadoxObject>();
                    paradoxComponents[obj] = paradoxComp;
                    paradoxComp.Initialize(this);
                }
            }
        }

        // Self-initialize if scene objects exist but Initialize() was not called
        void Start()
        {
            if (bossController == null)
            {
                bossController = FindObjectOfType<BossController>();
            }

            if (playerTransform == null)
            {
                var playerGo = GameObject.FindGameObjectWithTag("Player");
                if (playerGo != null)
                {
                    SetPlayer(playerGo.transform);
                }
            }

            // Ensure paradox objects are prepared
            InitializeParadoxObjects();
        }

        // This method now accepts optional duration for compatibility
        public void TriggerCollisionParadox(float duration = 0f)
        {
            if (bossController == null)
            {
                // Attempt auto-resolve instead of aborting
                bossController = FindObjectOfType<BossController>();
                if (bossController == null)
                {
                    Debug.LogWarning("CollisionParadoxSystem: BossController not found in scene; proceeding without direct boss reference.");
                }
            }

            if (paradoxComponents.Count == 0)
            {
                Debug.LogWarning("No paradox objects available for collision paradox!");
                return;
            }

            isParadoxActive = true;

            // Use currentIntensity set by SetIntensity (or fallback to Mild)
            switch (currentIntensity)
            {
                case BugManager.BugIntensity.Mild:
                    reverseOnlyMode = true;
                    lookAwayMode = false;
                    break;

                case BugManager.BugIntensity.Aggressive:
                    int mode = Random.Range(0, 3);
                    reverseOnlyMode = (mode == 0 || mode == 2);
                    lookAwayMode = (mode == 1 || mode == 2);
                    break;

                case BugManager.BugIntensity.Paused:
                default:
                    reverseOnlyMode = false;
                    lookAwayMode = false;
                    break;
            }

            int objectsToAffect = Mathf.Min(Random.Range(1, 4), paradoxComponents.Count);
            List<GameObject> objectsToActivate = new List<GameObject>(paradoxObjects);

            for (int i = 0; i < objectsToAffect; i++)
            {
                if (objectsToActivate.Count == 0) break;

                int randomIndex = Random.Range(0, objectsToActivate.Count);
                GameObject obj = objectsToActivate[randomIndex];
                objectsToActivate.RemoveAt(randomIndex);

                if (paradoxComponents.ContainsKey(obj))
                {
                    paradoxComponents[obj].ActivateParadox(reverseOnlyMode, lookAwayMode);
                }
            }

            FindObjectOfType<UIBetrayalSystem>()?.ShowDebugMessage(GetParadoxDescription(), 4f);

            // If a duration was provided, schedule reset after duration
            if (duration > 0f)
            {
                StartCoroutine(ResetAfterDelay(duration));
            }
        }

        IEnumerator ResetAfterDelay(float duration)
        {
            yield return new WaitForSeconds(duration);
            ResetCollision();
        }

        public IEnumerator TriggerCollisionParadoxTimed(float duration)
        {
            TriggerCollisionParadox(duration);
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
                paradoxComp?.DeactivateParadox();
            }
        }

        string GetParadoxDescription()
        {
            if (reverseOnlyMode && lookAwayMode) return "Debug: Multi-directional phase variance detected";
            if (reverseOnlyMode) return "Debug: Reverse-motion bypass enabled";
            if (lookAwayMode) return "Debug: Observer-dependent solidity active";
            return "Debug: Paradox state undefined";
        }

        public bool IsPlayerMovingBackward()
        {
            if (playerTransform == null || playerRigidbody == null || playerController == null)
                return false;

            bool facingRight = playerController.IsFacingRight();
            float velocityX = playerRigidbody.linearVelocity.x;

            return (facingRight && velocityX < -0.1f) || (!facingRight && velocityX > 0.1f);
        }

        public bool IsPlayerLookingAt(GameObject obj)
        {
            if (playerTransform == null || obj == null || playerController == null)
                return false;

            Vector2 directionToObject = (obj.transform.position - playerTransform.position).normalized;
            bool facingRight = playerController.IsFacingRight();

            return (facingRight && directionToObject.x > 0.3f) || (!facingRight && directionToObject.x < -0.3f);
        }

        public void AddParadoxObject(GameObject obj)
        {
            if (!paradoxObjects.Contains(obj))
            {
                paradoxObjects.Add(obj);
                var paradoxComp = obj.GetComponent<CollisionParadoxObject>() ?? obj.AddComponent<CollisionParadoxObject>();
                paradoxComponents[obj] = paradoxComp;
                paradoxComp.Initialize(this);
            }
        }

        public void RemoveParadoxObject(GameObject obj)
        {
            if (paradoxObjects.Remove(obj) && paradoxComponents.TryGetValue(obj, out var comp))
            {
                comp?.DeactivateParadox();
                paradoxComponents.Remove(obj);
            }
        }

        // The critical SetIntensity method to satisfy BugManager calls
        public void SetIntensity(BugManager.BugIntensity intensity)
        {
            currentIntensity = intensity;
        }
    }
}
