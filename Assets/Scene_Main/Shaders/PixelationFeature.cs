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

    // Feature가 활성화될 때 한 번 호출
    public override void Create()
    {
        pixelationPass = new PixelationRenderPass(settings.material);
    }

    // 카메라마다 렌더 패스가 큐에 추가될 때 호출
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null) return;

        int currentResolution = GlobalPixelResolution;

        // 픽셀 크기 계산 및 셰이더에 전달
        Vector2 pixelSize = new Vector2(
            1.0f / currentResolution, // _PixelSize.x
            1.0f / (currentResolution * (float)renderingData.cameraData.camera.pixelHeight / (float)renderingData.cameraData.camera.pixelWidth) // _PixelSize.y
        );

        settings.material.SetVector("_PixelSize", new Vector4(pixelSize.x, pixelSize.y, 0, 0));


        renderer.EnqueuePass(pixelationPass);
    }

    // 에디터에서 기능이 제거되거나 어플리케이션이 종료될 때 호출
    protected override void Dispose(bool disposing)
    {
        pixelationPass.Dispose();
    }
}