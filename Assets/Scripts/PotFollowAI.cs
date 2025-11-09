using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetFollowAI : MonoBehaviour
{
    public Transform player;  // 플레이어의 Transform
    public float followSpeed = 5f;  // 따라오는 속도
    public float heightOffset = 2f; //플레이어 위쪽으로 유지할 높이
    public float followDistance = 2f;   // 플레이어와의 거리 유지
    public float smoothness = 3f;  //움직임 부드럽게

    private Vector3 targetPos;

    private void Update()
    {
        if (player == null) return;

        targetPos = player.position + (-player.forward * followDistance) + (Vector3.up * heightOffset);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothness);

        transform.LookAt(player.position + Vector3.up * 1.5f);

    }
}
