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

    [Header("Jump Control")]
    [Tooltip("���� ��� �� �߷¿� �߰��� ���� (1.0�̸� �⺻ �߷�)")]
    public float ascentMultiplier = 1.5f;
    [Tooltip("���� �ϰ� �� �߷¿� �߰��� ���� (1.0�̸� �⺻ �߷�)")]
    public float descentMultiplier = 1.5f;


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

    [Header("Control/Links")]
    [SerializeField] InventorySideBias sideBias;
    bool _controlEnabled = true;

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
        if (!_controlEnabled)
        {
            sideBias?.SetCorrectionInput(0f); // ��� �� ���� ����
            return;                           // �Է� ó�� ����(�߷�/������ ���� FixedUpdate�� �ñ�)
        }

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        float capsuleRadius = capsule != null ? capsule.radius : 0.5f;

        Vector3 footPosition = transform.position + Vector3.down * (playerHeight - capsuleRadius);

        Vector3 boxCenter = footPosition + Vector3.up * 0.01f;

        Vector3 boxHalfExtents = new Vector3(groundCheckExtent, 0.05f, groundCheckExtent);

        grounded = Physics.BoxCast(
            boxCenter,
            boxHalfExtents,
            Vector3.down,
            Quaternion.identity,
            groundCheckMargin, 
            whatIsGround
        );
        
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

        ApplyVariableGravity();
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        bool isRunningInput = Input.GetKey(runKey);
        float speedMultiplier = isRunningInput ? runMultiplier : 1.0f;
        currentMoveSpeed = moveSpeed * speedMultiplier;

        float corr = 0f;
        if (Input.GetKey(KeyCode.Z)) corr -= 1f;
        if (Input.GetKey(KeyCode.C)) corr += 1f;
        sideBias?.SetCorrectionInput(corr);

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

        float adjustedJumpForce = jumpForce * ascentMultiplier;
        rb.AddForce(transform.up * adjustedJumpForce, ForceMode.Impulse);

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

    private void ApplyVariableGravity()
    {
        // ���� ���� ���� �������� ����
        if (grounded) return;

        float gravityScale;

        if (rb.velocity.y > 0)
        {
            // ��� ��: ascentMultiplier ����
            gravityScale = ascentMultiplier;
        }
        else
        {
            // �ϰ� ��: descentMultiplier ����
            gravityScale = descentMultiplier;
        }

        // �⺻ �߷�(Physics.gravity)�� �̹� ����ǰ� �����Ƿ�,
        // (���� - 1) ��ŭ �߰����� �߷� ���� ���մϴ�.
        if (gravityScale > 1.0f)
        {
            float additionalGravity = Physics.gravity.y * (gravityScale - 1) * rb.mass;
            rb.AddForce(Vector3.up * additionalGravity, ForceMode.Force);
        }
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
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        float capsuleRadius = capsule != null ? capsule.radius : 0.5f;

        Vector3 footPosition = transform.position + Vector3.down * (playerHeight - capsuleRadius);
        Vector3 boxCenter = footPosition + Vector3.up * 0.01f;
        Vector3 boxHalfExtents = new Vector3(groundCheckExtent, 0.05f, groundCheckExtent);
        Vector3 boxSize = boxHalfExtents * 2f;

        if (Application.isPlaying)
        {
            Gizmos.color = grounded ? Color.green : Color.red;
        }
        else
        {
            Gizmos.color = Color.cyan;
        }

        Gizmos.DrawWireCube(boxCenter, boxSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boxCenter + Vector3.down * groundCheckMargin, boxSize);
    }

    public void SetControlEnabled(bool v) 
    { 
        _controlEnabled = v;
    }
}