using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace BossRoom
{
    public class GoalGaslightingSystem : MonoBehaviour
    {
        [Header("UI References")]
        public RectTransform bossHealthFill;
        public TextMeshProUGUI bossHealthPercentageText;
        public List<UnityEngine.UI.Image> playerHearts = new List<UnityEngine.UI.Image>();
        public Sprite fullHeartSprite;
        public Sprite emptyHeartSprite;

        [Header("Hit Markers")]
        public List<GameObject> hitMarkers = new List<GameObject>();

        [Header("Gaslighting Settings")]
        [Range(0f, 1f)] public float fakeDamageChance = 0.3f;
        [Range(0f, 1f)] public float preventRealDamageChance = 0.2f;
        [Range(0f, 1f)] public float fakeHealChance = 0.4f;
        [Range(0f, 1f)] public float wrongHitMarkerChance = 0.5f;

        private BugManager.BugIntensity currentIntensity = BugManager.BugIntensity.Mild;
        private bool isGaslightingActive = false;
        private BossController bossController;
        private GameObject player;

        private float displayedBossHealth = 1f;
        private bool isShowingFakeHealth = false;
        private int correctHitMarkerIndex = 0;

        private UnityEngine.UI.Image bossHealthImage;

        void Start()
        {
            InitializeSystem();

            if (hitMarkers.Count == 0)
                CreateHitMarkers();
        }

        void InitializeSystem()
        {
            bossController = FindObjectOfType<BossController>();

            if (bossController != null)
            {
                displayedBossHealth = bossController.GetHealthPercentage();
                UpdateBossHealthUI(displayedBossHealth);
            }

            if (bossHealthFill == null)
            {
                var healthBar = GameObject.Find("BossHealthBar");
                if (healthBar != null)
                    bossHealthFill = healthBar.GetComponent<RectTransform>();
            }

            if (bossHealthPercentageText == null)
            {
                var healthText = GameObject.Find("BossHealthText");
                if (healthText != null)
                    bossHealthPercentageText = healthText.GetComponent<TextMeshProUGUI>();
            }

            if (bossHealthFill != null)
                bossHealthImage = bossHealthFill.GetComponent<UnityEngine.UI.Image>();

            if (playerHearts.Count == 0)
            {
                var hearts = GameObject.FindGameObjectsWithTag("PlayerHeart");
                foreach (var heart in hearts)
                {
                    var img = heart.GetComponent<UnityEngine.UI.Image>();
                    if (img != null)
                        playerHearts.Add(img);
                }
            }

            if (hitMarkers.Count > 0)
                correctHitMarkerIndex = Random.Range(0, hitMarkers.Count);
        }

        void CreateHitMarkers()
        {
            var boss = FindObjectOfType<BossController>();
            if (boss == null) return;

            Vector3 bossPos = boss.transform.position;
            Vector3[] positions = {
                bossPos + new Vector3(-2, 1, 0),
                bossPos + new Vector3(2, 1, 0),
                bossPos + new Vector3(0, 2, 0),
                bossPos + new Vector3(0, -1, 0)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject marker = new GameObject($"HitMarker_{i}");
                marker.transform.position = positions[i];

                var sr = marker.AddComponent<SpriteRenderer>();

                Texture2D texture = new Texture2D(32, 32);
                for (int x = 0; x < 32; x++)
                {
                    for (int y = 0; y < 32; y++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                        if (dist < 12 && dist > 8)
                            texture.SetPixel(x, y, Color.yellow);
                        else
                            texture.SetPixel(x, y, Color.clear);
                    }
                }
                texture.Apply();

                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
                sr.sprite = sprite;
                sr.sortingOrder = 10;

                hitMarkers.Add(marker);
            }

            correctHitMarkerIndex = Random.Range(0, hitMarkers.Count);
        }

        void Update()
        {
            if (isGaslightingActive)
            {
                UpdateFakeHealthDisplay();
                UpdateHitMarkerMisleading();
            }
        }

        public void UpdatePlayerHearts(int currentLives, int maxLives = 3)
        {
            for (int i = 0; i < playerHearts.Count && i < maxLives; i++)
            {
                if (playerHearts[i] == null) continue;

                if (i < currentLives)
                {
                    if (fullHeartSprite != null)
                        playerHearts[i].sprite = fullHeartSprite;
                    else
                        playerHearts[i].color = Color.red;
                }
                else
                {
                    if (emptyHeartSprite != null)
                        playerHearts[i].sprite = emptyHeartSprite;
                    else
                        playerHearts[i].color = Color.gray;
                }
            }
        }

        public void SetIntensity(BugManager.BugIntensity intensity)
        {
            currentIntensity = intensity;

            if (intensity == BugManager.BugIntensity.Aggressive)
            {
                fakeDamageChance = 0.5f;
                preventRealDamageChance = 0.3f;
                fakeHealChance = 0.6f;
                wrongHitMarkerChance = 0.8f;
            }
            else
            {
                fakeDamageChance = 0.3f;
                preventRealDamageChance = 0.2f;
                fakeHealChance = 0.4f;
                wrongHitMarkerChance = 0.5f;
            }
        }

        public void SetPlayer(GameObject player)
        {
            this.player = player;
        }

        public void TriggerGaslighting()
        {
            if (isGaslightingActive)
                return;

            isGaslightingActive = true;
            StartCoroutine(GaslightingSequence());
        }

        public IEnumerator TriggerGaslightingTimed(float duration)
        {
            TriggerGaslighting();
            yield return new WaitForSeconds(duration);
            ClearGaslighting();
        }

        IEnumerator GaslightingSequence()
        {
            float duration = currentIntensity == BugManager.BugIntensity.Mild ? 5f : 8f;
            float endTime = Time.time + duration;

            while (Time.time < endTime && isGaslightingActive)
            {
                float randomEvent = Random.value;

                if (randomEvent < fakeDamageChance)
                    TriggerFakeDamage();
                else if (randomEvent < fakeDamageChance + wrongHitMarkerChance)
                    TriggerWrongHitMarkers();
                else if (randomEvent < fakeDamageChance + wrongHitMarkerChance + fakeHealChance)
                    TriggerFakeHeal();

                yield return new WaitForSeconds(Random.Range(1f, 3f));
            }
        }

        void UpdateFakeHealthDisplay()
        {
            if (bossController == null) return;

            float realHealth = bossController.GetHealthPercentage();

            if (isShowingFakeHealth)
            {
                displayedBossHealth = Mathf.MoveTowards(displayedBossHealth, realHealth, Time.deltaTime * 0.3f);
                UpdateBossHealthUI(displayedBossHealth);

                if (Mathf.Abs(displayedBossHealth - realHealth) < 0.01f)
                    isShowingFakeHealth = false;
            }
            else
            {
                displayedBossHealth = realHealth;
                UpdateBossHealthUI(realHealth);
            }
        }

        void UpdateBossHealthUI(float normalizedHealth)
        {
            if (bossHealthImage != null)
            {
                bossHealthImage.fillAmount = Mathf.Clamp01(normalizedHealth);
            }
            else if (bossHealthFill != null)
            {
                bossHealthFill.localScale = new Vector3(Mathf.Clamp01(normalizedHealth), 1f, 1f);
            }

            if (bossHealthPercentageText != null)
            {
                bossHealthPercentageText.text = $"{normalizedHealth * 100f:F0}%";
            }
        }

        float lastShuffleTime = 0f;
        float shuffleInterval = 1f;

        void UpdateHitMarkerMisleading()
        {
            if (Time.time - lastShuffleTime >= shuffleInterval && Random.value < wrongHitMarkerChance)
            {
                ShuffleHitMarkers();
                lastShuffleTime = Time.time;
            }
        }

        public bool ShouldPreventDamage()
        {
            return isGaslightingActive && Random.value < preventRealDamageChance;
        }

        public void ShowFakeDamage()
        {
            StartCoroutine(FlashHealthBar(Color.red));
            var uiSystem = FindObjectOfType<UIBetrayalSystem>();
            uiSystem?.ShowDebugMessage("Hit registered!", 1f);
        }

        void TriggerFakeDamage()
        {
            if (isShowingFakeHealth) return;

            float fakeDamageAmount = Random.Range(0.05f, 0.15f);
            displayedBossHealth = Mathf.Max(0f, displayedBossHealth - fakeDamageAmount);
            isShowingFakeHealth = true;
            UpdateBossHealthUI(displayedBossHealth);
            StartCoroutine(FlashHealthBar(Color.red));
        }

        void TriggerFakeHeal()
        {
            float fakeHealAmount = Random.Range(0.03f, 0.08f);
            displayedBossHealth = Mathf.Min(1f, displayedBossHealth + fakeHealAmount);
            isShowingFakeHealth = true;
            UpdateBossHealthUI(displayedBossHealth);
            StartCoroutine(FlashHealthBar(Color.green));

            var uiSystem = FindObjectOfType<UIBetrayalSystem>();
            uiSystem?.ShowFakeDialog($"Auto-Heal Patch Applied (+{fakeHealAmount * 100f:F0} HP)", 2f);
        }

        void TriggerWrongHitMarkers()
        {
            ShuffleHitMarkers();
            StartCoroutine(HighlightWrongMarkers());
        }

        void ShuffleHitMarkers()
        {
            if (hitMarkers.Count < 2) return;

            int oldCorrect = correctHitMarkerIndex;
            do
            {
                correctHitMarkerIndex = Random.Range(0, hitMarkers.Count);
            } while (correctHitMarkerIndex == oldCorrect);
        }

        IEnumerator HighlightWrongMarkers()
        {
            var wrongMarkers = new List<GameObject>();

            for (int i = 0; i < hitMarkers.Count; i++)
            {
                if (i != correctHitMarkerIndex && hitMarkers[i] != null)
                    wrongMarkers.Add(hitMarkers[i]);
            }

            int flashCount = 3;
            float flashDuration = 0.3f;

            for (int i = 0; i < flashCount; i++)
            {
                foreach (var marker in wrongMarkers)
                {
                    if (marker == null) continue;
                    var sr = marker.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = Color.yellow;
                }

                yield return new WaitForSeconds(flashDuration);

                foreach (var marker in wrongMarkers)
                {
                    if (marker == null) continue;
                    var sr = marker.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = Color.white;
                }

                yield return new WaitForSeconds(flashDuration);
            }
        }

        IEnumerator FlashHealthBar(Color flashColor)
        {
            if (bossHealthImage == null)
                yield break;

            Color originalColor = bossHealthImage.color;
            bossHealthImage.color = flashColor;

            yield return new WaitForSeconds(0.2f);

            bossHealthImage.color = originalColor;
        }

        public bool IsHitMarkerCorrect(GameObject hitMarker)
        {
            return hitMarkers.IndexOf(hitMarker) == correctHitMarkerIndex;
        }

        public void OnPlayerAttack(GameObject targetMarker)
        {
            if (!isGaslightingActive)
                return;

            bool hitCorrect = IsHitMarkerCorrect(targetMarker);

            if (!hitCorrect && Random.value < wrongHitMarkerChance)
                TriggerFakeDamage();
            else if (hitCorrect && Random.value < preventRealDamageChance)
            {
                // Real damage prevention handled externally via ShouldPreventDamage()
            }
        }

        public void ClearGaslighting()
        {
            isGaslightingActive = false;
            isShowingFakeHealth = false;

            if (bossController != null)
            {
                float realHealth = bossController.GetHealthPercentage();
                UpdateBossHealthUI(realHealth);
                displayedBossHealth = realHealth;
            }
        }

        public void AddHitMarker(GameObject marker)
        {
            if (!hitMarkers.Contains(marker))
                hitMarkers.Add(marker);
        }

        public void RemoveHitMarker(GameObject marker)
        {
            if (hitMarkers.Remove(marker) && correctHitMarkerIndex >= hitMarkers.Count)
                correctHitMarkerIndex = Mathf.Max(0, hitMarkers.Count - 1);
        }

        public GameObject GetCorrectHitMarker()
        {
            if (correctHitMarkerIndex >= 0 && correctHitMarkerIndex < hitMarkers.Count)
                return hitMarkers[correctHitMarkerIndex];
            return null;
        }

        // === Added missing methods to fix errors ===

        public bool IsCurrentlyGaslighting()
        {
            return isGaslightingActive;
        }

        public void StopGaslighting()
        {
            ClearGaslighting();
        }

        public bool ShouldBlockDamage()
        {
            return isGaslightingActive && Random.value < preventRealDamageChance;
        }

        // Added an overload for UpdateHealthBar to accept 2 params to fix errors
        public void UpdateHealthBar(float healthNormalized, bool useFake = false)
        {
            if (useFake)
            {
                displayedBossHealth = healthNormalized;
                isShowingFakeHealth = true;
            }
            UpdateBossHealthUI(healthNormalized);
        }

        public void TriggerCollisionParadox()
        {
            Debug.LogWarning("TriggerCollisionParadox called on GoalGaslightingSystem but not implemented.");
        }
    }
}
