using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public Dictionary<string, Quest> activeQuests = new Dictionary<string, Quest>();
    private DatabaseReference dbRef;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void Start()
    {
        if (BackendManager.Auth != null && BackendManager.Database != null)
        {
            dbRef = BackendManager.Database.RootReference;
            LoadQuests();
        }
    }

    //퀘스트 시작
    public void StartQuest(Quest quest)
    {
        if (!activeQuests.ContainsKey(quest.questID))
        {
            activeQuests.Add(quest.questID, quest);
            Debug.Log($"퀘스트 이미 진행 중: {quest.questName}");
            SaveQuests();
        }
    }

    //진행 업데이트
    public void UpdateQuest(string questId, int amount = 1)
    {
        if (activeQuests.TryGetValue(questId, out Quest quest))
        {
            if (quest.isComplete) return; // 이미 완료된 퀘스트는 업데이트하지 않음

            quest.valueProgress += amount;

            if (quest.valueProgress >= quest.valueGoal)
            {
                quest.valueProgress = quest.valueGoal; // 목표 수치 초과 방지
                CompleteQuest(quest);
            }
        }
        Debug.Log($"퀘스트 진행: {quest.questName}, 현재 진행: {quest.valueProgress}/{quest.valueGoal}");
        SaveQuests();
    }

    //완료 처리
    private void CompleteQuest(Quest quest)
    {
        quest.isComplete = true;
        Debug.Log($"퀘스트 완료: {quest.questName}");

        SaveQuests();
    }

    private void ClaimReward(string questID)
    {
        if (activeQuests.TryGetValue(questID, out Quest quest))
        {
            if (quest.isComplete && !quest.isClaimed)
            {
                quest.isClaimed = true;
                Debug.Log($"퀘스트 보상 수령: {quest.rewardID} x {quest.rewardCount}");
                // 보상 지급 로직 예시
                // PlayerManager.Instance.AddReward(quest.rewardID, quest.rewardType, quest.rewardCount);
                // 퀘스트 UI에 보상 받기 버튼 추가
                // 버튼 클릭시 ClaimReward(questID) 호출

                SaveQuests();
            }
        }
    }

    private void SaveQuests()
    {
        string userId = BackendManager.Auth.CurrentUser.UserId;
        QuestList wrapper = new QuestList(activeQuests.Values);
        string json = JsonUtility.ToJson(wrapper);

        dbRef.Child("users").Child(userId).Child("quests").SetRawJsonValueAsync(json);
    }

    private void LoadQuests()
    {
        string userId = BackendManager.Auth.CurrentUser.UserId;
        dbRef.Child("users").Child(userId).Child("quests")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("퀘스트 로드 실패");
                    return;
                }

                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    string json = snapshot.GetRawJsonValue();
                    QuestList wrapper = JsonUtility.FromJson<QuestList>(json);

                    activeQuests.Clear();
                    foreach (var quest in wrapper.quests)
                        activeQuests[quest.questID] = quest;

                    Debug.Log("퀘스트 로드 성공");
                }
            });
    }

    [System.Serializable]
    private class QuestList
    {
        public List<Quest> quests;
        public QuestList(IEnumerable<Quest> quests) =>
            this.quests = new List<Quest>(quests);

    }
}
