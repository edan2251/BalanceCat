using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using System.Collections;

public class InGameQuestManager : MonoBehaviour
{
    public static InGameQuestManager Instance;

    [Header("--- 데이터 ---")]
    public StageData currentStageData;

    [Tooltip("다음 스테이지의 데이터 SO를 여기에 넣어주세요!")]
    public StageData nextStageData;

    [Header("--- 퀘스트 UI 연결 ---")]
    [Tooltip("Canvas에 있는 QuestDisplayManager가 붙은 오브젝트 연결")]
    public QuestDisplayManager questPanelController; // [추가됨]

    [Header("--- 커서 제어 ---")]
    public ShowCursor cursorController;

    [Header("--- 인게임 HUD (상단 작게 표시되는) ---")]
    public Image[] hudStarImages;
    public Sprite filledStarSprite;
    public Sprite emptyStarSprite;

    [Tooltip("인게임 화면 상단에 시간을 표시할 TMP를 연결하세요")]
    public TextMeshProUGUI hudPlayTimeText;

    [Header("--- 결과창 UI ---")]
    public GameObject resultPanel;
    public TextMeshProUGUI timeText;
    public Image[] resultStarFills;
    public Image resultBgImage;
    public TextMeshProUGUI resultTitleText;

    [Header("--- 버튼 연결 ---")]
    public Button restartBtn;
    public Button mainBtn;
    public Button nextBtn;

    private int respawnCount = 0;
    private int fallCount = 0;

    private bool[] tempQuestCleared = new bool[3];
    private float playTime = 0f;
    private bool isGameActive = true;

    void Awake() { Instance = this; }

    void Start()
    {
        Time.timeScale = 1f;
        isGameActive = true;
        playTime = 0f;
        resultPanel.SetActive(false);

        // 상단 HUD 별 초기화
        if (emptyStarSprite != null)
        {
            foreach (var img in hudStarImages) if (img != null) img.sprite = emptyStarSprite;
        }

        // 결과창 별 초기화
        foreach (var star in resultStarFills) if (star != null) star.gameObject.SetActive(false);

        restartBtn.onClick.AddListener(OnRestartClicked);
        mainBtn.onClick.AddListener(OnMainClicked);
        nextBtn.onClick.AddListener(OnNextClicked);

        RefreshQuestUI();
    }

