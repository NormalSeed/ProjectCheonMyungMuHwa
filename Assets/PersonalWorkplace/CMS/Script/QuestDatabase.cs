using System;
using System.Collections.Generic;
using UnityEngine;

public static class QuestDatabase
{
    public static readonly List<Quest> AllQuests = new List<Quest>();

    public static void LoadFromCSV(string fileName)
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
            if (values.Length < 8) continue; // 안전장치

            var quest = new Quest
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
                rewardCount = int.Parse(values[7].Trim()),

                // 리셋 기준 초기화(초기 저장 시 당일/당주로 인식되도록)
                lastUpdated = DateTime.UtcNow,
                lastWeek = GetWeekOfYearUTC(DateTime.UtcNow)
            };

            AllQuests.Add(quest);
        }

        Debug.Log($"퀘스트 DB 로드 완료: {AllQuests.Count}개");
    }

    private static int GetWeekOfYearUTC(DateTime utc)
    {
        var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        return cal.GetWeekOfYear(utc, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }
}

//CSV 파일 예시
//questID,questName,questType,questTarget,valueGoal,rewardID,rewardType,rewardCount
//q001,첫 모험, 1, 2, 3, G001, 1, 100