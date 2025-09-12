using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    public int ClearedStage { get; private set; } = 1;
    public BigCurrency Gold => CurrencyManager.Instance.Get(CurrencyType.Gold);

    public int Level { get; set; } = 1; // 임시, 나중에 경험치 시스템 나오면 수정

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        // 임시 테스트: 키보드 누르면 ClearedStage 증가
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            ClearedStage++;
            Debug.Log($"[테스트] ClearedStage 올림 {ClearedStage}");

            QuestManager.Instance.TryUnlockQuests();
        }

        // 임시 테스트: 키보드 누르면 ClearedStage 감소
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            ClearedStage = Mathf.Max(1, ClearedStage - 1);
            Debug.Log($"[테스트] ClearedStage 내림, {ClearedStage}");
            QuestManager.Instance.NotifyQuestsUpdated();
        }
    }

    // 서버에서 불러온 값 반영
    public void ApplyData(int stage, BigCurrency gold)
    {
        ClearedStage = stage;
        CurrencyManager.Instance.Set(CurrencyType.Gold, gold);
        Debug.Log($"[데이터 적용 완료] Stage={stage}, Gold={gold}");
    }

    // 골드 추가
    public void AddGold(BigCurrency amount)
    {
        CurrencyManager.Instance.Add(CurrencyType.Gold, amount);
        BackendManager.Instance.UpdatePlayerData(ClearedStage, Gold); // 이제 BigCurrency
    }

    // Stage 업데이트, 서버 저장
    public void SetClearedStage(int stage)
    {
        if (stage > ClearedStage)
            ClearedStage = stage;

        // Stage 조건만 체크
        MissionQuestManager.Instance?.TryActivateMission(ClearedStage);

        BackendManager.Instance.UpdatePlayerData(
            ClearedStage,
            CurrencyManager.Instance.Get(CurrencyType.Gold)
        );
    }
}
