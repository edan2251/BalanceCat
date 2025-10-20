using UnityEngine;

[ExecuteInEditMode]
public class PixelationManager : MonoBehaviour
{
    [Tooltip("�ȼ�ȭ�� �ػ󵵸� �����մϴ�. ���� �������� �ȼ��� Ŀ���ϴ�.")]
    [Range(32, 1024)]
    public int resolution = 256;

    void Update()
    {
        PixelationFeature.GlobalPixelResolution = resolution;
    }
}