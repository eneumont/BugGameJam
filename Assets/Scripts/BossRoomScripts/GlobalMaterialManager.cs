using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GlobalMaterialManager : MonoBehaviour
{
    [Header("Material Settings")]
    [SerializeField] private List<MaterialSetting> materialSettings = new();

    // Track all components using managed materials
    private readonly Dictionary<Material, List<IMaterialUser>> materialToUsers = new();
    private bool isInitialized = false;

    // Singleton pattern
    private static GlobalMaterialManager _instance;
    public static GlobalMaterialManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GlobalMaterialManager>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("GlobalMaterialManager");
                    _instance = go.AddComponent<GlobalMaterialManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadMaterialSettings();
    }

    private void Start()
    {
        // Initialize after all objects are loaded
        Invoke(nameof(InitializeMaterialTracking), 0.1f);
    }

    public void AddMaterialSetting(string materialName, Material originalMaterial, Material disabledMaterial)
    {
        MaterialSetting existing = materialSettings.Find(s => s.materialName == materialName);
        if (existing == null)
        {
            materialSettings.Add(new MaterialSetting(materialName, originalMaterial, disabledMaterial));
        }
    }

    public void SetMaterialEnabled(string materialName, bool enabled)
    {
        MaterialSetting setting = materialSettings.Find(s => s.materialName == materialName);
        if (setting != null)
        {
            setting.isDisabled = !enabled;
            ApplyMaterialState(setting);
            SaveMaterialSettings();
        }
    }

    public void SetMaterialEnabledByIndex(int index, bool enabled)
    {
        if (index >= 0 && index < materialSettings.Count)
        {
            materialSettings[index].isDisabled = !enabled;
            ApplyMaterialState(materialSettings[index]);
            SaveMaterialSettings();
        }
    }

    public bool IsMaterialEnabled(string materialName)
    {
        MaterialSetting setting = materialSettings.Find(s => s.materialName == materialName);
        return setting != null ? !setting.isDisabled : true;
    }

    public List<MaterialSetting> GetMaterialSettings()
    {
        return new List<MaterialSetting>(materialSettings);
    }

    private void InitializeMaterialTracking()
    {
        if (isInitialized) return;

        // Clear existing tracking
        materialToUsers.Clear();

        // Find all renderers in the scene (including inactive)
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Renderer renderer in allRenderers)
        {
            TrackRenderer(renderer);
        }

        // Find all UI Images in the scene (including inactive)
        Image[] allImages = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Image image in allImages)
        {
            TrackImage(image);
        }

        // Find all UI RawImages in the scene (including inactive)
        RawImage[] allRawImages = FindObjectsByType<RawImage>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (RawImage rawImage in allRawImages)
        {
            TrackRawImage(rawImage);
        }

        // Apply current material states
        ApplyAllMaterialStates();
        isInitialized = true;
    }

    private void TrackRenderer(Renderer renderer)
    {
        for (int i = 0; i < renderer.materials.Length; i++)
        {
            Material material = renderer.materials[i];
            MaterialSetting setting = materialSettings.Find(s => s.originalMaterial == material || s.disabledMaterial == material);

            if (setting != null)
            {
                Material trackingMaterial = setting.originalMaterial;

                if (!materialToUsers.ContainsKey(trackingMaterial))
                {
                    materialToUsers[trackingMaterial] = new List<IMaterialUser>();
                }

                RendererMaterialUser user = new RendererMaterialUser(renderer, i);
                if (!ContainsUser(materialToUsers[trackingMaterial], user))
                {
                    materialToUsers[trackingMaterial].Add(user);
                }
            }
        }
    }

    private void TrackImage(Image image)
    {
        if (image.material != null)
        {
            MaterialSetting setting = materialSettings.Find(s => s.originalMaterial == image.material || s.disabledMaterial == image.material);

            if (setting != null)
            {
                Material trackingMaterial = setting.originalMaterial;

                if (!materialToUsers.ContainsKey(trackingMaterial))
                {
                    materialToUsers[trackingMaterial] = new List<IMaterialUser>();
                }

                ImageMaterialUser user = new ImageMaterialUser(image);
                if (!ContainsUser(materialToUsers[trackingMaterial], user))
                {
                    materialToUsers[trackingMaterial].Add(user);
                }
            }
        }
    }

    private void TrackRawImage(RawImage rawImage)
    {
        if (rawImage.material != null)
        {
            MaterialSetting setting = materialSettings.Find(s => s.originalMaterial == rawImage.material || s.disabledMaterial == rawImage.material);

            if (setting != null)
            {
                Material trackingMaterial = setting.originalMaterial;

                if (!materialToUsers.ContainsKey(trackingMaterial))
                {
                    materialToUsers[trackingMaterial] = new List<IMaterialUser>();
                }

                RawImageMaterialUser user = new RawImageMaterialUser(rawImage);
                if (!ContainsUser(materialToUsers[trackingMaterial], user))
                {
                    materialToUsers[trackingMaterial].Add(user);
                }
            }
        }
    }

    private bool ContainsUser(List<IMaterialUser> users, IMaterialUser newUser)
    {
        foreach (IMaterialUser user in users)
        {
            if (user.GetComponent() == newUser.GetComponent())
            {
                return true;
            }
        }
        return false;
    }

    private void ApplyMaterialState(MaterialSetting setting)
    {
        if (setting.originalMaterial == null || setting.disabledMaterial == null) return;

        Material targetMaterial = setting.isDisabled ? setting.disabledMaterial : setting.originalMaterial;
        Material trackingMaterial = setting.originalMaterial;

        // Update all components using this material
        if (materialToUsers.ContainsKey(trackingMaterial))
        {
            List<IMaterialUser> users = materialToUsers[trackingMaterial];

            // Create a copy of the list to avoid modification during iteration
            List<IMaterialUser> usersCopy = new List<IMaterialUser>(users);

            foreach (IMaterialUser user in usersCopy)
            {
                if (user.GetComponent() != null)
                {
                    user.SetMaterial(targetMaterial);
                }
                else
                {
                    // Remove null references
                    users.Remove(user);
                }
            }
        }
    }

    private void ApplyAllMaterialStates()
    {
        foreach (MaterialSetting setting in materialSettings)
        {
            ApplyMaterialState(setting);
        }
    }

    // Call this when new objects are instantiated at runtime
    public void RefreshMaterialTracking()
    {
        isInitialized = false;
        InitializeMaterialTracking();
    }

    // Call this when changing scenes
    private void OnLevelWasLoaded(int level)
    {
        isInitialized = false;
        Invoke(nameof(InitializeMaterialTracking), 0.1f);
    }

    private void SaveMaterialSettings()
    {
        for (int i = 0; i < materialSettings.Count; i++)
        {
            string key = $"Material_{materialSettings[i].materialName}_Disabled";
            PlayerPrefs.SetInt(key, materialSettings[i].isDisabled ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    private void LoadMaterialSettings()
    {
        for (int i = 0; i < materialSettings.Count; i++)
        {
            string key = $"Material_{materialSettings[i].materialName}_Disabled";
            materialSettings[i].isDisabled = PlayerPrefs.GetInt(key, 0) == 1;
        }
    }
}