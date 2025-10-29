// SafeRangeFollower.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeRangeFollower : MonoBehaviour
{
    [Header("Links")]
    public Transform player;                 // ����� ��Ʈ(���� Pivot)
    public Transform orientation;            // PlayerMovement���� ���� �� orientation
    public PlayerMovement movement;          // ������ �Է�/�ӵ� ����
    public Rigidbody playerRb;               // [ADD] ����(���� �帮��Ʈ) ����

    [Header("Speed Model")]
    public float baseSpeed = 5f;             // movement ���� �� ���
    public bool useRunKey = true;            // Shift ���� �ݿ�
    public bool useAirMultiplier = true;     // [KEEP] �ʿ�� �״�� ���
    public bool ignoreAirMul = true;         // [ADD] ���߿����� ���� �ӵ��� ����(airMultiplier ����)

    [Header("Y Align")]
    public float yOffset = 0.02f;            // �� ��¦ ����
    public bool followPlayerY = true;        // �÷��̾� ���� ����

    [Header("Bias Drift Removal")]
    public bool subtractSideDrift = true;    // [ADD] ��(��/�� ���� �ӵ�)�� �������� ����
    public Transform facingForSide;          // [ADD] ��/�� �� ����(������ orientation ���)
    public float sideDriftScale = 1f;        // [ADD] ���� ����(1=��������)

    Vector3 _pos;
    public enum FollowUpdateMode { Update, FixedUpdate }
    [SerializeField] FollowUpdateMode updateMode = FollowUpdateMode.Update;

    void Awake()
    {
        if (!player) player = movement ? movement.transform : null;
        if (!playerRb && player) playerRb = player.GetComponent<Rigidbody>();  // [ADD]
        if (!facingForSide) facingForSide = orientation;                       // [ADD]
        _pos = (player ? player.position : transform.position);
        transform.position = _pos;
    }

    void Update()
    {
        if (updateMode == FollowUpdateMode.Update) Tick(Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (updateMode == FollowUpdateMode.FixedUpdate) Tick(Time.fixedDeltaTime);
    }

    void Tick(float dt)
    {
        if (!orientation) return;

        // 1) ���� �Է�
        float h, v;
        if (movement)
        {
            h = movement.horizontalInput;
            v = movement.verticalInput;
        }
        else
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }

        Vector3 dir = Vector3.zero;
        if (Mathf.Abs(h) + Mathf.Abs(v) > 0f)
        {
            Vector3 fwd = new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;
            Vector3 rgt = new Vector3(orientation.right.x, 0f, orientation.right.z).normalized;
            dir = (fwd * v + rgt * h).normalized;
        }

        // 2) ���� �ӵ�(���߿����� ������ �ʰ�)
        float spd = movement ? movement.moveSpeed : baseSpeed;
        if (useRunKey && movement && Input.GetKey(KeyCode.LeftShift)) spd *= movement.runMultiplier;

        if (movement && !movement.IsGrounded)
        {
            // [FIX] �⺻������ ���߿����� ���� �ӵ��� ����
            if (!ignoreAirMul && useAirMultiplier)
                spd *= Mathf.Max(0.01f, movement.airMultiplier); // ���������� ������
            // ignoreAirMul=true �� ����� ���� �ӵ� ����
        }

        // 3) �Է� ��� ����
        _pos += dir * spd * dt;

        // 4) �� ���� �帮��Ʈ ����(�ɼ�)
        if (subtractSideDrift && playerRb)
        {
            Transform sideRef = facingForSide ? facingForSide : orientation;
            Vector3 rightAxis = Vector3.ProjectOnPlane(sideRef.right, Vector3.up).normalized;
            Vector3 sideV = Vector3.Project(playerRb.velocity, rightAxis);
            _pos -= sideV * sideDriftScale * dt; // ��/��� �и��� ��ŭ ����
        }

        // 5) Y ����
        if (followPlayerY && player) _pos.y = player.position.y + yOffset;
        else _pos.y = (player ? player.position.y : _pos.y) + yOffset;

        // 6) ����
        transform.position = _pos;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
}
