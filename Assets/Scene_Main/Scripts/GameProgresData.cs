using UnityEngine;
using System.Collections.Generic;
using System.IO; // 파일 입출력
using System.Linq; // Dictionary 변환

// 게임의 모든 '동적 데이터'(세이브 파일)를 관리하는 싱글톤
public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    // --- 메모리 상의 데이터 (빠른 접근용) ---
    // (string stageID, int stars)
    private Dictionary<string, int> stageStars = new Dictionary<string, int>();
    // (int chapterIndex)
    private HashSet<int> unlockedCoreStones = new HashSet<int>();

    private string saveFilePath; // 저장 경로

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 저장 파일 경로 설정
            saveFilePath = Path.Combine(Application.persistentDataPath, "progress.json");

            LoadGame(); // 게임 시작 시 데이터 로드
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- 1. 데이터 업데이트 함수 ---

    /// <summary>
    /// 스테이지 클리어 시 호출되어 별 개수를 업데이트합니다. (가장 중요)
    /// </summary>
    /// <param name="stageID">"1-1" 등 StageData의 ID</param>
    /// <param name="starsEarned">획득한 별 개수 (1, 2, 3)</param>
    public void UpdateStageProgress(string stageID, int starsEarned)
    {
        // 기존 별보다 더 많이 획득했을 때만 업데이트
        if (stageStars.ContainsKey(stageID))
        {
            if (starsEarned > stageStars[stageID])
            {
                stageStars[stageID] = starsEarned;
            }
        }
        else
        {
            stageStars[stageID] = starsEarned;
        }

        Debug.Log($"[GameProgress] {stageID} 스테이지, 별 {starsEarned}개 획득 (최종 {stageStars[stageID]}개)");
        CheckForCoreStoneUnlock(stageID); // 챕터 올클리어 체크
    }

    // --- 2. 데이터 조회 함수 ---

    /// <summary>
    /// 챕터 잠금 해제 시 이 함수를 사용하여 별 개수를 확인합니다.
    /// </summary>
    public int GetStars(string stageID)
    {
        if (stageStars.ContainsKey(stageID))
        {
            return stageStars[stageID];
        }
        return 0; // 클리어하지 않았으면 0 반환
    }

    /// <summary>
    /// '핵심 돌' 획득 여부를 반환합니다.
    /// </summary>
    public bool HasCoreStone(int chapterIndex) // 챕터 인덱스 (예: 1, 2, 3, 4)
    {
        return unlockedCoreStones.Contains(chapterIndex);
    }

    // --- (내부) 챕터 올클리어 체크 ---
    private void CheckForCoreStoneUnlock(string lastCompletedStageID)
    {
        // 예: stageID가 "1-3"일 때, 이 챕터(1챕터)의 모든 스테이지를 체크
        // (이 부분은 모든 1챕터 StageData SO를 참조하여 자동화해야 함 - 지금은 생략)

        // (가정) 1챕터(1-1, 1-2, 1-3)를 모두 별 3개로 클리어했다면:
        // if (GetStars("1-1") == 3 && GetStars("1-2") == 3 && GetStars("1-3") == 3)
        // {
        //     unlockedCoreStones.Add(1);
        //     Debug.Log("!!! 1챕터 핵심 돌 획득 !!!");
        // }
    }


    // --- 3. 저장 및 로드 ---
    public void SaveGame()
    {
        ProgressData data = new ProgressData();

        // Dictionary를 List로 변환하여 저장
        data.completedStageIDs = stageStars.Keys.ToList();
        data.starsPerStage = stageStars.Values.ToList();
        data.unlockedCoreStoneChapters = unlockedCoreStones.ToList();

        string json = JsonUtility.ToJson(data, true); // JSON으로 변환
        File.WriteAllText(saveFilePath, json); // 파일로 저장
        Debug.Log($"게임 저장 완료: {saveFilePath}");
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            ProgressData data = JsonUtility.FromJson<ProgressData>(json); // JSON에서 로드

            // List를 Dictionary/HashSet으로 변환하여 메모리에 로드
            stageStars.Clear();
            for (int i = 0; i < data.completedStageIDs.Count; i++)
            {
                stageStars[data.completedStageIDs[i]] = data.starsPerStage[i];
            }

            unlockedCoreStones.Clear();
            foreach (int chapter in data.unlockedCoreStoneChapters)
            {
                unlockedCoreStones.Add(chapter);
            }

            Debug.Log("게임 로드 완료.");
        }
        else
        {
            Debug.Log("세이브 파일 없음. 새 게임 시작.");
            // 새 게임 데이터로 초기화 (기본값)
            stageStars = new Dictionary<string, int>();
            unlockedCoreStones = new HashSet<int>();
        }
    }
}