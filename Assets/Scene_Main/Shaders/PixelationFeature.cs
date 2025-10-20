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

    // Feature가 활성화될 때 한 번 호출
    public override void Create()
    {
        pixelationPass = new PixelationRenderPass(settings.material);
    }

    // 카메라마다 렌더 패스가 큐에 추가될 때 호출
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null) return;

        // 픽셀 크기 계산 및 셰이더에 전달
        Vector2 pixelSize = new Vector2(
            1.0f / settings.pixelResolution, // _PixelSize.x
            1.0f / (settings.pixelResolution * (float)renderingData.cameraData.camera.pixelHeight / (float)renderingData.cameraData.camera.pixelWidth) // _PixelSize.y (화면 비율 보정)
        );

        settings.material.SetVector("_PixelSize", new Vector4(pixelSize.x, pixelSize.y, 0, 0));

        // 렌더 패스 설정
        // **오류 발생 지점 제거: pixelationPass.Setup(renderer.cameraColorTargetHandle);**

        renderer.EnqueuePass(pixelationPass);
    }

    // 에디터에서 기능이 제거되거나 어플리케이션이 종료될 때 호출
    protected override void Dispose(bool disposing)
    {
        pixelationPass.Dispose();
    }
}