using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelationRenderPass : ScriptableRenderPass
{
    private Material effectMaterial;
    // RTHandle을 사용하면 Unity가 렌더 타겟을 효율적으로 관리할 수 있습니다.
    private RTHandle tempRT;

    // 임시 RT의 이름을 상수로 정의
    private const string TempRTName = "_TempPixelationTexture";

    public PixelationRenderPass(Material material)
    {
        this.effectMaterial = material;
        renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // RTHandle을 여기서 할당하고, 매번 재할당하지 않도록 합니다.
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // 현재 카메라의 렌더 타겟 디스크립터를 가져옵니다.
        var cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        cameraTargetDescriptor.depthBufferBits = 0; // 깊이 버퍼 제거

        // 현재 카메라의 크기에 맞게 RTHandle을 할당합니다.
        // ReAllocateIfNeeded는 이미 할당되어 크기가 맞으면 아무 작업도 하지 않아 효율적입니다.
        RenderingUtils.ReAllocateIfNeeded(ref tempRT, cameraTargetDescriptor, name: TempRTName);

        // **JobTempAlloc 경고를 줄이기 위해: (선택 사항이지만 권장)**
        // Unity 내부에서 MaterialPropertyBlock을 사용하는 경우 
        // 렌더 패스 시작 시 Material에 텍스처를 명시적으로 설정해 주는 것이 도움이 될 수 있습니다.
        // effectMaterial.SetTexture("_MainTex", cameraColorTargetHandle); // Execute에서 처리할 것이므로 일단 주석 처리
    }

    // 실제 렌더링 작업이 이루어지는 곳
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (effectMaterial == null || tempRT == null) return;

        RTHandle cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

        CommandBuffer cmd = CommandBufferPool.Get("Pixelation Effect");

        // Material을 사용하는 경우, Graphics.Blit 대신 CommandBuffer.Blit을 사용하는 것이 더 안정적입니다.

        // 1. 현재 화면을 임시 RT로 블릿합니다. (여기서 셰이더 적용)
        // **Material을 사용하는 Blit의 경우, 첫 번째 인자로 source 텍스처를 명시적으로 전달합니다.**
        cmd.Blit(cameraColorTargetHandle, tempRT, effectMaterial, 0);

        // 2. 임시 RT의 픽셀화된 결과를 다시 최종 화면으로 블릿합니다. (Material 없이)
        cmd.Blit(tempRT, cameraColorTargetHandle);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    // 기능이 제거될 때 호출됩니다.
    // RTHandle.Release()를 여기서 호출하여 JobTempAlloc 경고를 해결합니다.
    public void Dispose()
    {
        tempRT?.Release();
    }
}