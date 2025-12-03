using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using System.Collections;

public class InGameQuestManager : MonoBehaviour
{
    // ... (기존 변수들은 그대로) ...
    public static InGameQuestManager Instance;

    [Header("--- 데이터 ---")]
    public StageData currentStageData;

    [Tooltip("다음 스테이지의 데이터 SO를 여기에 넣어주세요!")]
    public StageData nextStageData;

    [Header("--- 커서 제어 ---")]
    public ShowCursor cursorController; // 인스펙터 연결 필수

    [Header("--- 인게임 HUD ---")]
    public Image[] hudStarImages;
    public Sprite filledStarSprite;
    public Sprite emptyStarSprite;

    [Header("--- 결과창 UI ---")]
    public GameObject resultPanel;
    public TextMeshProUGUI timeText;
    public Image[] resultStarFills;

    // [추가] A. 배경 어둡게 처리를 위한 이미지 (패널 뒤의 검은 배경)
    public Image resultBgImage;

    // [추가] B. 타이틀 연출을 위한 텍스트 ("STAGE CLEAR")
    public TextMeshProUGUI resultTitleText;

    [Header("--- 버튼 연결 ---")]
    public Button restartBtn;
    public Button mainBtn;
    public Button nextBtn;

    private bool[] tempQuestCleared = new bool[3];
    private float playTime = 0f;
    private bool isGameActive = true;

    void Awake() { Instance = this; }

    void Start()
    {
        // [중요] 혹시 멈춘 상태로 들어왔을 경우를 대비해 시간 복구
        Time.timeScale = 1f;

        isGameActive = true;
        playTime = 0f;
        resultPanel.SetActive(false);

        if (emptyStarSprite != null)
        {
            foreach (var img in hudStarImages) if (img != null) img.sprite = emptyStarSprite;
        }

        foreach (var star in resultStarFills) if (star != null) star.gameObject.SetActive(false);

        restartBtn.onClick.AddListener(OnRestartClicked);
        mainBtn.onClick.AddListener(OnMainClicked);
        nextBtn.onClick.AddListener(OnNextClicked);
    }

    void Update()
    {
        if (isGameActive) playTime += Time.deltaTime;

        // 테스트용 (삭제 가능)
        if (Input.GetKeyDown(KeyCode.Alpha1)) SolveQuest(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SolveQuest(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SolveQuest(2);
    }

    public void SolveQuest(int questIndex)
    {
        if (questIndex < 0 || questIndex >= 3) return;
        if (tempQuestCleared[questIndex]) return;

        tempQuestCleared[questIndex] = true;

        // HUD 연출 (게임 중이므로 일반 TimeScale 따름)
        if (hudStarImages.Length > questIndex && hudStarImages[questIndex] != null)
        {
            hudStarImages[questIndex].sprite = filledStarSprite;
            hudStarImages[questIndex].transform.DOPunchScale(Vector3.one * 0.5f, 0.3f);
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

        // 결과창 띄우기 (1초 뒤)
        // [수정] 코루틴을 사용하여 딜레이 후 실행 (Invoke는 timeScale=0이면 작동 안 할 수 있어서)
        StartCoroutine(ShowResultRoutine());
    }

    IEnumerator ShowResultRoutine()
    {
        // 1초 대기 (이때는 아직 시간이 흐름)
        yield return new WaitForSeconds(1.0f);

        ShowResultUI();
    }

    void ShowResultUI()
    {
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


        // --- Step 4: 별 채우기 (타이밍 조절 핵심!) ---

        //  [조절 1] 별 시작 시간: 0.8초
        // 숫자가 거의 다 올라갈 때쯤(0.9초 종료) 살짝 겹치면서 시작합니다.
        // 아까 너무 느리다고 하셔서 1.0f -> 0.8f로 당겼습니다.
        float starStartTime = 0.8f;

        //  [조절 2] 별 간격: 0.3초
        // 아까 0.1f는 너무 빨라서 후다닥 지나갔으니, 0.3f로 늘려서 "쿵... 쿵... 쿵" 느낌을 줍니다.
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

    // --- 버튼 기능들 (중요: 시간을 다시 흐르게 해줘야 함) ---

    void OnRestartClicked()
    {
        Time.timeScale = 1f; // [필수] 시간 복구
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnMainClicked()
    {
        Time.timeScale = 1f; // [필수] 시간 복구
        SceneManager.LoadScene("Main");
    }

    void OnNextClicked()
    {
        Time.timeScale = 1f; // [필수] 시간 복구

        if (nextStageData != null)
        {
            // [핵심] 로딩 씬 컨트롤러에게 다음 스테이지 데이터를 넘기면서 로딩 시작!
            LoadingSceneController.LoadScene(nextStageData);
        }
        else
        {
            Debug.LogWarning("다음 스테이지 데이터(nextStageData)가 연결되지 않았습니다! 메인으로 이동합니다.");
            SceneManager.LoadScene("Main"); // 데이터 없으면 안전하게 메인으로
        }
    }
}