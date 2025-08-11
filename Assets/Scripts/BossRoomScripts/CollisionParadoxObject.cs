using UnityEngine;

namespace BossRoom
{
    [RequireComponent(typeof(Collider2D))]
    public class CollisionParadoxObject : MonoBehaviour
    {
        private Collider2D col;
        private CollisionParadoxSystem paradoxSystem;

        private bool paradoxActive = false;
        private bool reverseOnlyMode = false;
        private bool lookAwayMode = false;
        private BugManager.BugIntensity currentIntensity;
        private SpriteRenderer spriteRenderer;

        public void Initialize(CollisionParadoxSystem system)
        {
            paradoxSystem = system;
            col = GetComponent<Collider2D>();

            if (col == null)
                Debug.LogError($"{nameof(CollisionParadoxObject)} requires a Collider2D component.");
        }

        public void ActivateParadox(bool reverseOnly, bool lookAway)
        {
            paradoxActive = true;
            reverseOnlyMode = reverseOnly;
            lookAwayMode = lookAway;
        }

        public void DeactivateParadox()
        {
            paradoxActive = false;
            reverseOnlyMode = false;
            lookAwayMode = false;

            if (col != null)
                col.enabled = true; // Solid when paradox inactive
        }

        void Update()
        {
            if (!paradoxActive || paradoxSystem == null || col == null)
            {
                if (col != null && !col.enabled)
                    col.enabled = true; // Ensure collider is enabled if not paradox active
                return;
            }

            // Evaluate passability conditions based on current modes

            bool passableDueToReverse = false;
            bool passableDueToLookAway = false;

            if (reverseOnlyMode)
                passableDueToReverse = paradoxSystem.IsPlayerMovingBackward();

            if (lookAwayMode)
                passableDueToLookAway = !paradoxSystem.IsPlayerLookingAt(gameObject);

            bool passable;

            if (reverseOnlyMode && lookAwayMode)
            {
                // Must satisfy both to be passable
                passable = passableDueToReverse && passableDueToLookAway;
            }
            else if (reverseOnlyMode)
            {
                passable = passableDueToReverse;
            }
            else if (lookAwayMode)
            {
                passable = passableDueToLookAway;
            }
            else
            {
                // No mode active, default to solid
                passable = false;
            }

            col.enabled = !passable; // Collider enabled means solid (not passable)
        }

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void SetVisuals(Color color)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }

        public void SetIntensity(BugManager.BugIntensity intensity)
        {
            currentIntensity = intensity;

            switch (intensity)
            {
                case BugManager.BugIntensity.Mild:
                    SetVisuals(Color.white);
                    break;

                case BugManager.BugIntensity.Aggressive:
                    SetVisuals(Color.red);
                    break;
            }
        }

    }
}
