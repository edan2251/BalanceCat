using System.Collections.Generic;

// 이 클래스는 MonoBehaviour가 아니며, 오직 저장을 위해 사용됩니다.
[System.Serializable]
public class ProgressData
{
    // 1. 스테이지별 획득한 별의 개수
    // (Dictionary는 Unity의 JsonUtility가 직접 시리얼라이즈할 수 없으므로, List 두 개로 관리)
    public List<string> completedStageIDs;
    public List<int> starsPerStage;

    // 2. 획득한 '핵심 돌'
    public List<int> unlockedCoreStoneChapters;

    // 3. 마지막으로 선택한 챕터 (보너스)
    public int lastSelectedChapter;

    // 생성자 (초기화)
    public ProgressData()
    {
        completedStageIDs = new List<string>();
        starsPerStage = new List<int>();
        unlockedCoreStoneChapters = new List<int>();
        lastSelectedChapter = 0; // 0번 인덱스 (1챕터)
    }
}