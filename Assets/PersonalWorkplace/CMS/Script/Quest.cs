using System;
using Unity.VisualScripting;

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
    public string rewardID; //보상 ID
    public RewardType rewardType; //보상 유형 (장비/재화 등)
    public int rewardCount; //보상 수량

    //리셋 관리
    public DateTime lastUpdated; //마지막 갱신
    public int lastWeek; //마지막 갱신 주차 

    public Quest() { }

    public Quest(string id, string name, QuestCategory type, QuestTargetType target, int goal, string rewardId, RewardType rewardType, int rewardCount)
{
    questID = id;
    questName = name;
    questType = type;
    questTarget = target;
    valueProgress = 0;
    valueGoal = goal;
    isComplete = false;
    isClaimed = false;
    rewardID = rewardId;
    this.rewardType = rewardType;
    this.rewardCount = rewardCount;
    
    this.lastUpdated = DateTime.UtcNow;
    this.lastWeek = GetCurrentWeek(DateTime.UtcNow);
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
    lastUpdated = DateTime.UtcNow;
}

//퀘스트 리셋
public void ResetProgress()
{
    valueProgress = 0;
    isComplete = false;
    isClaimed = false;
    lastUpdated = DateTime.UtcNow;
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
    Daily = 1, // 일일 퀘스트
    Weekly = 2, // 주간 퀘스트
    Repeatable = 3 // 반복 퀘스트 (확장용)
}

public enum QuestTargetType
{
    Collect = 1, // 아이템/재화 수집
    Kill = 2, // 몬스터 처치
    Explore = 3 // 탐험 (확장용)
}

public enum RewardType
{ 
    Currency = 1, // 재화
    Equipment = 2, // 장비
    Item = 3 // 아이템 (확장용)
}