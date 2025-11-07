using UnityEngine;

/// <summary>
/// 게임 진행 상황(스테이지 클리어 여부)을 PlayerPrefs에 저장하고 관리하는 정적 클래스입니다.
/// </summary>
public static class GameProgressManager
{
    /// <summary>
    /// PlayerPrefs에 사용할 고유 키를 생성합니다.
    /// </summary>
    /// <param name="chapterIndex">챕터 인덱스 (0부터 시작)</param>
    /// <param name="stageID">StageData의 stageID (1부터 시작)</param>
    private static string GetStageKey(int chapterIndex, int stageID)
    {
        return $"Chapter_{chapterIndex}_Stage_{stageID}_Cleared";
    }

    /// <summary>
    /// 특정 스테이지를 클리어 상태로 저장합니다.
    /// </summary>
    public static void ClearStage(int chapterIndex, int stageID)
    {
        string key = GetStageKey(chapterIndex, stageID);
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        Debug.Log($"[GameProgress] 챕터 {chapterIndex}, 스테이지 {stageID} 클리어 저장됨 (Key: {key})");
    }

    /// <summary>
    /// 특정 스테이지가 클리어되었는지 확인합니다.
    /// </summary>
    public static bool IsStageCleared(int chapterIndex, int stageID)
    {
        string key = GetStageKey(chapterIndex, stageID);
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    /// <summary>
    /// [테스트용] 모든 진행 상황을 리셋합니다.
    /// </summary>
    public static void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[GameProgress] 모든 진행 상황이 리셋되었습니다.");
    }
}