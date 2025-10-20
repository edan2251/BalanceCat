using UnityEngine;

[ExecuteInEditMode]
public class PixelationManager : MonoBehaviour
{
    [Tooltip("픽셀화의 해상도를 조절합니다. 값이 낮을수록 픽셀이 커집니다.")]
    [Range(32, 1024)]
    public int resolution = 256;

    void Update()
    {
        PixelationFeature.GlobalPixelResolution = resolution;
    }
}