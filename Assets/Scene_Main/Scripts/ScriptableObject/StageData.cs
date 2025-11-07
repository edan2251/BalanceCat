using UnityEngine;
// StagePoint 스크립트와 일치시키기 위해 stageID를 포함합니다.

[CreateAssetMenu(fileName = "StageData_", menuName = "GameData/Stage Data", order = 1)]
public class StageData : ScriptableObject
{
    [Header("씬 로드 설정")]
    [Tooltip("씬로드할때 넘길 씬 이름. 챕터_ 스테이지")]
    public string targetSceneName = "1_ 1"; 

    // === 스테이지 식별 정보 ===
    [Tooltip("StagePoint 스크립트의 Stage ID와 일치해야 합니다 (1부터 시작).")]
    public int stageID = 1;

    // === UI 표시 정보 ===
    [Header("UI 표시 정보")]
    public string chapterName = "챕터 이름";
    public string stageTitle = "스테이지 이름";

    [TextArea(3, 10)]
    public string synopsis = "스테이지의 간략한 배경 이야기나 목표를 설명합니다.";

    public string rewardName = "이곳에 퀘스트 보상을 입력하시오";
    // 필요한 경우, 실제 보상 오브젝트나 아이콘을 위한 변수 추가 가능
    // public Sprite rewardIcon; 

    [Header("퀘스트 할당")]
    // 퀘스트 1은 "클리어"로 고정되므로, QuestData가 필요 없을 수도 있습니다.
    // 여기서는 퀘스트 2, 3만 SO로 할당하는 구조를 제안합니다.

    [Tooltip("퀘스트 1: 스테이지 클리어 (기본 제공)")]
    public QuestData MainQuest;

    [Tooltip("퀘스트 2: 도전 퀘스트 (SO 할당)")]
    public QuestData quest2;

    [Tooltip("퀘스트 3: 탐험 퀘스트 (SO 할당)")]
    public QuestData quest3;

}