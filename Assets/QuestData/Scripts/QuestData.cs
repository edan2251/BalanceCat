using UnityEngine;

// 퀘스트 종류를 정의합니다.
public enum QuestType
{
    General,        // 일반 (그냥 깨면 됨)
    TimeLimit,      // 시간 제한 (targetValue초 이내 클리어)
    Delivery,       // 아이템 배달 (나중에 구현)
    NoFall,         // 안 넘어지기 (나중에 구현)
    NoRespawn,      // 노 데스 (나중에 구현)
    InventoryLimit  // 가방 정리 (나중에 구현)
}

[CreateAssetMenu(fileName = "QuestData_", menuName = "GameData/Quest Data", order = 2)]
public class QuestData : ScriptableObject
{
    [Header("기본 정보")]
    public string questTitle;
    [TextArea(2, 5)]
    public string questDescription;

    [Header("퀘스트 조건 설정")]
    [Tooltip("이 퀘스트의 종류를 선택하세요.")]
    public QuestType type;

    [Tooltip("목표 값 (시간 제한일 경우 '초' 단위, 아이템일 경우 '개수' 등)")]
    public float targetValue;

    [Header("Delivery Quest 전용")]
    public string requiredItemID;
}