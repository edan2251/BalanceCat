using System.Collections.Generic;
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
    public int chapterIndex = 0;

    // === UI 표시 정보 ===
    [Header("UI 표시 정보")]
    public string chapterName = "챕터 이름";
    public string stageTitle = "스테이지 이름";

    [TextArea(3, 10)]
    public string synopsis = "스테이지의 간략한 배경 이야기나 목표를 설명합니다.";

    [Header("로딩 화면 설정")]
    [Tooltip("로딩 화면에 띄울 한 컷 만화 이미지")]
    public Sprite loadingComicImage;

    [TextArea(2, 5)]
    [Tooltip("로딩 화면에 띄울 설명 텍스트")]
    public string loadingDescription = "로딩 중에 표시할 팁이나 스토리 텍스트";


    [Header("퀘스트 할당")]
    public List<QuestData> quests = new List<QuestData>();

}