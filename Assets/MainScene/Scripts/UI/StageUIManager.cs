using UnityEngine;
using UnityEngine.UI;

public class StageUIManager : MonoBehaviour
{
    // 인스펙터에 연결할 UI 요소들
    public Text chapterNameText;
    public Text stageTitleText;
    public Text stageSynopsisText;
    public Text questText;
    public Text rewardText;
    public Button startButton; // 시작 버튼

    // StageSelector가 호출할 메인 업데이트 함수
    public void UpdateStageInfo(StageData data)
    {
        if (data == null)
        {
            // 데이터가 null일 경우 UI를 숨기거나 초기화
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        // 텍스트 업데이트
        chapterNameText.text = data.chapterName;
        // "3 스테이지" 처럼 현재 스테이지 ID와 함께 표시   /*{data.stageID}*/
        stageTitleText.text = $"{data.stageTitle}";
        stageSynopsisText.text = data.synopsis;
        questText.text = $"{data.questDescription}";
        rewardText.text = $"{data.rewardName}";

        // 시작 버튼 이벤트 설정 (예시)
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() => StartStage(data));
    }

    private void StartStage(StageData data)
    {
        Debug.Log($"스테이지 시작: 챕터 {data.chapterName}, ID {data.stageID}");
        // 실제 씬 로딩 또는 게임 로직 호출
    }
}