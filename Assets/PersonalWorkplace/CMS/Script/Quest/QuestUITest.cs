using UnityEngine;

public class QuestUITest : MonoBehaviour
{
    public QuestUIController questUI;  // 인스펙터에 연결
    private Quest testQuest;

    void Start()
    {
        // 가짜 퀘스트 생성
        testQuest = new Quest(
            id: "Q001",
            name: "몬스터 10마리 처치",
            type: QuestCategory.Daily,
            target: QuestTargetType.Monster,
            goal: 10,
            rewardId: "GOLD001",
            currencyType: CurrencyType.Gold,
            rewardCount: 500
        );

        testQuest.state = QuestState.InProgress;
        testQuest.valueProgress = 5;

        questUI.SetData(testQuest);
    }

    void Update()
    {
        // Q 키 → 퀘스트 클리어 (보상 받을 준비)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            testQuest.valueProgress = 10;
            testQuest.state = QuestState.RewardReady;
            questUI.SetData(testQuest);
        }

        // R 키 → 보상 수령 완료
        if (Input.GetKeyDown(KeyCode.R))
        {
            QuestManager.Instance.ClaimReward(testQuest);
            questUI.SetData(testQuest);

            var goldNow = CurrencyManager.Instance.Model.Get(CurrencyType.Gold);
            Debug.Log($"[테스트] 보상 수령 후 골드: {goldNow}");
        }
    }
}