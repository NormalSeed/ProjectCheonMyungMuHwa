using System.Collections.Generic;
using UnityEngine;

public class QuestDatabase : MonoBehaviour
{
    public static List<Quest> AllQuests = new List<Quest>();

    public void LoadFromCSV(string fileName)
    {
        AllQuests.Clear();

        TextAsset csvFile = Resources.Load<TextAsset>(fileName);
        if (csvFile == null)
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다: " + fileName);
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] values = lines[i].Split(',');

            Quest quest = new Quest
            {
                questID = values[0].Trim(),
                questName = values[1].Trim(),

                questType = (QuestCategory)int.Parse(values[2].Trim()),
                questTarget = (QuestTargetType)int.Parse(values[3].Trim()),
                valueProgress = 0,
                valueGoal = int.Parse(values[4].Trim()),

                isComplete = false,
                isClaimed = false,

                rewardID = values[5].Trim(),
                rewardType = (RewardType)int.Parse(values[6].Trim()),
                rewardCount = int.Parse(values[7].Trim())
            };

            AllQuests.Add(quest);
        }

        Debug.Log("퀘스트 데이터베이스 로드 완료: " + AllQuests.Count + "개의 퀘스트 로드됨");
    }
}

//CSV 파일 예시
//questID,questName,questType,questTarget,valueGoal,rewardID,rewardType,rewardCount
//q001,첫 모험, 1, 2, 3, G001, 1, 100