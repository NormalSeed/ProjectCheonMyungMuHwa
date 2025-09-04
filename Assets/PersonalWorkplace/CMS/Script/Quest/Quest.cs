using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

[System.Serializable]
public class Quest
{
    //기본 정보
    public string questID; // 퀘스트 ID
    public string questName; // 퀘스트 제목
    public QuestCategory questType; //퀘스트 유형 (일일/주간/반복)
    public QuestTargetType questTarget; //목표 유형 (처치/수집/탐험 등)

    //진행 정보
    public int valueProgress; //현재 진행 수치
    public int valueGoal; //목표 수치
    public bool isComplete; //완료 여부
    public bool isClaimed; //보상 수령 여부

    //보상 정보
    public List<Reward> rewards = new List<Reward>();

    //리셋 관리
    public DateTime lastUpdated; //마지막 갱신
    public int lastWeek; //마지막 갱신 주차 

    public Quest() { }

    public Quest(string id, string name, QuestCategory type, QuestTargetType target, int goal,
             string rewardId, CurrencyType currencyType, int rewardCount)
    {
        questID = id;
        questName = name;
        questType = type;
        questTarget = target;
        valueProgress = 0;
        valueGoal = goal;
        isComplete = false;
        isClaimed = false;

        this.lastUpdated = DateTime.UtcNow;
        this.lastWeek = GetCurrentWeek(DateTime.UtcNow);

        rewards.Add(new Reward
        {
            rewardID = rewardId,
            currencyType = currencyType, 
            rewardCount = rewardCount
        });
    }
    public string GetRemainingTimeString()
    {
        DateTime now = QuestManager.Instance != null ? QuestManager.Instance.NowUtc() : DateTime.UtcNow;
        TimeSpan remain = TimeSpan.Zero;

        switch (questType)
        {
            case QuestCategory.Daily:
                // 마지막 갱신일 기준 자정까지 남은 시간
                DateTime nextReset = lastUpdated.Date.AddDays(1);
                remain = nextReset - now;
                break;

            case QuestCategory.Weekly:
                // 마지막 갱신일 기준 다음 주 월요일 0시까지 남은 시간
                int daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
                DateTime nextWeekReset = now.Date.AddDays(daysUntilMonday).Date;
                remain = nextWeekReset - now;
                break;

            default:
                return ""; // 반복/목표 퀘스트는 제한 시간 없음
        }

        if (remain.TotalSeconds < 0) remain = TimeSpan.Zero;
        return $"남은 시간: {remain:hh\\:mm\\:ss}";
    }

    //진행도 추가
    public void AddProgress(int amount)
    {
        if (isComplete) return;

        valueProgress += amount;
        if (valueProgress >= valueGoal)
        {
            valueProgress = valueGoal;
            isComplete = true;
        }
        lastUpdated = QuestManager.Instance != null ? QuestManager.Instance.NowUtc() : DateTime.UtcNow;
    }


    //퀘스트 리셋
    public void ResetProgress()
    {
        valueProgress = 0;
        isComplete = false;
        isClaimed = false;
        lastUpdated = QuestManager.Instance != null ? QuestManager.Instance.NowUtc() : DateTime.UtcNow;
    }
    // 주차 계산 (주간 퀘스트 체크용)
    private int GetCurrentWeek(DateTime time)
    {
        var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        return cal.GetWeekOfYear(time, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }
}
public enum QuestCategory
{
    Daily = 1,
    Weekly = 2,
    Repeat = 3,
    Objective = 4
}

public enum QuestTargetType
{
    None,
    Onlogin,
    Monster,
    Summon,
    Training,
    Upgrade,
    Playtime
}

public enum RewardType
{ 
    Currency = 1, // 재화
    Equipment = 2, // 장비
    Item = 3 // 아이템 (확장용)
}

[System.Serializable]
public class Reward
{
    public string rewardID;          // 아이템/재화 식별자 (ex "H0001", "S0001" 등)
    public RewardType rewardType;    // CSV에서 넘어오는 보상 타입 (Currency, Equipment, Item 등)
    public CurrencyType currencyType; // 재화형 보상일 경우 구분(선택적, 프로젝트에 따라 사용)
    public int rewardCount;          // 수량

    public string GetDisplayName()
    {
        if (rewardType == RewardType.Currency)
        {
            return $"{currencyType} x{rewardCount}";
        }
        else
        {
            return $"{rewardType} {rewardID} x{rewardCount}";
        }
    }
}