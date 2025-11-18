using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingSceneController : MonoBehaviour
{
    static string nextScene;
    static Sprite currentLoadingSprite;
    static string currentLoadingText;

    [Header("UI References")]
    [SerializeField] Image progressBar;
    [SerializeField] Image comicImage;
    [SerializeField] TextMeshProUGUI descText;
    [SerializeField] GameObject clickToStartObject;

    [Header("Loading Settings")]
    [Tooltip("실제 로딩이 빨라도 이 시간만큼은 로딩바가 채워지는 연출을 보여줍니다.")]
    [SerializeField] float minLoadingTime = 2.5f; // 최소 2.5초 동안 로딩 연출

    public static void LoadScene(StageData data)
    {
        nextScene = data.targetSceneName;
        currentLoadingSprite = data.loadingComicImage;
        currentLoadingText = data.loadingDescription;

        SceneManager.LoadScene("Loading");
    }

    void Start()
    {
        if (comicImage != null && currentLoadingSprite != null)
            comicImage.sprite = currentLoadingSprite;

        if (descText != null)
            descText.text = currentLoadingText;

        if (clickToStartObject != null)
            clickToStartObject.SetActive(false);

        // 시작 시 로딩바 0으로 초기화
        progressBar.fillAmount = 0f;

        StartCoroutine(LoadSceneProcess());
    }

    IEnumerator LoadSceneProcess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0f;

        // 로딩바가 채워지는 연출을 위해 반복문을 돕니다.
        // op.isDone은 allowSceneActivation이 true가 되기 전엔 false로 유지되므로
        // 별도의 탈출 조건을 사용합니다.
        while (true)
        {
            yield return null;

            timer += Time.unscaledDeltaTime;

            // [핵심 로직]
            // 1. 실제 로딩 진행률 (op.progress는 최대 0.9까지 오름) -> 0.9로 나누어 0~1 사이 값으로 변환
            float realProgress = op.progress / 0.9f;

            // 2. 가짜(연출) 로딩 진행률 -> 시간 흐름에 따라 0~1 사이 값으로 증가
            float fakeProgress = timer / minLoadingTime;

            // 3. 둘 중 '더 작은 값'을 사용해서 로딩바를 채움
            //    (실제 로딩이 아무리 빨라도 fakeProgress 때문에 천천히 차오름)
            //    (반대로 실제 로딩이 아주 느리면 realProgress 때문에 멈춰있음)
            float currentFill = Mathf.Min(realProgress, fakeProgress);

            // 로딩바 갱신 (부드럽게 보이려면 Lerp를 써도 되지만, fakeProgress 자체가 선형이라 바로 대입해도 됨)
            progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, currentFill, Time.unscaledDeltaTime * 10f); // 약간의 보간 추가

            // [조건]
            // 1. 실제 로딩이 90% 이상 끝남 (op.progress >= 0.9f)
            // 2. 최소 로딩 시간도 지남 (timer >= minLoadingTime)
            // 3. 로딩바가 시각적으로 거의 꽉 참 (progressBar.fillAmount >= 0.99f)
            if (op.progress >= 0.9f && timer >= minLoadingTime && progressBar.fillAmount >= 0.99f)
            {
                // 확실하게 100%로 맞춤
                progressBar.fillAmount = 1f;

                // 안내 문구 표시
                if (clickToStartObject != null)
                    clickToStartObject.SetActive(true);

                // 클릭 대기
                if (Input.GetMouseButtonDown(0))
                {
                    op.allowSceneActivation = true; // 씬 전환
                    yield break; // 코루틴 종료
                }
            }
        }
    }
}