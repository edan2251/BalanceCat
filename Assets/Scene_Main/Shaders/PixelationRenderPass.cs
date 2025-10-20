using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelationRenderPass : ScriptableRenderPass
{
    private Material effectMaterial;
    // RTHandle�� ����ϸ� Unity�� ���� Ÿ���� ȿ�������� ������ �� �ֽ��ϴ�.
    private RTHandle tempRT;

    // �ӽ� RT�� �̸��� ����� ����
    private const string TempRTName = "_TempPixelationTexture";

    public PixelationRenderPass(Material material)
    {
        this.effectMaterial = material;
        renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // RTHandle�� ���⼭ �Ҵ��ϰ�, �Ź� ���Ҵ����� �ʵ��� �մϴ�.
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // ���� ī�޶��� ���� Ÿ�� ��ũ���͸� �����ɴϴ�.
        var cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        cameraTargetDescriptor.depthBufferBits = 0; // ���� ���� ����

        // ���� ī�޶��� ũ�⿡ �°� RTHandle�� �Ҵ��մϴ�.
        // ReAllocateIfNeeded�� �̹� �Ҵ�Ǿ� ũ�Ⱑ ������ �ƹ� �۾��� ���� �ʾ� ȿ�����Դϴ�.
        RenderingUtils.ReAllocateIfNeeded(ref tempRT, cameraTargetDescriptor, name: TempRTName);

        // **JobTempAlloc ��� ���̱� ����: (���� ���������� ����)**
        // Unity ���ο��� MaterialPropertyBlock�� ����ϴ� ��� 
        // ���� �н� ���� �� Material�� �ؽ�ó�� ��������� ������ �ִ� ���� ������ �� �� �ֽ��ϴ�.
        // effectMaterial.SetTexture("_MainTex", cameraColorTargetHandle); // Execute���� ó���� ���̹Ƿ� �ϴ� �ּ� ó��
    }

    // ���� ������ �۾��� �̷������ ��
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (effectMaterial == null || tempRT == null) return;

        RTHandle cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

        CommandBuffer cmd = CommandBufferPool.Get("Pixelation Effect");

        // Material�� ����ϴ� ���, Graphics.Blit ��� CommandBuffer.Blit�� ����ϴ� ���� �� �������Դϴ�.

        // 1. ���� ȭ���� �ӽ� RT�� ���մϴ�. (���⼭ ���̴� ����)
        // **Material�� ����ϴ� Blit�� ���, ù ��° ���ڷ� source �ؽ�ó�� ��������� �����մϴ�.**
        cmd.Blit(cameraColorTargetHandle, tempRT, effectMaterial, 0);

        // 2. �ӽ� RT�� �ȼ�ȭ�� ����� �ٽ� ���� ȭ������ ���մϴ�. (Material ����)
        cmd.Blit(tempRT, cameraColorTargetHandle);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    // ����� ���ŵ� �� ȣ��˴ϴ�.
    // RTHandle.Release()�� ���⼭ ȣ���Ͽ� JobTempAlloc ��� �ذ��մϴ�.
    public void Dispose()
    {
        tempRT?.Release();
    }
}