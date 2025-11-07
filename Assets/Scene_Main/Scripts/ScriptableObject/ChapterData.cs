using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ChapterData_", menuName = "GameData/Chapter Data", order = 0)]
public class ChapterData : ScriptableObject
{
    [Tooltip("이 챕터(행성)에 포함된 모든 스테이지 데이터 리스트입니다.")]
    public List<StageData> stages;

    public string chapterName;
}