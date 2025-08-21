using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MasterMaterialToggle : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Toggle masterToggle; // Single toggle to control all materials

    [Header("Settings")]
    [SerializeField] private string toggleLabel = "Enable Effects"; // Label for the toggle
    [SerializeField] private bool showDebugLogs = true;

    private void Start()
    {
        // Wait a bit to ensure GlobalMaterialManager is ready
        Invoke(nameof(InitializeMasterToggle), 0.2f);
    }

    private void InitializeMasterToggle()
    {
        if (masterToggle == null)
        {
            Debug.LogError("MasterMaterialToggle: Master toggle is not assigned!");
            return;
        }

        // Set up the toggle label
        Text label = masterToggle.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.text = toggleLabel;
        }

        // Set initial state based on the first material (or all if they should be the same)
        SetInitialToggleState();

        // Add listener for toggle changes
        masterToggle.onValueChanged.AddListener(OnMasterToggleChanged);

        if (showDebugLogs)
        {
            List<MaterialSetting> settings = GlobalMaterialManager.Instance.GetMaterialSettings();
            Debug.Log($"MasterMaterialToggle: Initialized master toggle controlling {settings.Count} materials");
        }
    }

    private void SetInitialToggleState()
    {
        List<MaterialSetting> settings = GlobalMaterialManager.Instance.GetMaterialSettings();

        if (settings.Count == 0)
        {
            Debug.LogWarning("MasterMaterialToggle: No material settings found!");
            masterToggle.isOn = true; // Default to enabled
            return;
        }

        // Check if all materials have the same state
        bool firstMaterialEnabled = !settings[0].isDisabled;
        bool allSameState = true;

        foreach (MaterialSetting setting in settings)
        {
            if ((!setting.isDisabled) != firstMaterialEnabled)
            {
                allSameState = false;
                break;
            }
        }

        if (allSameState)
        {
            // All materials have the same state
            masterToggle.isOn = firstMaterialEnabled;
        }
        else
        {
            // Mixed states - default to enabled and force all materials to enabled
            masterToggle.isOn = true;
            SetAllMaterialsEnabled(true);
        }

        if (showDebugLogs)
        {
            Debug.Log($"MasterMaterialToggle: Initial state set to {masterToggle.isOn} (all same state: {allSameState})");
        }
    }

    private void OnMasterToggleChanged(bool isEnabled)
    {
        SetAllMaterialsEnabled(isEnabled);

        if (showDebugLogs)
        {
            Debug.Log($"MasterMaterialToggle: All materials set to {(isEnabled ? "Enabled" : "Disabled")}");
        }
    }

    private void SetAllMaterialsEnabled(bool enabled)
    {
        List<MaterialSetting> settings = GlobalMaterialManager.Instance.GetMaterialSettings();

        foreach (MaterialSetting setting in settings)
        {
            GlobalMaterialManager.Instance.SetMaterialEnabled(setting.materialName, enabled);
        }

        if (showDebugLogs)
        {
            Debug.Log($"MasterMaterialToggle: Applied {(enabled ? "enabled" : "disabled")} state to {settings.Count} materials");
        }
    }

    // Method to manually refresh the toggle state (useful if materials change externally)
    public void RefreshToggleState()
    {
        SetInitialToggleState();
    }

    // Public methods for external control (can be called from buttons, etc.)
    public void EnableAllMaterials()
    {
        masterToggle.isOn = true;
        // The onValueChanged listener will handle the rest
    }

    public void DisableAllMaterials()
    {
        masterToggle.isOn = false;
        // The onValueChanged listener will handle the rest
    }

    // Context menu for testing
    [ContextMenu("Enable All Materials")]
    public void ForceEnableAll()
    {
        EnableAllMaterials();
    }

    [ContextMenu("Disable All Materials")]
    public void ForceDisableAll()
    {
        DisableAllMaterials();
    }

    [ContextMenu("Debug Master Toggle State")]
    public void DebugToggleState()
    {
        List<MaterialSetting> settings = GlobalMaterialManager.Instance.GetMaterialSettings();
        Debug.Log($"=== Master Toggle Debug ===");
        Debug.Log($"Master Toggle State: {masterToggle.isOn}");
        Debug.Log($"Total Materials: {settings.Count}");

        for (int i = 0; i < settings.Count; i++)
        {
            MaterialSetting setting = settings[i];
            Debug.Log($"[{i}] '{setting.materialName}': {(!setting.isDisabled ? "Enabled" : "Disabled")}");
        }
    }
}