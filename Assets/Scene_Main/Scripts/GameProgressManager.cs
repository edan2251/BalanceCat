using UnityEngine;

/// <summary>
/// 게임 진행 상황(스테이지 클리어 및 퀘스트 완료 여부)을 PlayerPrefs에 저장하고 관리하는 정적 클래스입니다.
/// </summary>
public static class GameProgressManager
{
    // === 스테이지 클리어 (잠금 해제용) ===

    /// <summary>
    /// PlayerPrefs에 사용할 스테이지 클리어(잠금해제) 키를 생성합니다.
    /// </summary>
    private static string GetStageKey(int chapterIndex, int stageID)
    {
        return $"Chapter_{chapterIndex}_Stage_{stageID}_Cleared";
    }

    /// <summary>
    /// 특정 스테이지를 '클리어' 상태로 저장합니다. (다음 스테이지 잠금 해제용)
    /// </summary>
    public static void ClearStage(int chapterIndex, int stageID)
    {
        string key = GetStageKey(chapterIndex, stageID);
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        Debug.Log($"[GameProgress] 잠금 해제: 챕터 {chapterIndex}, 스테이지 {stageID} (Key: {key})");
    }

    /// <summary>
    /// 특정 스테이지가 '클리어'되었는지 확인합니다. (다음 스테이지 잠금 해제용)
    /// </summary>
    public static bool IsStageCleared(int chapterIndex, int stageID)
    {
        string key = GetStageKey(chapterIndex, stageID);
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    // === 퀘스트 완료 (별 획득용) ===

    /// <summary>
    /// PlayerPrefs에 사용할 퀘스트 완료(별) 키를 생성합니다.
    /// </summary>
    /// <param name="questIndex">퀘스트 인덱스 (1, 2, 3)</param>
    private static string GetQuestKey(int chapterIndex, int stageID, int questIndex)
    {
        return $"Chapter_{chapterIndex}_Stage_{stageID}_Quest_{questIndex}_Completed";
    }

    /// <summary>
    /// 특정 퀘스트(별)를 '완료' 상태로 저장합니다.
    /// </summary>
    /// <param name="questIndex">퀘스트 인덱스 (1, 2, 3)</param>
    public static void CompleteQuest(int chapterIndex, int stageID, int questIndex)
    {
        if (questIndex < 1 || questIndex > 3)
        {
            Debug.LogError($"잘못된 퀘스트 인덱스: {questIndex}");
            return;
        }
        string key = GetQuestKey(chapterIndex, stageID, questIndex);
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        Debug.Log($"[GameProgress] 퀘스트(별) 획득: 챕터 {chapterIndex}, 스테이지 {stageID}, 퀘스트 {questIndex} (Key: {key})");
    }

    /// <summary>
    /// 특정 퀘스트(별)가 '완료'되었는지 확인합니다.
    /// </summary>
    /// <param name="questIndex">퀘스트 인덱스 (1, 2, 3)</param>
    public static bool IsQuestCompleted(int chapterIndex, int stageID, int questIndex)
    {
        if (questIndex < 1 || questIndex > 3) return false;
        string key = GetQuestKey(chapterIndex, stageID, questIndex);
        return PlayerPrefs.GetInt(key, 0) == 1;
    }


    /// <summary>
    /// [테스트용] 모든 진행 상황을 리셋합니다.
    /// </summary>
    public static void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}