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