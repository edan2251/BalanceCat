using UnityEngine;

// 퀘스트의 "설계도" 역할을 하는 ScriptableObject입니다.
// 예: "1-1_Quest_2" (타임어택 퀘스트), "1-1_Quest_3" (수집 퀘스트)
[CreateAssetMenu(fileName = "QuestData_", menuName = "GameData/Quest Data", order = 2)]
public class QuestData : ScriptableObject
{
    [Header("퀘스트 정보")]
    public string questName = "퀘스트 이름";

    [TextArea(2, 5)]
    public string description = "퀘스트 상세 설명";

    // 퀘스트 타입을 정의하여 퀘스트 매니저가 이 퀘스트를 어떻게 처리할지 구분
    public enum QuestType
    {
        ClearStage, // (기본 퀘스트 1)
        TimeAttack, // (퀘스트 2 예시)
        NoDamage,
        FindCollectibles, // (퀘스트 3 예시)
        UseSpecificMechanic
    }

    public QuestType questType;

    [Header("퀘스트 조건 (선택 사항)")]
    // 퀘스트 타입에 따라 사용하는 값 (예: TimeAttack의 제한 시간)
    public float conditionValue = 60.0f;
}