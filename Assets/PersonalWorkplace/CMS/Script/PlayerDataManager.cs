using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    public int ClearedStage { get; private set; } = 1;
    public BigCurrency Gold => CurrencyManager.Instance.Get(CurrencyType.Gold);

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

        BackendManager.Instance.UpdatePlayerData(ClearedStage, Gold); // double 대신 BigCurrency
    }
}
