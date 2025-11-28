using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class InGameQuestManager : MonoBehaviour
{
    public static InGameQuestManager Instance;

    [Header("현재 스테이지 데이터")]
    public StageData currentStageData;

    [Header("UI 연결")]
    [Tooltip("퀘스트 1, 2, 3 순서대로 별 이미지")]
    public Image[] starImages; 

    [Header("별 스프라이트 소스")]
    [Tooltip("채워진 별 이미지")]
    public Sprite filledStarSprite;
    [Tooltip("비어있는 별 이미지")]
    public Sprite emptyStarSprite;

    private bool[] tempQuestCleared = new bool[3];

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (emptyStarSprite != null)
        {
            foreach (var img in starImages)
            {
                if (img != null)
                {
                    img.sprite = emptyStarSprite;
                    img.gameObject.SetActive(true); 
                }
            }
        }
    }

    void Update()
    {
        // === 테스트용 키 입력 ===
        if (Input.GetKeyDown(KeyCode.Alpha1)) SolveQuest(0); // 1번(메인) 
        if (Input.GetKeyDown(KeyCode.Alpha2)) SolveQuest(1); // 2번
        if (Input.GetKeyDown(KeyCode.Alpha3)) SolveQuest(2); // 3번
    }

    /// <summary>
    /// 퀘스트 조건을 달성했을 때 호출
    /// </summary>
    /// <param name="questIndex">0, 1, 2 (리스트 순서)</param>
    public void SolveQuest(int questIndex)
    {
        if (questIndex < 0 || questIndex >= 3) return;

        if (tempQuestCleared[questIndex]) return;

        tempQuestCleared[questIndex] = true;
        Debug.Log($"[인게임] {questIndex + 1}번 퀘스트 조건 달성!");

        if (starImages.Length > questIndex && starImages[questIndex] != null && filledStarSprite != null)
        {
            starImages[questIndex].sprite = filledStarSprite;
        }

        // 만약 깬 퀘스트가 0번(게임클리어면 저거실행)
        if (questIndex == 0)
        {
            FinishStage();
        }
    }

    private void FinishStage()
    {
        int chapterIdx = currentStageData.chapterIndex;
        int stageID = currentStageData.stageID;

        // 저장 로직
        for (int i = 0; i < 3; i++)
        {
            if (tempQuestCleared[i])
            {
                GameProgressManager.CompleteQuest(chapterIdx, stageID, i + 1);
            }
        }

        GameProgressManager.ClearStage(chapterIdx, stageID);

        // 잠시 후 메인으로 이동
        Invoke("GoToMainMenu", 2.0f);
    }

    void GoToMainMenu()
    {
        SceneManager.LoadScene("Main"); 
    }
}