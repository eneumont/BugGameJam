using TMPro;
using UnityEngine;

public class GraphicsSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown graphicsDropdown;

    private const string GraphicsKey = "GraphicsQuality";

    void Start()
    {
        if (graphicsDropdown != null)
        {
            // Clear existing options
            graphicsDropdown.ClearOptions();

            // Get available quality levels and add them to dropdown
            string[] qualityNames = QualitySettings.names;
            graphicsDropdown.AddOptions(new System.Collections.Generic.List<string>(qualityNames));

            // Load saved graphics setting or default to current
            int savedQuality = PlayerPrefs.GetInt(GraphicsKey, QualitySettings.GetQualityLevel());

            QualitySettings.SetQualityLevel(savedQuality);
            graphicsDropdown.value = savedQuality;
            graphicsDropdown.RefreshShownValue();

            // Add listener
            graphicsDropdown.onValueChanged.AddListener(SetGraphicsQuality);
        }
    }

    public void SetGraphicsQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt(GraphicsKey, qualityIndex);
        PlayerPrefs.Save();
    }
}
