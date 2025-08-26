using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class VignetteWakeUp : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image vignetteImage;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Settings")]
    [SerializeField] private float minDelayBetweenCycles = 1f;
    [SerializeField] private float maxDelayBetweenCycles = 5f;
    [SerializeField] private float minFadeSpeed = 0.2f;
    [SerializeField] private float maxFadeSpeed = 1.5f;
    [SerializeField] private float maxAlpha = 1f;

    private KeyCode chosenKey;
    private bool requiresShift = false;
    private int pressCount = 0;
    private bool isFadingIn = false;
    private bool isFadingOut = false;

    private List<KeyCode> allowedKeys = new List<KeyCode>();

    private void Start()
    {
        if (vignetteImage != null)
        {
            Color c = vignetteImage.color;
            c.a = 0f;
            vignetteImage.color = c;
            vignetteImage.raycastTarget = false;
        }

        // Add letters A–Z
        for (KeyCode kc = KeyCode.A; kc <= KeyCode.Z; kc++)
            allowedKeys.Add(kc);

        // Add numbers 0–9 (Alpha keys at top of keyboard)
        for (KeyCode kc = KeyCode.Alpha0; kc <= KeyCode.Alpha9; kc++)
            allowedKeys.Add(kc);

        // Add spacebar
        allowedKeys.Add(KeyCode.Space);

        StartCoroutine(FadeCycle());
    }

    private void Update()
    {
        if (isFadingIn)
        {
            bool correctPress = false;

            if (requiresShift)
            {
                if (Input.GetKeyDown(chosenKey) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                    correctPress = true;
            }
            else
            {
                if (Input.GetKeyDown(chosenKey))
                    correctPress = true;
            }

            if (correctPress)
            {
                pressCount++;
                if (pressCount >= 3)
                {
                    pressCount = 0;
                    StartCoroutine(FadeOut());
                }
            }
        }
    }

    private IEnumerator FadeCycle()
    {
        while (true)
        {
            float delay = Random.Range(minDelayBetweenCycles, maxDelayBetweenCycles);
            yield return new WaitForSeconds(delay);

            chosenKey = allowedKeys[Random.Range(0, allowedKeys.Count)];
            requiresShift = Random.value < 0.3f; // 30% chance to require Shift

            if (promptText != null)
            {
                string keyDisplay = FormatKey(chosenKey);
                if (requiresShift)
                    promptText.text = $"Press [Shift + {keyDisplay}] three times to wake up!";
                else
                    promptText.text = $"Press [{keyDisplay}] three times to wake up!";
            }

            float fadeSpeed = Random.Range(minFadeSpeed, maxFadeSpeed);
            yield return StartCoroutine(FadeIn(fadeSpeed));
        }
    }

    private IEnumerator FadeIn(float fadeSpeed)
    {
        isFadingIn = true;
        isFadingOut = false;

        while (vignetteImage.color.a < maxAlpha)
        {
            Color c = vignetteImage.color;
            c.a = Mathf.MoveTowards(c.a, maxAlpha, fadeSpeed * Time.deltaTime);
            vignetteImage.color = c;
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        isFadingOut = true;
        isFadingIn = false;

        float fadeSpeed = Random.Range(minFadeSpeed, maxFadeSpeed);

        while (vignetteImage.color.a > 0f)
        {
            Color c = vignetteImage.color;
            c.a = Mathf.MoveTowards(c.a, 0f, fadeSpeed * Time.deltaTime);
            vignetteImage.color = c;
            yield return null;
        }
    }

    private string FormatKey(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Space: return "Space";
            case KeyCode.Alpha0: return "0";
            case KeyCode.Alpha1: return "1";
            case KeyCode.Alpha2: return "2";
            case KeyCode.Alpha3: return "3";
            case KeyCode.Alpha4: return "4";
            case KeyCode.Alpha5: return "5";
            case KeyCode.Alpha6: return "6";
            case KeyCode.Alpha7: return "7";
            case KeyCode.Alpha8: return "8";
            case KeyCode.Alpha9: return "9";
            default: return key.ToString(); // Letters A–Z
        }
    }
}
