using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class AttendanceManager : MonoBehaviour
{
    public static AttendanceManager Instance;

    [Header("Config")]
    public AttendanceCSVLoader _csvLoader;
    public int totalDays = 14;

    private List<AttendanceReward> rewardTable;
    private DatabaseReference dbRef;
    private string uid;
    private TableManager _tableManager;

    private const string AdRewardClaimedDay7Key = "AdRewardClaimed_Day7";
    private const string AdRewardClaimedDay14Key = "AdRewardClaimed_Day14";

    [Inject]
    public void Construct(AttendanceCSVLoader csvLoader, TableManager tableManager)
    {
        _csvLoader = csvLoader;
        Debug.Log("[AttendanceManager] Construct 호출됨, TableManager 주입 완료");
        _tableManager = tableManager;
    }


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public async void Start()
    {
        Debug.Log("[AttendanceManager] Start() 진입");

        int safety = 0;
        while (_tableManager == null || !_tableManager.AllInitialized)
        {
            safety++;
            if (safety % 100 == 0) // 100프레임마다 한 번 찍기
            {
                Debug.Log($"[AttendanceManager] 대기중... _tableManager={_tableManager != null}, AllInitialized={_tableManager?.AllInitialized}");
            }
            await Task.Yield();
        }

        Debug.Log("[AttendanceManager] TableManager 준비 완료");

        rewardTable = _csvLoader.LoadRewards();
        Debug.Log($"[AttendanceManager] rewardTable.Count={rewardTable?.Count}");
        if (rewardTable == null || rewardTable.Count == 0)
        {
            Debug.LogError("[Attendance] CSV 로딩 실패 - rewardTable이 비어있음");
            return;
        }

        uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        bool claimed = await CheckAndClaimTodayReward();
        if (claimed)
        {
            Debug.Log("[Attendance] 오늘 출석 보상 자동 수령됨");
        }

        AttendanceUIManager.Instance?.ShowUI();
    }

    private int GetTodayIndex(int lastDay, DateTime serverDate)
    {
        // 1. 매월 1일이면 무조건 1일차로 초기화 (최우선 순위)
        if (serverDate.Day == 1)
        {
            return 1;
        }

        string lastDateStr = PlayerPrefs.GetString("LastAttendanceDate", "");
        if (DateTime.TryParseExact(lastDateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime lastDate))
        {
            int daysPassed = (int)(serverDate.Date - lastDate.Date).TotalDays;

            // 하루가 지났으면 출석일 +1
            if (daysPassed == 1)
            {
                // 14일차를 받고 다음날이 되면 1일차로 초기화
                return (lastDay % totalDays) + 1;
            }
            // 이틀 이상 접속 안했으면 마지막 출석일 다음날로 처리
            if (daysPassed > 1)
            {
                return (lastDay % totalDays) + 1;
            }
        }

        // 데이터가 없는 최초 접속 시 1일차로 처리
        return lastDay > 0 ? lastDay : 1;
    }

    public async Task<bool> CheckAndClaimTodayReward()
    {
        DateTime serverDate = await ServerTimeManager.GetServerTime();

        string todayKey = serverDate.ToString("yyyyMMdd");
        string lastDate = PlayerPrefs.GetString("LastAttendanceDate", "");

        // 이미 오늘 수령했다면 아무것도 하지 않음
        if (todayKey == lastDate)
        {
            return false;
        }

        int lastDay = PlayerPrefs.GetInt("LastAttendanceDay", 0);
        int todayIndex = GetTodayIndex(lastDay, serverDate);

        if (lastDay > 0 && todayIndex == 1)
        {
            ResetAdRewardStatus();
            Debug.Log("[Attendance] 출석 사이클 초기화. 광고 보상 기록도 초기화됩니다.");
        }

        await ClaimReward(todayIndex);

        PlayerPrefs.SetString("LastAttendanceDate", todayKey);
        PlayerPrefs.SetInt("LastAttendanceDay", todayIndex);
        PlayerPrefs.Save();

        if (!string.IsNullOrEmpty(uid))
        {
            var data = new Dictionary<string, object>
            {
                { "lastDay", todayIndex },
                { "lastClaimedDate", todayKey }
            };
            await dbRef.Child("users").Child(uid).Child("attendance").UpdateChildrenAsync(data);
        }

        return true;
    }

    public async Task ClaimAdBonus(int day)
    {
        int currentDay = PlayerPrefs.GetInt("LastAttendanceDay", 0);

        // 기획: 7일차 광고 보상은 7~14일차 사이에 받을 수 있음
        if (day == 7 && currentDay >= 7 && !IsAdRewardClaimed(7))
        {
            var rewardData = GetRewardForDay(day); // 7일차 보상 테이블 가져오기
            if (rewardData == null) return;

            // 실제 보상 지급 로직
            // 예시: InventoryManager.Instance.Add("AdBonus_Day7", 1);
            Debug.Log($"[Attendance] 7일차 광고 보상 지급 완료!");
            MarkAdRewardAsClaimed(7); // 수령 상태 저장
        }
        // 기획: 14일차 광고 보상은 14일차에만 가능
        else if (day == 14 && currentDay == 14 && !IsAdRewardClaimed(14))
        {
            Debug.Log($"[Attendance] 14일차 광고 보상 지급 완료!");
            MarkAdRewardAsClaimed(14); // 수령 상태 저장
        }
        else
        {
            Debug.LogWarning($"[Attendance] 광고 보상 수령 조건 미충족. Day: {day}, CurrentDay: {currentDay}");
            return;
        }

        // UI 갱신 요청
        AttendanceUIManager.Instance?.ShowUI();
        await Task.CompletedTask;
    }

    public bool IsAdRewardClaimed(int day)
    {
        if (day == 7) return PlayerPrefs.GetInt(AdRewardClaimedDay7Key, 0) == 1;
        if (day == 14) return PlayerPrefs.GetInt(AdRewardClaimedDay14Key, 0) == 1;
        return true; // 7, 14가 아니면 항상 받은 것으로 처리
    }

    public void MarkAdRewardAsClaimed(int day)
    {
        if (day == 7) PlayerPrefs.SetInt(AdRewardClaimedDay7Key, 1);
        if (day == 14) PlayerPrefs.SetInt(AdRewardClaimedDay14Key, 1);
        PlayerPrefs.Save();
    }

    private void ResetAdRewardStatus()
    {
        PlayerPrefs.SetInt(AdRewardClaimedDay7Key, 0);
        PlayerPrefs.SetInt(AdRewardClaimedDay14Key, 0);
        PlayerPrefs.Save();
    }

    public int GetCurrentAttendanceDay()
    {
        return PlayerPrefs.GetInt("LastAttendanceDay", 0);
    }

    public async Task ClaimReward(int day)
    {
        var rewardData = GetRewardForDay(day);
        if (rewardData == null || rewardData.rewards == null)
        {
            Debug.LogError($"[Attendance] {day}일차 보상 데이터가 없음");
            return;
        }

        foreach (var reward in rewardData.rewards)
        {
            var itemTable = _tableManager.GetTable<TItem>(TableType.Item);
            ItemData item = itemTable?.GetItem(reward.itemID);

            if (item == null)
            {
                Debug.LogError($"[Attendance] 보상 아이템 ID {reward.itemID} 를 TItem에서 찾을 수 없음");
                continue;
            }

            int amount = (int)reward.amount;

            // 📌 CurrencyType 매핑 가능한 경우 (재화 지급)
            CurrencyType? currency = TryParseCurrency(item.ImageKey);
            if (currency.HasValue)
            {
                CurrencyManager.Instance?.Add(currency.Value, new BigCurrency(amount, reward.amountTier));
                Debug.Log($"[Attendance] {day}일차 보상 지급: {currency.Value} x{amount}");
            }
            else
            {
                InventoryManager.Instance?.Add(item.Id.ToString(), amount);
                Debug.Log($"[Attendance] {day}일차 보상 지급: {item.Name} x{amount}");
            }
        }

        await Task.CompletedTask;
    }

    public AttendanceReward GetRewardForDay(int day)
    {
        if (rewardTable == null || rewardTable.Count == 0) return null;
        return rewardTable.Find(r => r.day == day);
    }
    private CurrencyType? TryParseCurrency(string itemImage)
    {
        // CurrencyType enum 과 Item_Image 매칭
        return itemImage switch
        {
            "금화" => CurrencyType.Gold,
            "혼백" => CurrencyType.Soul,
            "용옥" => CurrencyType.Jewel,
            "영석" => CurrencyType.SpiritStone,
            "등용패" => CurrencyType.SummonTicket,
            "장비패" => CurrencyType.InvitationTicket,
            //"연마석" => CurrencyType.GrindStone,
            "금화 던전 도전장" => CurrencyType.GoldChallengeTicket,
            "혼백 던전 도전장" => CurrencyType.SoulChallengeTicket,
            "영석 던전 도전장" => CurrencyType.SpiritStoneChallengeTicket,
            _ => null
        };
    }
}
