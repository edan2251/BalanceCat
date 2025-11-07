using UnityEngine;

[CreateAssetMenu(fileName = "QuestData_", menuName = "GameData/Quest Data", order = 2)]
public class QuestData : ScriptableObject
{
    [Tooltip("UI에 표시될 퀘스트 제목 (예: 스테이지 클리어)")]
    public string questTitle = "퀘스트 제목";

    [TextArea(2, 5)]
    [Tooltip("UI에 표시될 퀘스트 상세 설명")]
    public string questDescription = "퀘스트 설명을 입력하세요.";

    // 필요시 아이콘 등 추가 가능
    // public Sprite questIcon;
}