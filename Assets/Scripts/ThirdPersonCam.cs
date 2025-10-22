using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("Referencess")]
    public Transform orientation;
    public Transform player;
    public Transform playerOBJ;
    public Rigidbody rb;

    public float rotationSpeed;

    private void Update()
    {
        //if (ShowCursor.IsLocked)
        //{
        //    CameraRotate();
        //}
        CameraRotate();
    }

   void CameraRotate()
    {
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (inputDir != Vector3.zero)
        {
            playerOBJ.forward = Vector3.Slerp(playerOBJ.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
    }
}