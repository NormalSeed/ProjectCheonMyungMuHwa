using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Offline Reward UI")]
    public GameObject offlinePopup;
    public TextMeshProUGUI rewardText;
    public Button claimButton;

    [Header("Systems")]
    [SerializeField] private OfflineRewardSystem rewardSystem;

    private const string LastLogoutKey = "LastLogoutTime";
    private readonly TimeSpan MaxAllowedOffline = TimeSpan.FromHours(8); // 동일한 상한

    private Dictionary<CurrencyType, int> _pendingRewards;

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
            return;
        }
    }

    private void Start()
    {
        // 앱 시작 시 저장된 로그아웃 시간을 미리 처리하려고 하지만,
        // CurrencyManager 초기화가 끝나야 실제 지급이 가능하므로 대기 로직 구현
        if (CurrencyManager.Instance != null && CurrencyManager.Instance.IsInitialized)
        {
            LoadOfflineReward();
        }
        else
        {
            // CurrencyManager.OnInitialized 를 통해 초기화 완료 시 호출되도록 구독
            CurrencyManager.OnInitialized += OnCurrencyManagerInitialized;
        }
    }

    private void OnDestroy()
    {
        CurrencyManager.OnInitialized -= OnCurrencyManagerInitialized;
    }

    private void OnCurrencyManagerInitialized()
    {
        // 안전하게 한 번만 처리
        CurrencyManager.OnInitialized -= OnCurrencyManagerInitialized;
        LoadOfflineReward();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveLastLogoutTime();
    }

    private void OnApplicationQuit()
    {
        SaveLastLogoutTime();
    }

    private void SaveLastLogoutTime()
    {
        // ISO 8601 포맷(원형)로 저장
        PlayerPrefs.SetString(LastLogoutKey, DateTime.UtcNow.ToString("o"));
        PlayerPrefs.Save();
    }

    private void LoadOfflineReward()
    {
        if (!PlayerPrefs.HasKey(LastLogoutKey))
            return;

        string raw = PlayerPrefs.GetString(LastLogoutKey);
        if (!DateTime.TryParse(raw, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime lastTime))
        {
            Debug.LogWarning("[GameManager] LastLogoutTime 파싱 실패");
            return;
        }

        TimeSpan offlineDuration = DateTime.UtcNow - lastTime;

        // 방어 로직: 음수 => 0, 상한 적용
        if (offlineDuration.TotalSeconds < 0)
        {
            Debug.LogWarning("[GameManager] 디바이스 시간 조작 의심(음수 경과) - 0으로 보정");
            offlineDuration = TimeSpan.Zero;
        }
        if (offlineDuration > MaxAllowedOffline)
        {
            // 선택: 상한으로 자르거나 특정 보상 상한 적용
            offlineDuration = MaxAllowedOffline;
        }

        // 플레이어가 클리어한 스테이지 가져오기 (null-safe)
        int clearedStage = 1;
        if (PlayerDataManager.Instance != null)
            clearedStage = PlayerDataManager.Instance.ClearedStage;

        var rewards = rewardSystem.CalculateRewards(offlineDuration, clearedStage);
        if (rewards == null || rewards.Count == 0)
            return;

        _pendingRewards = rewards;
        ShowOfflineRewardPopup(rewards);
    }

    private void ShowOfflineRewardPopup(Dictionary<CurrencyType, int> rewards)
    {
        if (offlinePopup == null || rewardText == null || claimButton == null)
        {
            Debug.LogError("[GameManager] 오프라인 팝업 UI 참조가 없음");
            return;
        }

        offlinePopup.SetActive(true);

        // UI 텍스트 갱신 (예: "100,000 Gold\n50 Jewel")
        rewardText.text = string.Join("\n", rewards.Select(r => $"{r.Value:N0} {r.Key}"));

        // 안전하게 기존 리스너 제거 후 등록
        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() =>
        {
            TryGrantOfflineRewards();
        });
    }

    private void TryGrantOfflineRewards()
    {
        if (_pendingRewards == null || _pendingRewards.Count == 0)
        {
            offlinePopup.SetActive(false);
            return;
        }

        // CurrencyManager가 초기화 되었는지 확인
        if (CurrencyManager.Instance == null || !CurrencyManager.Instance.IsInitialized)
        {
            Debug.LogWarning("[GameManager] CurrencyManager가 준비되지 않아 지급 실패, 나중에 시도");
            // UX: 사용자에게 메시지 띄워도 좋음
            return;
        }

        // 지급
        foreach (var kvp in _pendingRewards)
        {
            var type = kvp.Key;
            var amount = kvp.Value;
            if (amount <= 0) continue;

            // BigCurrency 타입으로 포장해서 지급 (tier 0 기본)
            CurrencyManager.Instance.Add(type, new BigCurrency(amount, 0));
        }

        // 지급 완료: 팝업 닫고 중복 지급 방지 위해 마지막 로그아웃시간을 '지금'으로 갱신
        SaveLastLogoutTime();

        _pendingRewards = null;
        offlinePopup.SetActive(false);

        Debug.Log("[GameManager] 오프라인 보상 지급 완료");
    }
}