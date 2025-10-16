using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteDirectionalController_Octo : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform mainTransform; 
    [SerializeField] Animator animator;

    [Header("Player Input Reference")]
    [SerializeField] PlayerMovement playerMovement;

    // === Idle 시간 추적을 위한 변수 추가 ===
    private float currentIdleTime = 0f;
    //private const float MAX_IDLE_DURATION = 2.0f;

    private void Update()
    {
        // 1. 이동 여부 판단 및 Idle 시간 관리 (기존 로직)
        bool isMoving = playerMovement.FlatSpeed > 0.1f;

        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            currentIdleTime = 0f;
        }
        else
        {
            currentIdleTime += Time.deltaTime;
        }
        animator.SetFloat("IdleTime", currentIdleTime);

        // 2. 점프/공중 상태 체크
        bool isJumping = !playerMovement.IsGrounded;

        // Animator에 isJumping 파라미터 전달
        animator.SetBool("isJumping", isJumping); 
    }

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

        //Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        //animator.SetBool("isMoving", flatVel.magnitude > 0.1f);

    }
}
