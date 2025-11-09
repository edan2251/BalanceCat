using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text; // UTF-8 인코딩을 위해 추가

public class QuestDataImporter
{
    // CSV 파일이 위치한 프로젝트 상대 경로
    private static string csvPath = "Assets/QuestData/QuestList/BalanceCat_Quest.csv";

    // 생성된 ScriptableObject를 저장할 경로
    private static string savePath = "Assets/QuestData/QuestList";

    [MenuItem("Tools/Import Quest Data")]
    public static void ImportQuestData()
    {
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            Debug.Log($"Save path created: {savePath}");
        }

        ClearFolder(savePath);


        string[] allLines;
        try
        {
            allLines = File.ReadAllLines(csvPath, Encoding.UTF8);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CSV 파일 읽기 실패: {e.Message}");
            return;
        }

        if (allLines.Length <= 1)
        {
            Debug.LogWarning("CSV 파일이 비어있거나 헤더만 있습니다.");
            return;
        }

        int createdCount = 0;

        for (int i = 1; i < allLines.Length; i++)
        {
            string line = allLines[i];
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            if (values.Length < 4)
            {
                Debug.LogWarning($"Skipping line {i + 1}: 열 개수가 4개 미만입니다. (A~D열 필요)");
                continue;
            }

            // 4. 데이터 추출
            string ChapterID = values[0].Trim();      // A열 (챕터 번호)
            string StageID = values[1].Trim();        // B열 (스테이지 번호)
            string QuestID = values[2].Trim();        // C열 (퀘스트 번호)
            string questTitle = values[3].Trim();  // D열 (퀘스트 전체 내용)
            string questDescription = values[4].Trim();  // E열 (퀘스트 전체 내용)

            if (string.IsNullOrEmpty(questDescription))
            {
                Debug.LogWarning($"Skipping line {i + 1}: 퀘스트 내용(D열)이 비어있습니다.");
                continue;
            }

            QuestData questData = ScriptableObject.CreateInstance<QuestData>();

            questData.questTitle = questTitle;
            questData.questDescription = questDescription;

            // SO 파일로 저장
            // 파일명 예시: QuestData_0_0_Quest1.asset
            string assetName = $"QuestData_{ChapterID}_{StageID}_{"Quest"}{QuestID}.asset";
            string assetPath = Path.Combine(savePath, assetName);

            AssetDatabase.CreateAsset(questData, assetPath);
            createdCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<b>{createdCount}개</b>의 퀘스트 데이터를 성공적으로 임포트했습니다! (저장 경로: {savePath})");
    }

    // savePath 폴더 내의 모든 .asset 파일을 삭제하는 도우미 함수
    private static void ClearFolder(string folderPath)
    {
        DirectoryInfo dir = new DirectoryInfo(folderPath);
        if (!dir.Exists) return;

        FileInfo[] files = dir.GetFiles("*.asset");
        foreach (FileInfo file in files)
        {
            AssetDatabase.DeleteAsset(file.FullName.Replace(Application.dataPath, "Assets"));
        }
        Debug.Log($"기존 퀘스트 데이터 {files.Length}개를 삭제했습니다. (경로: {folderPath})");
    }
}