using UnityEngine;

public class SpirteBillboard : MonoBehaviour
{
    [SerializeField] bool freezeXZAxis = true;

    private void Update()
    {
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