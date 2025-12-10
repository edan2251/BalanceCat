using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MessageTriggerZone : MonoBehaviour
{
    [Header("메시지 설정")]
    [TextArea(3, 10)]
    public string messageToSend = "여기에 표시할 메시지를 입력하세요.";

    [Tooltip("메시지가 떠있을 시간 (초)")]
    public float displayDuration = 10f;

    private bool _hasTriggered = false;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            SpeechBubbleController bubbleCtrl = other.GetComponentInChildren<SpeechBubbleController>(true);

            if (bubbleCtrl != null)
            {
                // [핵심 수정] isPriority = true로 설정하여 호출!
                // 이렇게 하면 아이템 획득 메시지(기본값 false)가 이 메시지를 덮어쓰지 못합니다.
                // 하지만 다른 TriggerZone이 true로 호출하면 덮어쓸 수 있습니다.
                bubbleCtrl.ShowMessage(messageToSend, displayDuration, true);

                _hasTriggered = true;
            }
            else
            {
                // Debug.LogWarning(...)
            }
        }
    }
}