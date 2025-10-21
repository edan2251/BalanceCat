using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float runMultiplier = 3.0f;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;


    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode runKey = KeyCode.LeftShift;

    // --- ���� üũ ���� ---
    [Header("Ground Check")]
    [Tooltip("�÷��̾� �ݶ��̴��� ���� ���ݰ� (������)")]
    public float playerHeight = 1.0f;
    [Tooltip("���� üũ �ڽ��� ����/���� ũ�� (���� ũ��)")]
    public float groundCheckExtent = 0.4f;
    [Tooltip("���� üũ �ڽ��� �Ʒ��� ��� �߰� �Ÿ� (������ ����)")]
    public float groundCheckMargin = 0.1f;
    public LayerMask whatIsGround;
    bool grounded;

    // SpriteDirectionalController_Octo ���� �����ϴ� �Ӽ�
    [HideInInspector] public bool IsGrounded => grounded;
    [HideInInspector] public float FlatSpeed => new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;

    [Header("References")]
    public Transform orientation;
    public Rigidbody rb;

    [Header("Animator")]
    public Animator playerAnimator;

    [HideInInspector] public float horizontalInput;
    [HideInInspector] public float verticalInput;

    Vector3 moveDirection;
    float currentMoveSpeed;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        if (rb == null) rb = GetComponent<Rigidbody>();

        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        // ----------------------------------------------------
        // BoxCast�� ����� ���� üũ
        // ----------------------------------------------------

        // ĸ�� �ݶ��̴��� �߹ٴ� ��ó�� �߽����� ����
        Vector3 boxCenter = transform.position + Vector3.down * (playerHeight - 0.05f);

        // üũ �ڽ��� ���� ũ�� 
        Vector3 boxHalfExtents = new Vector3(groundCheckExtent, 0.05f, groundCheckExtent);

        // BoxCast ����: �ڽ��� �Ʒ��� ���� ���� ���̾ ����� Ȯ��
        grounded = Physics.BoxCast(
            boxCenter,
            boxHalfExtents,
            Vector3.down,
            Quaternion.identity,
            groundCheckMargin, // �ִ� �˻� �Ÿ� (margin��ŭ �� ���� ������ Ȯ��)
            whatIsGround
        );
        // ----------------------------------------------------


        GetInput();
        SpeedControl();

        UpdateAnimator();

        // ���鿡 ���� ���� �巡�� ����
        rb.drag = grounded ? groundDrag : 0f;
    }

    private void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        bool isRunningInput = Input.GetKey(runKey);
        float speedMultiplier = isRunningInput ? runMultiplier : 1.0f;
        currentMoveSpeed = moveSpeed * speedMultiplier;

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        // ī�޶� ���⿡ �°� �̵� ���� ���
        Vector3 camForward = new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;
        Vector3 camRight = new Vector3(orientation.right.x, 0f, orientation.right.z).normalized;
        moveDirection = (camForward * verticalInput + camRight * horizontalInput).normalized;

        if (grounded)
            rb.AddForce(moveDirection * currentMoveSpeed * 10f, ForceMode.Force);
        else
            // ���߿����� airMultiplier�� ���Ͽ� �̵� ����
            rb.AddForce(moveDirection * currentMoveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void RotatePlayer()
    {
        Vector3 flatInput = new Vector3(horizontalInput, 0f, verticalInput);
        if (flatInput.magnitude >= 0.1f)
            transform.forward = moveDirection; // �Է� �������� ȸ��
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > currentMoveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentMoveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // Y�� �ӵ��� 0���� �ʱ�ȭ�Ͽ� ���� ���̸� �ϰ��� �ְ� ����ϴ�.
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        grounded = false; // ���� �� ������ ���� ���� ����

        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("JumpTrigger");
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }


    private void UpdateAnimator()
    {
        if (playerAnimator == null) return;

        bool isMoving = horizontalInput != 0 || verticalInput != 0;
        bool isRunning = Input.GetKey(runKey) && isMoving;
        playerAnimator.SetBool("isRunning", isRunning);

    }


    private void OnDrawGizmos()
    {
        Vector3 boxCenter = transform.position + Vector3.down * (playerHeight - 0.05f);
        Vector3 boxHalfExtents = new Vector3(groundCheckExtent, 0.05f, groundCheckExtent);
        Vector3 boxSize = boxHalfExtents * 2f;
        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(boxCenter, boxSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boxCenter + Vector3.down * groundCheckMargin, boxSize);
    }
}