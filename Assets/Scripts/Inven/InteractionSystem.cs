using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.UI; // 더 이상 필요 없음

public class InteractionSystem : MonoBehaviour
{
    [Header("상호 작용 설정")]
    public float interactionRange = 2.0f;
    public LayerMask interactionLayerMask = 1;
    public KeyCode interactionKey = KeyCode.E;

    [Header("UI 연결")]
    // [신규] 말풍선 컨트롤러 연결 (Inspector에서 할당하거나 Start에서 자동 찾기)
    public SpeechBubbleController speechBubble;

    private Transform playerTransform;
    private InteractableObject currentInteractable;

    private void Start()
    {
        playerTransform = transform;

        // 만약 인스펙터에서 연결 안 했다면 자식 오브젝트에서 찾기
        if (speechBubble == null)
        {
            speechBubble = GetComponentInChildren<SpeechBubbleController>();
        }

        HideInteractionUI();
    }

    private void Update()
    {
        CheckForInteractables();
        HandleInteractionInput();
    }

    void HandleInteractionInput()
    {
        if (currentInteractable != null && Input.GetKeyDown(interactionKey))
        {
            var t = currentInteractable.interactionType;
            currentInteractable.Interact();

            if (t == InteractableObject.InteractionType.Item)
            {
                //아이템 획득
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(SFX.Pickup, 2f);
                }

                // 아이템을 먹었으면 UI 즉시 숨김
                HideInteractionUI();
                currentInteractable = null;
            }
        }
    }

    // [수정] 기존 UI 대신 말풍선 함수 호출
    void ShowInteractionUI(string text)
    {
        if (speechBubble != null)
        {
            // 예: "[E] 상호작용" 형태로 표시
            speechBubble.ShowContinuousMessage($"[{interactionKey}] 줍기");
        }
    }

    // [수정] 말풍선 숨기기 호출
    void HideInteractionUI()
    {
        if (speechBubble != null)
        {
            // true를 넣어 부드럽게 페이드 아웃
            speechBubble.HideContinuousMessage(true);
        }
    }

    void CheckForInteractables()
    {
        Vector3 checkPosition = playerTransform.position + playerTransform.forward * (interactionRange * 0.5f);

        Collider[] hitColliders = Physics.OverlapSphere(checkPosition, interactionRange, interactionLayerMask);

        InteractableObject closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (Collider collider in hitColliders)
        {
            // 부모나 자식에 있을 수도 있으니 InParent/InChildren 고려 가능 (지금은 그대로 유지)
            InteractableObject interactable = collider.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                float distance = Vector3.Distance(playerTransform.position, collider.transform.position);

                Vector3 directionToObect = (collider.transform.position - playerTransform.position).normalized;
                float angle = Vector3.Angle(playerTransform.forward, directionToObect);

                if (angle < 90f && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        // 대상이 바뀌었거나 새로 생겼을 때
        if (closestInteractable != currentInteractable)
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnPlayerExit();
            }

            currentInteractable = closestInteractable;

            if (currentInteractable != null)
            {
                currentInteractable.OnPlayerEnter();
                // 여기서 ShowInteractionUI 호출 -> 말풍선 켜짐
                ShowInteractionUI(currentInteractable.GetInteractionText());
            }
            else
            {
                // 대상이 없어지면 말풍선 꺼짐
                HideInteractionUI();
            }
        }
    }
}