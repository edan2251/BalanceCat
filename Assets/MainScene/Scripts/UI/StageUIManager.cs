using UnityEngine;
using UnityEngine.UI; // UI ����� ���� �ʼ�
using System;

public class StageUIManager : MonoBehaviour
{
    [Header("Main UI Panel")]
    public GameObject mainUIPanel;

    // === UI ��� ���� (�ν����Ϳ��� �Ҵ�) ===
    [Header("UI Text Fields")]
    public Text chapterNameText;     // [ é�͸� ]
    public Text stageTitleText;      // [ 1 �������� ]
    public Text stageSynopsisText;   // �������� �ó�ý�
    public Text questText;           // �־��� ����Ʈ
    public Text rewardText;          // ���� ����

    [Header("UI Button")]
    public Button startButton;       // ���� ��ư

    [Header("����")]
    public ChapterSelector chapterSelector; 


    void Start()
    {
        if (chapterSelector == null)
        {
            chapterSelector = FindObjectOfType<ChapterSelector>();
        }

        if (chapterSelector == null || !chapterSelector.IsChapterSelectionActive())
        {
            gameObject.SetActive(false);
        }
    }


    // StageSelector�� ȣ���Ͽ� UI ������ ������Ʈ�ϴ� �Լ�
    public void UpdateStageInfo(StageData data)
    {
        gameObject.SetActive(true);

        if (data == null)
        {
            Debug.LogError("StageData�� NULL�Դϴ�. UI ������Ʈ�� �ǳʶݴϴ�.");
            // �����Ͱ� ������ UI�� ����ϴ�.
            chapterNameText.text = "������ ����";
            stageTitleText.text = "";
            gameObject.SetActive(false);
            return;
        }

        // === ������ ������� UI �ؽ�Ʈ ������Ʈ ===

        // 1. é�͸� �� ����������
        chapterNameText.text = $"{data.chapterName}";
        stageTitleText.text = $"{data.stageTitle}";

        // 2. �� ����
        stageSynopsisText.text = data.synopsis;
        questText.text = $"{data.questDescription}";
        rewardText.text = $"{data.rewardName}";

        // 3. ���� ��ư ������ ������Ʈ
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() => StartStage(data));
    }

    // UI ����� (��: é�� ���� ��带 �������� �� ChapterSelector���� ȣ��)
    public void HideUI()
    {
        if (mainUIPanel != null)
        {
            mainUIPanel.SetActive(false);
        }
    }

    private void StartStage(StageData data)
    {
        Debug.Log($"�������� ����: é�� {data.chapterName}, ID {data.stageID}");

        // TODO:
        // 1. ChapterSelector�� ���� é�� ���� ��� ��Ȱ��ȭ (ī�޶� ���� ��ġ ���� ��)
        // 2. ���� ���� �� �ε� ���� (SceneManager.LoadScene ��) �߰�
    }
}