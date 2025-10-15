using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ChapterData_", menuName = "GameData/Chapter Data", order = 0)]
public class ChapterData : ScriptableObject
{
    [Tooltip("�� é��(�༺)�� ���Ե� ��� �������� ������ ����Ʈ�Դϴ�.")]
    public List<StageData> stages;
}