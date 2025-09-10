using UnityEngine;

public class CameraToggle : MonoBehaviour
{
    public GameObject camera2D;           // 2D 카메라
    public GameObject freeLookCamera;     // FreeLook 카메라 (Cinemachine Virtual Camera 포함)

    public static bool use2D = true;

    void Start()
    {
        // 시작은 2D 카메라로
        camera2D.SetActive(true);
        freeLookCamera.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) // 원하는 키 지정
        {
            use2D = !use2D;
            camera2D.SetActive(use2D);
            freeLookCamera.SetActive(!use2D);
        }
    }
}