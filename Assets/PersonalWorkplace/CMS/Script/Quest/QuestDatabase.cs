using System;
using System.Collections.Generic;
using UnityEngine;

public static class QuestDatabase
{
    public static readonly List<Quest> DailyQuests = new List<Quest>();
    public static readonly List<Quest> WeeklyQuests = new List<Quest>();
    public static readonly List<Quest> RepeatQuests = new List<Quest>();
    public static List<Quest> MissionQuests = new List<Quest>();

    public static void LoadAll()
    {
        Debug.Log("=== [QuestDatabase.LoadAll 호출됨] ===");

        LoadFromCSV("QuestTable_Day", QuestCategory.Daily, DailyQuests);
        LoadFromCSV("QuestTable_Week", QuestCategory.Weekly, WeeklyQuests);
        LoadFromCSV("QuestTable_Repeat", QuestCategory.Repeat, RepeatQuests);
        LoadFromCSV("QuestTable_Mission", QuestCategory.Mission, MissionQuests);

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

    // 카테고리별로 필요한 최소 열의 개수를 정의
    int requiredColumns = (category == QuestCategory.Mission) ? 9 : 8;

    for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더
    {
        if (string.IsNullOrWhiteSpace(lines[i])) continue;

        string[] values = lines[i].Split(',');

        // 위에서 정의한 변수로 검사
        if (values.Length < requiredColumns)
        {
            Debug.LogWarning($"[{category}] {fileName} 줄 {i} 건너뜀 (컬럼 부족, 필요: {requiredColumns}, 실제: {values.Length})");
            continue;
        }

        var quest = new Quest
        {
            questID = values[0].Trim(),
            questName = values[1].Trim(),
            questType = category,
            questTarget = Enum.TryParse(values[3].Trim(), out QuestTargetType targetType)
                            ? targetType : QuestTargetType.None,
            valueProgress = 0,
            valueGoal = int.Parse(values[4].Trim()),
            isComplete = false,
            isClaimed = false,
            lastUpdated = DateTime.UtcNow,
            lastWeek = GetWeekOfYearUTC(DateTime.UtcNow),
        };

        // 보상
        var reward = new Reward
        {
            rewardID = values[5].Trim(),
            rewardType = (RewardType)int.Parse(values[6].Trim()),
            rewardCount = int.Parse(values[7].Trim())
        };
        quest.rewards.Add(reward);

        // 카테고리별 해금 처리
        if (category == QuestCategory.Mission)
        {
            quest.unlockCondition = values[8].Trim(); // 미션 퀘스트일 때만 9번째 열을 읽음

            if (quest.unlockCondition.StartsWith("Stage", StringComparison.OrdinalIgnoreCase))
            {
                string stageStr = quest.unlockCondition.Replace("Stage", "").TrimStart('0');
                if (int.TryParse(stageStr, out int stageNum))
                {
                    quest.requiredStage = stageNum;
                    quest.state = QuestState.Locked; // 기본 잠금
                }
            }
        }
        else
        {
            quest.state = QuestState.InProgress; // 항상 활성화
        }

        targetList.Add(quest);
        Debug.Log($"[{category}] {quest.questID} / {quest.questName}");
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