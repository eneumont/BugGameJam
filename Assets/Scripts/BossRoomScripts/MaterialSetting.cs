using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class MaterialSetting
{
    public string materialName;
    public Material originalMaterial;
    public Material disabledMaterial; // Alternative material when disabled
    public bool isDisabled;

    public MaterialSetting(string name, Material original, Material disabled)
    {
        materialName = name;
        originalMaterial = original;
        disabledMaterial = disabled;
        isDisabled = false;
    }
}

// Interface to handle different component types that use materials
public interface IMaterialUser
{
    Material GetMaterial();
    void SetMaterial(Material material);
    Component GetComponent();
}

public class RendererMaterialUser : IMaterialUser
{
    private readonly Renderer renderer;
    private readonly int materialIndex;

    public RendererMaterialUser(Renderer renderer, int materialIndex)
    {
        this.renderer = renderer;
        this.materialIndex = materialIndex;
    }

    public Material GetMaterial()
    {
        return renderer.materials[materialIndex];
    }

    public void SetMaterial(Material material)
    {
        Material[] materials = renderer.materials;
        materials[materialIndex] = material;
        renderer.materials = materials;
    }

    public Component GetComponent()
    {
        return renderer;
    }
}

// Wrapper for UI Image components
public class ImageMaterialUser : IMaterialUser
{
    private readonly Image image;

    public ImageMaterialUser(Image image)
    {
        this.image = image;
    }

    public Material GetMaterial()
    {
        return image.material;
    }

    public void SetMaterial(Material material)
    {
        image.material = material;
    }

    public Component GetComponent()
    {
        return image;
    }
}

// Wrapper for UI RawImage components
public class RawImageMaterialUser : IMaterialUser
{
    private readonly RawImage rawImage;

    public RawImageMaterialUser(RawImage rawImage)
    {
        this.rawImage = rawImage;
    }

    public Material GetMaterial()
    {
        return rawImage.material;
    }

    public void SetMaterial(Material material)
    {
        rawImage.material = material;
    }

    public Component GetComponent()
    {
        return rawImage;
    }
}
