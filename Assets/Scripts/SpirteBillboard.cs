using UnityEngine;

public class SpirteBillboard : MonoBehaviour
{
    [SerializeField] bool freezeXZAxis = true;
    private void Update()
    {
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
