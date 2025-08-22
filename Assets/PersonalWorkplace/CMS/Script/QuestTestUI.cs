using System.Collections.Generic;
using UnityEngine;

public class QuestTestUI : MonoBehaviour
{
    private int currentQuestIndex = 0;
    private Quest currentQuest;

    private void Start()
    {
        // CSV에서 퀘스트 불러오기
        QuestDatabase db = FindObjectOfType<QuestDatabase>();
        db.LoadFromCSV("quests");

        if (QuestDatabase.AllQuests.Count > 0)
        {
            LoadQuest(0); // 첫 퀘스트 로드
        }
        else
        {
            Debug.LogError("퀘스트 데이터가 없습니다!");
        }
    }

    private void LoadQuest(int index)
    {
        if (index >= QuestDatabase.AllQuests.Count)
        {
            Debug.Log("모든 퀘스트를 완료했습니다!");
            return;
        }

        currentQuestIndex = index;
        currentQuest = QuestDatabase.AllQuests[index];

        Debug.Log($"새 퀘스트 시작! {currentQuest.questName}, 목표 {currentQuest.valueGoal}");
    }

    // 진행도 증가
    public void OnAddProgress()
    {
        if (currentQuest == null) return;

        if (currentQuest.isComplete)
        {
            Debug.Log("이미 완료된 퀘스트입니다!");
            return;
        }

        currentQuest.valueProgress++;
        Debug.Log($"퀘스트 진행도: {currentQuest.valueProgress}/{currentQuest.valueGoal}");

        if (currentQuest.valueProgress >= currentQuest.valueGoal)
        {
            currentQuest.isComplete = true;
            Debug.Log($"퀘스트 완료! → {currentQuest.questName}");
        }

        BackendManager.Instance.SaveQuest(currentQuest);
    }

    // 보상 수령
    public void OnClaimReward()
    {
        if (currentQuest == null) return;

        if (!currentQuest.isComplete)
        {
            Debug.Log("아직 퀘스트 완료가 안됐습니다!");
            return;
        }

        if (currentQuest.isClaimed)
        {
            Debug.Log("이미 보상을 수령했습니다!");
            return;
        }

        currentQuest.isClaimed = true;
        Debug.Log($"보상 수령 완료: {currentQuest.rewardType} x {currentQuest.rewardCount}");

        BackendManager.Instance.SaveQuest(currentQuest);

        //다음 퀘스트 자동 로드
        LoadQuest(currentQuestIndex + 1);
    }
}