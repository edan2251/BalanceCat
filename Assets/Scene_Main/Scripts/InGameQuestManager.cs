using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic; // [필수] Dictionary

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

    private Dictionary<string, int> deliveredCounts = new Dictionary<string, int>();

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

        if (questPanelController != null && currentStageData != null)
        {
            questPanelController.Initialize(currentStageData);
        }

        if (SoundManager.Instance != null && currentStageData != null)
        {
            // 1챕터 = index 0, 2챕터 = index 1 ...
            SoundManager.Instance.PlayChapterBGM(currentStageData.chapterIndex);
        }

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

        /*인게임 퀘스트 클리어 치트키용
          */
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("[Cheat] 퀘스트 1 (메인) 강제 완료");
            SolveQuest(0);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("[Cheat] 퀘스트 2 강제 완료");
            SolveQuest(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("[Cheat] 퀘스트 3 강제 완료");
            SolveQuest(2);
        }
    }

    public void OnItemDelivered(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return;

        // 1. 카운트 증가
        if (!deliveredCounts.ContainsKey(itemID))
            deliveredCounts[itemID] = 0;

        deliveredCounts[itemID]++;

        Debug.Log($"[배달] {itemID} 배달됨. 현재 {deliveredCounts[itemID]}개");

        // [버그 수정] 아이템 관련성 및 클리어 여부 확인
        bool isRelevant = false;
        bool willClearNow = false;

        if (currentStageData != null)
        {
            foreach (var q in currentStageData.quests)
            {
                // 배달 퀘스트이고, 아이템 ID가 일치하는 경우
                if (q != null && q.type == QuestType.Delivery && q.requiredItemID == itemID)
                {
                    isRelevant = true;

                    // 이번 배달로 목표치를 달성했는가?
                    if (deliveredCounts[itemID] >= (int)q.targetValue)
                    {
                        willClearNow = true;
                    }
                }
            }
        }

        // 2. UI 갱신
        RefreshQuestUI();

        // 3. 패널 열기 제어
        // [조건] 관련 있는 아이템이고(isRelevant) && 
        //        이번에 클리어되는 게 아닐 때(!willClearNow)만 
        //        진행 상황 패널을 엽니다.
        //        (클리어되는 순간이면 SolveQuest가 별 뾰잉 연출과 함께 패널을 열어줍니다)
        if (isRelevant && !willClearNow && questPanelController != null)
        {
            questPanelController.OpenPanelTemporary(3.0f);
        }

        // 4. 클리어 체크
        CheckClearCondition();
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

            // 1. 메인 퀘스트 & 시간 제한 (클리어해야 켜짐)
            if (i == 0 || q.type == QuestType.TimeLimit)
            {
                isStarOn = tempQuestCleared[i];
            }

            // 2. [수정됨] 부정 퀘스트 (NoRespawn, NoFall)
            // 아직 클리어 확정은 아니지만, 현재 조건을 만족 중이면 '별이 켜진 상태'로 보여줍니다.
            // 실패하는 순간(OnPlayerRespawn 등에서) RefreshQuestUI가 불리면 조건 불만족으로 꺼지게 됩니다.
            else if (q.type == QuestType.NoRespawn)
            {
                descText += $" <color=yellow>({respawnCount} / {q.targetValue})</color>";

                // [조건 복구] 현재 횟수가 목표치 '이하'면(아직 안 죽었으면) 켜둠!
                // 단, 목표치를 넘으면 꺼짐.
                if (respawnCount < q.targetValue) isStarOn = true;
                else isStarOn = false;
            }
            else if (q.type == QuestType.NoFall)
            {
                descText += $" <color=yellow>({fallCount} / {q.targetValue})</color>";

                // [조건 복구] 현재 횟수가 목표치 '이하'면 켜둠!
                if (fallCount < q.targetValue) isStarOn = true;
                else isStarOn = false;
            }

            // 3. 배달 퀘스트 (기존 유지)
            // 배달은 '달성 시' 켜져야 하므로 tempQuestCleared를 따릅니다.
            else if (q.type == QuestType.Delivery)
            {
                int current = 0;
                if (deliveredCounts.ContainsKey(q.requiredItemID))
                    current = deliveredCounts[q.requiredItemID];

                int required = (int)q.targetValue;
                descText += $" <color=yellow>({current} / {required})</color>";

                // 배달은 뾰잉 연출을 위해 확정 변수를 따름
                isStarOn = tempQuestCleared[i];
            }

            questPanelController.UpdateQuestRealtime(i, descText, isStarOn);
        }
    }

    public void OnPlayerRespawn()
    {
        respawnCount++;
        Debug.Log($"[퀘스트] 리스폰 {respawnCount}회");

        RefreshQuestUI(); // 1. 텍스트 갱신

        // [추가] 2. 수치가 변했으니 3초간 패널 열어서 보여줌
        if (questPanelController != null)
        {
            questPanelController.OpenPanelTemporary(3.0f);
        }

        // 3. 실패 조건 체크 (즉시 실패 반영)
        CheckClearCondition();
    }

    public void OnPlayerFall()
    {
        fallCount++;
        Debug.Log($"[퀘스트] 넘어짐 {fallCount}회");

        RefreshQuestUI(); // 1. 텍스트 갱신

        // [추가] 2. 수치가 변했으니 3초간 패널 열어서 보여줌
        if (questPanelController != null)
        {
            questPanelController.OpenPanelTemporary(3.0f);
        }

        // 3. 실패 조건 체크
        CheckClearCondition();
    }



    public void CheckClearCondition()
    {
        // 1. 메인 퀘스트(0번) 클리어 조건 확인
        bool isMainCleared = false;
        if (ScoreZone.Instance != null && ScoreZone.Instance.IsCleared)
        {
            isMainCleared = true;
            SolveQuest(0);
        }

        // 메인 퀘스트가 이미 깨져있었는지 확인 (재진입 방지용이 아니라, 조건부 퀘스트 체크용)
        if (tempQuestCleared[0]) isMainCleared = true;

        if (currentStageData != null)
        {
            for (int i = 1; i < currentStageData.quests.Count; i++)
            {
                QuestData q = currentStageData.quests[i];
                if (q == null) continue;

                bool isSuccess = false;

                // [Bug 2 수정] 조건부 퀘스트(시간, 노데스, 노폴)는
                // "메인 퀘스트가 클리어된 상태(또는 지금 되는 중)"일 때만 성공 여부를 따집니다.
                // 그래야 게임 도중에 아이템 하나 넣었다고 갑자기 "시간제한 성공!" 하고 별이 뜨지 않습니다.

                if (q.type == QuestType.TimeLimit)
                {
                    if (isMainCleared && playTime <= q.targetValue) isSuccess = true;
                }
                else if (q.type == QuestType.NoRespawn)
                {
                    if (isMainCleared && respawnCount < q.targetValue) isSuccess = true; // < 로 복구 (허용횟수)
                }
                else if (q.type == QuestType.NoFall)
                {
                    if (isMainCleared && fallCount < q.targetValue) isSuccess = true; // < 로 복구
                }

                // 배달 퀘스트는 메인 클리어와 상관없이 언제든 달성 가능
                else if (q.type == QuestType.Delivery)
                {
                    int current = 0;
                    if (deliveredCounts.ContainsKey(q.requiredItemID))
                        current = deliveredCounts[q.requiredItemID];

                    if (current >= (int)q.targetValue) isSuccess = true;
                }

                if (isSuccess) SolveQuest(i);
            }
        }
    }

    public void SolveQuest(int questIndex)
    {
        if (questIndex < 0 || questIndex >= 3) return;
        if (tempQuestCleared[questIndex]) return; // 이미 깬 퀘스트는 무시

        // 1. 데이터상 클리어 처리
        tempQuestCleared[questIndex] = true;

        // 2. HUD 연출
        if (hudStarImages.Length > questIndex && hudStarImages[questIndex] != null)
        {
            hudStarImages[questIndex].sprite = filledStarSprite;
            hudStarImages[questIndex].transform.DOPunchScale(Vector3.one * 0.5f, 0.3f);
        }

        // 3. [핵심] 퀘스트 패널 연출 (별 뾰잉)
        // RefreshQuestUI가 별을 미리 켜지 않게 수정했으므로, 
        // 여기서 ShowQuestClearSequence를 부르면 "빈 별 -> 뾰잉 -> 채워진 별" 연출이 정상 작동합니다.
        if (questPanelController != null)
        {
            questPanelController.ShowQuestClearSequence(questIndex, 5.0f);
        }

        // 4. 메인 퀘스트면 스테이지 종료 절차
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
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.ButtonClick);
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnMainClicked()
    {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SFX.ButtonClick);
            }
        
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }

    void OnNextClicked()
    {

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.ButtonClick);
        }
        Time.timeScale = 1f;
        if (nextStageData != null) LoadingSceneController.LoadScene(nextStageData);
        else SceneManager.LoadScene("Main");
    }
}