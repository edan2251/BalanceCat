using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PixelationFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class PixelationSettings
    {
        public Material material;
        [Range(32, 1024)]
        public int pixelResolution = 256;
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

        // �ȼ� ũ�� ��� �� ���̴��� ����
        Vector2 pixelSize = new Vector2(
            1.0f / settings.pixelResolution, // _PixelSize.x
            1.0f / (settings.pixelResolution * (float)renderingData.cameraData.camera.pixelHeight / (float)renderingData.cameraData.camera.pixelWidth) // _PixelSize.y (ȭ�� ���� ����)
        );

        settings.material.SetVector("_PixelSize", new Vector4(pixelSize.x, pixelSize.y, 0, 0));

        // ���� �н� ����
        // **���� �߻� ���� ����: pixelationPass.Setup(renderer.cameraColorTargetHandle);**

        renderer.EnqueuePass(pixelationPass);
    }

    // �����Ϳ��� ����� ���ŵǰų� ���ø����̼��� ����� �� ȣ��
    protected override void Dispose(bool disposing)
    {
        pixelationPass.Dispose();
    }
}