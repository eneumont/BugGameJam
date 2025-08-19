using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown graphicsDropdown; // or TMP_Dropdown

    private const string GraphicsKey = "GraphicsQuality";

    void Start()
    {
        // Load saved graphics setting
        int savedQuality = PlayerPrefs.GetInt(GraphicsKey, QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(savedQuality);

        if (graphicsDropdown != null)
        {
            graphicsDropdown.value = savedQuality;
            graphicsDropdown.onValueChanged.AddListener(SetGraphicsQuality);
        }
    }

    public void SetGraphicsQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt(GraphicsKey, qualityIndex);
    }
}
