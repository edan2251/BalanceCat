using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PixelationFeature : ScriptableRendererFeature
{
    public static int GlobalPixelResolution = 256;

    [System.Serializable]
    public class PixelationSettings
    {
        public Material material;
    }

    public PixelationSettings settings = new PixelationSettings();

    private PixelationRenderPass pixelationPass;

    // Feature�� Ȱ��ȭ�� �� �� �� ȣ��
    public override void Create()
    {
        pixelationPass = new PixelationRenderPass(settings.material);
    }

    // ī�޶󸶴� ���� �н��� ť�� �߰��� �� ȣ��
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null) return;

        int currentResolution = GlobalPixelResolution;

        // �ȼ� ũ�� ��� �� ���̴��� ����
        Vector2 pixelSize = new Vector2(
            1.0f / currentResolution, // _PixelSize.x
            1.0f / (currentResolution * (float)renderingData.cameraData.camera.pixelHeight / (float)renderingData.cameraData.camera.pixelWidth) // _PixelSize.y
        );

        settings.material.SetVector("_PixelSize", new Vector4(pixelSize.x, pixelSize.y, 0, 0));


        renderer.EnqueuePass(pixelationPass);
    }

    // �����Ϳ��� ����� ���ŵǰų� ���ø����̼��� ����� �� ȣ��
    protected override void Dispose(bool disposing)
    {
        pixelationPass.Dispose();
    }
}