using UnityEngine;
using UnityEngine.UI;

public class RawImageMaterialSwitcher : MonoBehaviour
{
    public RawImage targetRawImage;          // Assign your RawImage here
    public Material[] materials;              // Assign your materials here
    private int currentIndex = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwitchMaterial();
        }
    }

    public void SwitchMaterial()
    {
        if (materials == null || materials.Length == 0) return;

        currentIndex = (currentIndex + 1) % materials.Length;
        targetRawImage.material = materials[currentIndex];
    }
}
