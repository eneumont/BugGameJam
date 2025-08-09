using UnityEngine;
using UnityEngine.UI;

public class WaveTexture : MonoBehaviour
{
    public float waveSpeed = 0.1f;
    public float waveAmount = 0.02f;

    private Material material;
    private Vector2 originalOffset;

    void Start()
    {
        Image img = GetComponent<Image>();
        material = Instantiate(img.material);
        img.material = material;
        originalOffset = material.mainTextureOffset;
    }

    void Update()
    {
        float offsetX = Mathf.Sin(Time.time * waveSpeed) * waveAmount;
        float offsetY = Mathf.Cos(Time.time * waveSpeed) * waveAmount;
        material.mainTextureOffset = originalOffset + new Vector2(offsetX, offsetY);
    }
}
