using UnityEngine;

public class BubbleInstanceRandomizer : MonoBehaviour
{
    [Header("Randomization Settings")]
    [Range(0f, 2f)]
    public float distortionVariationAmount = 1f;

    private Renderer bubbleRenderer;
    private MaterialPropertyBlock propertyBlock;

    void Awake()
    {
        bubbleRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        // Generate unique seed for this instance
        float instanceSeed = Random.Range(0f, 100f);

        // Set unique properties for this bubble
        SetUniqueProperties(instanceSeed);
    }

    void SetUniqueProperties(float seed)
    {
        // Get current material properties
        bubbleRenderer.GetPropertyBlock(propertyBlock);

        // Set instance-specific properties
        propertyBlock.SetFloat("_InstanceSeed", seed);
        propertyBlock.SetFloat("_DistortionVariation", distortionVariationAmount);

        // Optional: Randomize other properties per instance
        float waveSpeedVariation = Random.Range(0.8f, 1.5f);
        float pulseSpeedVariation = Random.Range(0.7f, 1.8f);
        float shimmerVariation = Random.Range(0.5f, 1.5f);

        propertyBlock.SetFloat("_WaveSpeed", 2.5f * waveSpeedVariation);
        propertyBlock.SetFloat("_PulseSpeed", 2f * pulseSpeedVariation);
        propertyBlock.SetFloat("_ShimmerSpeed", 8f * shimmerVariation);

        // Slightly randomize colors for more variety
        Color baseColor = new Color(
            Random.Range(0.6f, 1f),
            Random.Range(0.2f, 0.8f),
            Random.Range(0.5f, 1f),
            1f
        );
        propertyBlock.SetColor("_Color", baseColor);

        // Apply the property block to this instance
        bubbleRenderer.SetPropertyBlock(propertyBlock);
    }

    // Optional: Call this to re-randomize during runtime
    public void Rerandomize()
    {
        float newSeed = Random.Range(0f, 100f);
        SetUniqueProperties(newSeed);
    }
}