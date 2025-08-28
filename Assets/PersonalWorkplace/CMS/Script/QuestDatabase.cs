using System;
using System.Collections.Generic;
using UnityEngine;

public static class QuestDatabase
{
    public static readonly List<Quest> DailyQuests = new List<Quest>();
    public static readonly List<Quest> WeeklyQuests = new List<Quest>();
    public static readonly List<Quest> RepeatQuests = new List<Quest>();

    public static void LoadAll()
    {
        LoadFromCSV("QuestTable_Day", QuestCategory.Daily, DailyQuests);
        LoadFromCSV("QuestTable_Week", QuestCategory.Weekly, WeeklyQuests);
        LoadFromCSV("QuestTable_Repeat", QuestCategory.Repeat, RepeatQuests);

        Debug.Log($"로드 완료: Daily={DailyQuests.Count}, Weekly={WeeklyQuests.Count}, Repeat={RepeatQuests.Count}");
    }

    private static void LoadFromCSV(string fileName, QuestCategory category, List<Quest> targetList)
    {
        targetList.Clear();

        TextAsset csvFile = Resources.Load<TextAsset>(fileName);
        if (csvFile == null)
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다: " + fileName);
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        // 0번줄은 헤더니까 i=1부터 시작
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(',');
            if (values.Length < 11) continue;

            var quest = new Quest
            {
                questID = values[0].Trim(),
                questName = values[1].Trim(),
                questType = category,  // CSV에 있는 Type(2)은 무시하고, 불러올 때 카테고리 지정
                questTarget = Enum.TryParse(values[3].Trim(), out QuestTargetType targetType)
                                ? targetType : QuestTargetType.None,
                valueProgress = int.Parse(values[4].Trim()),
                valueGoal = int.Parse(values[5].Trim()),
                isComplete = values[6].Trim().ToUpper() == "TRUE",
                isClaimed = values[7].Trim().ToUpper() == "TRUE",
                rewardID = values[8].Trim(),
                rewardType = (RewardType)int.Parse(values[9].Trim()),
                rewardCount = int.Parse(values[10].Trim()),

                lastUpdated = DateTime.UtcNow,
                lastWeek = GetWeekOfYearUTC(DateTime.UtcNow)
            };

            targetList.Add(quest);
        }
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