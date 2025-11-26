using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MessageTriggerZone : MonoBehaviour
{
    [Header("메시지 설정")]
    [TextArea(3, 10)] // 인스펙터에서 여러 줄 입력 가능하게
    public string messageToSend = "여기에 표시할 메시지를 입력하세요.";

    [Tooltip("메시지가 떠있을 시간 (초)")]
    public float displayDuration = 10f;

    private bool _hasTriggered = false; // 이미 작동했는지 체크하는 플래그

    void Awake()
    {
        // 콜라이더를 강제로 트리거로 설정
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            // [수정] (true)를 넣으면 꺼져있는(Inactive) 자식 오브젝트까지 다 뒤져서 찾아냅니다!
            SpeechBubbleController bubbleCtrl = other.GetComponentInChildren<SpeechBubbleController>(true);

            if (bubbleCtrl != null)
            {
                bubbleCtrl.ShowMessage(messageToSend, displayDuration);
                _hasTriggered = true;
                // Debug.Log 삭제 또는 주석 처리
            }
            else
            {
                Debug.LogWarning("플레이어에게 SpeechBubbleController가 없습니다! (혹시 자식 오브젝트에 스크립트 붙이는 걸 깜빡했나요?)");
            }
        }
    }
}