using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    public int ClearedStage { get; private set; } = 1;
    public double Gold { get; private set; } = 0; // 로컬 골드 관리

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
    public void ApplyData(int stage, double gold)
    {
        ClearedStage = stage;
        Gold = gold;
        Debug.Log($"[데이터 적용 완료] Stage={stage}, Gold={gold}");
    }

    // 골드 추가
    public void AddGold(double amount)
    {
        Gold += amount;
        Debug.Log($"골드 추가: +{amount}, 현재 골드: {Gold}");
    }

    // Stage 업데이트, 서버 저장
    public void SetClearedStage(int stage)
    {
        if (stage > ClearedStage)
            ClearedStage = stage;

        BackendManager.Instance.UpdatePlayerData(ClearedStage, Gold);
    }
}
