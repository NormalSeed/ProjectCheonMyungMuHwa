using System.Collections.Generic;
using UnityEngine;

public class QuestDatabase : MonoBehaviour
{
    public static List <Quest> AllQuests = new List<Quest>();

    public void LoadFromCSV(string csvText)
    {
        string[] lines = csvText.Split('\n');

        for(int i = 1; i < lines.Length; i++) //첫줄 스킵
        {
            if(string.IsNullOrWhiteSpace(lines[i]))
                continue;


            string[] values = lines[i].Split(',');

            Quest quest = new Quest
            {
                questID = values[0].Trim(),
                questName = values[1].Trim(),

                questType = (QuestCategory)int.Parse(values[2].Trim()),
                questTarget = (QuestTargetType)int.Parse(values[3].Trim()),
                valueProgress = 0, //초기 진행 수치는 0
                valueGoal = int.Parse(values[4].Trim()),

                isComplete = false, //초기 완료 여부는 false
                isClaimed = false, //초기 보상 수령 여부는 false

                rewardID = values[5].Trim(),
                rewardType = (RewardType)int.Parse(values[6].Trim()),
                rewardCount = int.Parse(values[7].Trim())
            };
            AllQuests.Add(quest);
        }
        Debug.Log("퀘스트 데이터베이스 로드 완료: " + AllQuests.Count + "개의 퀘스트가 로드되었습니다.");
    }
}

//CSV 파일 예시
//questID,questName,questType,questTarget,valueGoal,rewardID,rewardType,rewardCount
//q001,첫 모험, 1, 2, 3, G001, 1, 100