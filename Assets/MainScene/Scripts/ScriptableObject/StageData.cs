using UnityEngine;
// StagePoint ��ũ��Ʈ�� ��ġ��Ű�� ���� stageID�� �����մϴ�.

[CreateAssetMenu(fileName = "StageData_", menuName = "GameData/Stage Data", order = 1)]
public class StageData : ScriptableObject
{
    // === �������� �ĺ� ���� ===
    [Tooltip("StagePoint ��ũ��Ʈ�� Stage ID�� ��ġ�ؾ� �մϴ� (1���� ����).")]
    public int stageID = 1;

    // === UI ǥ�� ���� ===
    [Header("UI ǥ�� ����")]
    public string chapterName = "é�� �̸�";
    public string stageTitle = "�������� �̸�";

    [TextArea(3, 10)]
    public string synopsis = "���������� ������ ��� �̾߱⳪ ��ǥ�� �����մϴ�.";

    [Header("����Ʈ �� ����")]
    [TextArea(1, 5)]
    public string questDescription = "�̰��� ����Ʈ ������ �Է��Ͻÿ�";

    public string rewardName = "�̰��� ����Ʈ ������ �Է��Ͻÿ�";
    // �ʿ��� ���, ���� ���� ������Ʈ�� �������� ���� ���� �߰� ����
    // public Sprite rewardIcon; 

}