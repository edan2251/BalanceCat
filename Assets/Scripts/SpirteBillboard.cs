//using UnityEngine;

//public class SpirteBillboard : MonoBehaviour
//{
//    [SerializeField] bool freezeXZAxis = true;
//    private void Update()
//    {
//        if (freezeXZAxis)
//        {
//            transform.rotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
//        }
//        else
//        {
//            transform.rotation = Camera.main.transform.rotation;
//        }
//    }
//}
using UnityEngine;

public class SpirteBillboard : MonoBehaviour
{
    [SerializeField] bool freezeXZAxis = true;

    private void Update()
    {
        // 2D 모드이면 정면 고정
        if (CameraToggle.use2D)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f); // 정면 방향
            return;
        }

        // 3D 모드일 때 Billboard
        if (freezeXZAxis)
        {
            transform.rotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
        }
        else
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}