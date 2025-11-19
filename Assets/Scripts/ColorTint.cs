using UnityEngine;

public class ColorTint : MonoBehaviour
{
    public Color darkColor = Color.gray;

    private MaterialPropertyBlock propBlock;


    void OnEnable()
    {
        ApplyColor();
    }

    void OnValidate()
    {
        ApplyColor();
    }

    void ApplyColor()
    {
        if (propBlock == null)
            propBlock = new MaterialPropertyBlock();

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            r.GetPropertyBlock(propBlock);

            propBlock.SetColor("_BaseColor", darkColor);

            r.SetPropertyBlock(propBlock);
        }
    }
}