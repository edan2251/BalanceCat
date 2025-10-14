using UnityEngine;
using UnityEngine.UI;

public class StageUIManager : MonoBehaviour
{
    // �ν����Ϳ� ������ UI ��ҵ�
    public Text chapterNameText;
    public Text stageTitleText;
    public Text stageSynopsisText;
    public Text questText;
    public Text rewardText;
    public Button startButton; // ���� ��ư

    // StageSelector�� ȣ���� ���� ������Ʈ �Լ�
    public void UpdateStageInfo(StageData data)
    {
        if (data == null)
        {
            // �����Ͱ� null�� ��� UI�� ����ų� �ʱ�ȭ
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        // �ؽ�Ʈ ������Ʈ
        chapterNameText.text = data.chapterName;
        // "3 ��������" ó�� ���� �������� ID�� �Բ� ǥ��   /*{data.stageID}*/
        stageTitleText.text = $"{data.stageTitle}";
        stageSynopsisText.text = data.synopsis;
        questText.text = $"{data.questDescription}";
        rewardText.text = $"{data.rewardName}";

        // ���� ��ư �̺�Ʈ ���� (����)
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() => StartStage(data));
    }

    private void StartStage(StageData data)
    {
        Debug.Log($"�������� ����: é�� {data.chapterName}, ID {data.stageID}");
        // ���� �� �ε� �Ǵ� ���� ���� ȣ��
    }
}