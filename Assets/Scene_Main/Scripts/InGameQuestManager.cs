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
    public string nextSceneName;

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
        // [추가] 1. 게임 시간 정지! (물리, 애니메이션, Update 멈춤)
        Time.timeScale = 0f;

        resultPanel.SetActive(true);

        // 커서 풀기
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

        int minutes = Mathf.FloorToInt(playTime / 60F);
        int seconds = Mathf.FloorToInt(playTime % 60F);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // [중요] DOTween 시퀀스 생성
        Sequence starSequence = DOTween.Sequence();

        // [핵심] 이 시퀀스는 timeScale이 0이어도 돌아가게 설정!! (SetUpdate(true))
        starSequence.SetUpdate(true);

        for (int i = 0; i < 3; i++)
        {
            if (tempQuestCleared[i])
            {
                int index = i;
                Image targetStar = resultStarFills[index];

                starSequence.AppendCallback(() =>
                {
                    targetStar.gameObject.SetActive(true);
                    targetStar.transform.localScale = Vector3.zero;
                });

                // 별 커지는 애니메이션
                starSequence.Append(targetStar.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
                starSequence.AppendInterval(0.2f);
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
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene("Main");
        }
    }
}