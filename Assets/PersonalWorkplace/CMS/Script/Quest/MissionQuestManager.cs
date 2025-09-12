using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class MissionQuestManager : MonoBehaviour
{
    public static MissionQuestManager Instance { get; private set; }

    public Quest CurrentMission { get; private set; }
    private Dictionary<string, Quest> missionQuests = new Dictionary<string, Quest>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void LoadMissions()
    {
        var missions = QuestManager.Instance.GetQuestsByCategory(QuestCategory.Mission);
        missionQuests.Clear();
        foreach (var q in missions)
            missionQuests[q.questID] = q;

        CurrentMission = missions.FirstOrDefault(q => q.state == QuestState.InProgress || q.state == QuestState.RewardReady);
        if (CurrentMission != null)
        {
            Debug.Log($"현재 미션 퀘스트: {CurrentMission.questName}");
        }
        QuestManager.Instance?.NotifyQuestsUpdated();
    }

    public void TryActivateMission(int stage)
    {
        if (CurrentMission == null) return;

        if (CurrentMission.state == QuestState.Locked &&
            stage >= CurrentMission.requiredStage) // Stage만 조건 체크
        {
            CurrentMission.state = QuestState.InProgress;
            Debug.Log($"미션 활성화됨: {CurrentMission.questName}");

            // QuestManager에 갱신 이벤트 알리기
            QuestManager.Instance?.NotifyQuestsUpdated();
        }
    }

    public void ReportProgress(QuestTargetType target, int amount = 1)
    {
        if (CurrentMission == null) return;
        if (CurrentMission.questTarget == target)
            CurrentMission.AddProgress(amount);
    }

    public void ClaimReward()
    {
        if (CurrentMission == null || CurrentMission.state != QuestState.Completed) return;

        foreach (var reward in CurrentMission.rewards)
            CurrencyManager.Instance.Add(reward.currencyType, new BigCurrency(reward.rewardCount, 0));

        CurrentMission.ClaimReward();
        Debug.Log($"보상 수령: {CurrentMission.questName}");

        // 다음 미션 진행
        if (!string.IsNullOrEmpty(CurrentMission.nextQuestID) &&
            missionQuests.TryGetValue(CurrentMission.nextQuestID, out var next))
        {
            CurrentMission = next;
            Debug.Log($"다음 미션 시작: {CurrentMission.questName}");
        }
        else
        {
            Debug.Log("모든 미션 퀘스트 완료!");
            CurrentMission = null;
        }

        QuestManager.Instance?.NotifyQuestsUpdated();
    }
}
