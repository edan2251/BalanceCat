using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpriteDirectionalController_Octo : MonoBehaviour
{
    [SerializeField] Transform mainTransform;
    [SerializeField] Animator animator;

    private void LateUpdate()
    {
        Vector3 directionToCamera = Camera.main.transform.position - mainTransform.position;
        directionToCamera.y = 0f;

        if (directionToCamera.sqrMagnitude > 0.001f)
        {
            directionToCamera.Normalize();
        }
        else
        {
            return;
        }

        Vector3 localDirection = mainTransform.InverseTransformDirection(directionToCamera);

        float viewDirX = localDirection.x;

        float viewDirY = localDirection.z;


        animator.SetFloat("MoveX", viewDirX);
        animator.SetFloat("MoveY", viewDirY);


    }
}
