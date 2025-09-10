using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public RawImage[] popups;    // Raw Image 4장
    public GameObject panel;     // Panel 오브젝트
    private int currentIndex = 0;

    void Start()
    {
        ShowPanel(); // 시작 시 Panel 켜기
    }

    void Update()
    {
        if (panel == null || popups == null || popups.Length == 0)
            return;

        // 다음 이미지
        if (Input.GetKeyDown(KeyCode.M) || Input.GetMouseButtonDown(0))
        {
            ShowNext();
        }

        // 이전 이미지
        if (Input.GetKeyDown(KeyCode.N))
        {
            ShowPrevious();
        }

        // Panel 토글
        if (Input.GetKeyDown(KeyCode.B))
        {
            TogglePanel();
        }
    }

    void ShowNext()
    {
        if (currentIndex < 0 || currentIndex >= popups.Length)
            return;

        popups[currentIndex].enabled = false;
        currentIndex++;

        if (currentIndex >= popups.Length)
        {
            panel.SetActive(false); // 끝이면 Panel 끄기
            currentIndex = popups.Length - 1; // 안전하게 마지막 인덱스 유지
            return;
        }

        popups[currentIndex].enabled = true;
    }

    void ShowPrevious()
    {
        if (currentIndex <= 0)
        {
            currentIndex = 0; // 첫 페이지면 아무 동작 안함
            return;
        }

        popups[currentIndex].enabled = false;
        currentIndex--;
        popups[currentIndex].enabled = true;
    }

    void ShowPanel()
    {
        if (panel == null || popups == null || popups.Length == 0)
            return;

        panel.SetActive(true);

        // 모든 이미지 끄기
        for (int i = 0; i < popups.Length; i++)
        {
            if (popups[i] != null)
                popups[i].enabled = false;
        }

        currentIndex = 0;

        // 첫 이미지 켜기
        if (popups[0] != null)
            popups[0].enabled = true;
    }

    void TogglePanel()
    {
        if (panel == null)
            return;

        bool isActive = panel.activeSelf;
        if (isActive)
        {
            panel.SetActive(false); // 켜져있으면 끄기
        }
        else
        {
            ShowPanel(); // 꺼져있으면 켜기 + 첫 이미지로 초기화
        }
    }
}