    void Update()
    {
        if (isGameActive)
        {
            playTime += Time.deltaTime;

            // [추가] 실시간 시간 표시 업데이트 (00:00 형식)
            if (hudPlayTimeText != null)
            {
                int m = Mathf.FloorToInt(playTime / 60F);
                int s = Mathf.FloorToInt(playTime % 60F);
                hudPlayTimeText.text = string.Format("{0:00}:{1:00}", m, s);
            }
        }

        // 테스트용: 1번 키 누르면 메인 클리어 시도
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // 메인 퀘스트(0번) 클리어 시도 -> 이때 서브 퀘스트 체크도 같이 함
            CheckClearCondition();
        }
    }

    public void RefreshQuestUI()
    {
        if (questPanelController == null || currentStageData == null) return;

        for (int i = 0; i < currentStageData.quests.Count; i++)
        {
            if (i >= 3) break;

            QuestData q = currentStageData.quests[i];
            if (q == null) continue;

            string descText = $"{q.questTitle}\n{q.questDescription}";
            bool isStarOn = false;

            // 1. 메인 퀘스트
            if (i == 0) isStarOn = tempQuestCleared[0];

            // 2. 시간 제한
            else if (q.type == QuestType.TimeLimit) isStarOn = tempQuestCleared[i];

            // 3. 리스폰 제한 (수정됨!)
            else if (q.type == QuestType.NoRespawn)
            {
                descText += $" <color=yellow>({respawnCount} / {q.targetValue})</color>";

                // [변경] <= (이하)를 < (미만)으로 변경
                // 이제 respawnCount가 targetValue와 같아지는 순간(1/1) 별이 꺼집니다.
                if (respawnCount < q.targetValue) isStarOn = true;
                else isStarOn = false;
            }

            // 4. 넘어짐 제한 (수정됨!)
            else if (q.type == QuestType.NoFall)
            {
                descText += $" <color=yellow>({fallCount} / {q.targetValue})</color>";

                // [변경] <= (이하)를 < (미만)으로 변경
                if (fallCount < q.targetValue) isStarOn = true;
                else isStarOn = false;
            }

            questPanelController.UpdateQuestRealtime(i, descText, isStarOn);
        }
    }

    public void OnPlayerRespawn()
    {
        respawnCount++;
        Debug.Log($"[퀘스트] 리스폰 {respawnCount}회");
        RefreshQuestUI(); // 즉시 UI 반영 (별 꺼지거나 숫자 증가)
    }

    public void OnPlayerFall()
    {
        fallCount++;
        Debug.Log($"[퀘스트] 넘어짐 {fallCount}회");
        RefreshQuestUI(); // 즉시 UI 반영
    }



    public void CheckClearCondition()
    {
        SolveQuest(0);

        if (currentStageData != null)
        {
            for (int i = 1; i < currentStageData.quests.Count; i++)
            {
                QuestData q = currentStageData.quests[i];
                if (q == null) continue;

                bool isSuccess = false;

                if (q.type == QuestType.TimeLimit)
                {
                    if (playTime <= q.targetValue) isSuccess = true;
                }

                // [변경] 리스폰 제한 조건 수정
                else if (q.type == QuestType.NoRespawn)
                {
                    // 목표치보다 작아야 성공 (같으면 실패)
                    if (respawnCount < q.targetValue) isSuccess = true;
                }

                // [변경] 넘어짐 제한 조건 수정
                else if (q.type == QuestType.NoFall)
                {
                    // 목표치보다 작아야 성공 (같으면 실패)
                    if (fallCount < q.targetValue) isSuccess = true;
                }

                if (isSuccess) SolveQuest(i);
            }
        }
    }

    public void SolveQuest(int questIndex)
    {
        if (questIndex < 0 || questIndex >= 3) return;
        if (tempQuestCleared[questIndex]) return;

        tempQuestCleared[questIndex] = true;

        if (hudStarImages.Length > questIndex && hudStarImages[questIndex] != null)
        {
            hudStarImages[questIndex].sprite = filledStarSprite;
            hudStarImages[questIndex].transform.DOPunchScale(Vector3.one * 0.5f, 0.3f);
        }

        // 클리어 시에는 '멋지게 5초간 보여주기'만 호출 (이미 실시간으로 별은 켜져 있었을 테니)
        if (questPanelController != null)
        {
            questPanelController.ShowQuestClearSequence(questIndex, 5.0f);
        }

        if (questIndex == 0) FinishStage();
    }

    private void FinishStage()
    {
        if (!isGameActive) return;
        isGameActive = false;

        Debug.Log("!!! 스테이지 클리어 !!!");

        int chapterIdx = currentStageData.chapterIndex;
        int stageID = currentStageData.stageID;

        for (int i = 0; i < 3; i++)
        {
            if (tempQuestCleared[i])
                GameProgressManager.CompleteQuest(chapterIdx, stageID, i + 1);
        }
        GameProgressManager.ClearStage(chapterIdx, stageID);

        StartCoroutine(ShowResultRoutine());
    }

    // ... (이하 결과창 연출, 버튼 함수들은 기존 그대로 유지) ...
    // ... (너무 길어서 생략하지만 기존 코드를 유지하시면 됩니다) ...

    IEnumerator ShowResultRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        ShowResultUI();
    }

    void ShowResultUI()
    {
        // (직전에 만들어드린 Juicy한 결과창 코드 그대로 사용)
        Time.timeScale = 0f;

        if (resultBgImage != null)
        {
            resultBgImage.gameObject.SetActive(true);
            Color c = resultBgImage.color;
            c.a = 0f;
            resultBgImage.color = c;
        }

        resultPanel.SetActive(true);
        resultPanel.transform.localScale = Vector3.zero;

        if (resultTitleText != null)
        {
            resultTitleText.alpha = 0f;
            resultTitleText.transform.localScale = Vector3.one * 1.5f;
        }

        timeText.text = "00:00";

        if (cursorController != null)
        {
            cursorController.UnlockCursor();
            cursorController.ForceUnlocked = true;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        Sequence resultSequence = DOTween.Sequence();
        resultSequence.SetUpdate(true);

        if (resultBgImage != null)
            resultSequence.Append(resultBgImage.DOFade(0.8f, 0.5f));

        resultSequence.Join(resultPanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));

        if (resultTitleText != null)
        {
            resultSequence.Insert(0.2f, resultTitleText.DOFade(1f, 0.2f));
            resultSequence.Insert(0.2f, resultTitleText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBounce));
        }

        float tempTime = 0f;
        resultSequence.Insert(0.4f, DOTween.To(() => tempTime, x => tempTime = x, playTime, 0.5f)
            .OnUpdate(() =>
            {
                int m = Mathf.FloorToInt(tempTime / 60F);
                int s = Mathf.FloorToInt(tempTime % 60F);
                timeText.text = string.Format("{0:00}:{1:00}", m, s);
            }).SetEase(Ease.OutCubic));

        float starStartTime = 0.8f;
        float starInterval = 0.3f;

        for (int i = 0; i < 3; i++)
        {
            if (tempQuestCleared[i])
            {
                int index = i;
                Image targetStar = resultStarFills[index];

                resultSequence.InsertCallback(starStartTime, () =>
                {
                    targetStar.gameObject.SetActive(true);
                    targetStar.transform.localScale = Vector3.zero;
                });

                resultSequence.Insert(starStartTime, targetStar.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
                starStartTime += starInterval;
            }
        }
    }

    void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnMainClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }

    void OnNextClicked()
    {
        Time.timeScale = 1f;
        if (nextStageData != null) LoadingSceneController.LoadScene(nextStageData);
        else SceneManager.LoadScene("Main");
    }
}