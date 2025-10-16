using UnityEngine;

public class SpirteDirectionalController : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform mainTransform;
    [SerializeField] Animator animator;

    [Header("Angle Settings")]
    [SerializeField] float backAngle = 65f;
    [SerializeField] float sideAngle = 155f;

    [Header("Player Input Reference")]
    [SerializeField] PlayerMovement playerMovement;

    private void LateUpdate()
    {
        float horizontalInput = playerMovement.horizontalInput;
        float verticalInput = playerMovement.verticalInput;

        // ī�޶�� ĳ���� ����
        Vector3 camForward = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);
        Vector3 charForward = mainTransform.forward;
        float signedAngle = Vector3.SignedAngle(charForward, camForward, Vector3.up);
        float angle = Mathf.Abs(signedAngle);

        // �밢�� �Է� üũ
        bool isDiagonal = Mathf.Abs(horizontalInput) > 0 && Mathf.Abs(verticalInput) > 0;

        // �ִϸ��̼� ���� ����
        Vector2 animationDirection = new Vector2(0f, -1f); // �⺻ Front
        if (angle < backAngle)
        {
            animationDirection = new Vector2(0f, -1f); // Front
        }
        else if (angle < sideAngle)
        {
            if (isDiagonal)
                animationDirection = verticalInput > 0 ? new Vector2(0f, -1f) : new Vector2(0f, 1f); // �밢�� �� ��/��
            else
                animationDirection = signedAngle < 0 ? new Vector2(-1f, 0f) : new Vector2(1f, 0f); // ��/��
        }
        else
        {
            animationDirection = new Vector2(0f, 1f); // Back
        }

        // BlendTree �Ķ���� ����
        animator.SetFloat("MoveX", animationDirection.x);
        animator.SetFloat("MoveY", animationDirection.y);

        // �̵� ���� �Ǵ�
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        animator.SetBool("isMoving", flatVel.magnitude > 0.1f);
    }
}