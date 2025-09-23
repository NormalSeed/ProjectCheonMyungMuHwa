using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AttendanceManager : MonoBehaviour
{
    public static AttendanceManager Instance;

    [Header("Config")]
    public AttendanceCSVLoader csvLoader;
    public int totalDays = 14;

    private List<AttendanceReward> rewardTable;
    private DatabaseReference dbRef;
    private string uid;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private async void Start()
    {
        if (csvLoader == null)
        {
            Debug.LogError("[Attendance] CSV Loader가 인스펙터에 연결되지 않았습니다!");
            return;
        }

        rewardTable = csvLoader.LoadRewards();
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

        await Task.Yield();
        AttendanceUIManager.Instance?.ShowUI();
    }



    //오늘이 몇일차 출석인지 계산 (14일 루프 + 매월 1일 초기화)
    private int GetTodayIndex(DateTime serverDate)
    {
        // 매월 1일이면 무조건 Day1부터 시작
        if (serverDate.Day == 1)
            return 1;

        // 기준일(예시: 2025-01-01)로부터 며칠째인지
        int todayDay = (int)(serverDate.Date - new DateTime(2025, 1, 1)).TotalDays + 1;
        return ((todayDay - 1) % totalDays) + 1;
    }

    // 오늘 보상 자동 수령 (최초 접속 시)
    public async Task<bool> CheckAndClaimTodayReward()
    {
        DateTime serverDate = await ServerTimeManager.GetServerTime();
        int todayIndex = GetTodayIndex(serverDate);

        string todayKey = serverDate.ToString("yyyyMMdd");
        string lastDate = PlayerPrefs.GetString("LastAttendanceDate", "");

        // 이미 오늘 수령함
        if (todayKey == lastDate)
            return false;

        await ClaimReward(todayIndex);

        // 로컬 저장
        PlayerPrefs.SetString("LastAttendanceDate", todayKey);
        PlayerPrefs.SetInt("LastAttendanceDay", todayIndex);
        PlayerPrefs.Save();
        PlayerPrefs.DeleteKey("LastAttendanceDate");

        // 서버 저장
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

    // 특정 일차 보상 지급 
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
            switch (reward.rewardType)
            {
                case RewardType.Currency:
                    if (CurrencyManager.Instance == null)
                    {
                        Debug.LogError("[Attendance] CurrencyManager.Instance 가 초기화되지 않음");
                        continue;
                    }
                    if (reward.currencyType != null)
                        CurrencyManager.Instance.Model.Add(reward.currencyType.Value, new BigCurrency(reward.rewardCount, 0));
                    break;

                case RewardType.Item:
                case RewardType.Equipment:
                    if (InventoryManager.Instance == null)
                    {
                        Debug.LogError("[Attendance] InventoryManager.Instance 가 초기화되지 않음");
                        continue;
                    }
                    InventoryManager.Instance.Add(reward.rewardID, reward.rewardCount);
                    break;
            }
        }

        Debug.Log($"[Attendance] {day}일차 보상 지급 완료");
        await Task.CompletedTask;
    }
    // 7일차 광고 보상 지급 (7~14일차 허용)
    public async Task ClaimAdBonus(int day)
    {
        if (day < 7 || day > totalDays) return;

        // CSV에서 "AdBonus" 같은 RewardType 따로 관리 가능
        var rewardData = GetRewardForDay(day);
        if (rewardData == null) return;

        foreach (var reward in rewardData.rewards)
        {
            if (reward.rewardID.Contains("AdBonus")) // CSV에서 특별 보상 구분
            {
                InventoryManager.Instance.Add(reward.rewardID, reward.rewardCount);
                Debug.Log($"[Attendance] {day}일차 광고 보상 지급 완료");
            }
        }

        await Task.CompletedTask;
    }

    // 보상 데이터 가져오기
    public AttendanceReward GetRewardForDay(int day)
    {
        if (rewardTable == null || rewardTable.Count == 0)
        {
            Debug.LogError("[Attendance] rewardTable이 비어 있음");
            return null;
        }

        int cycleDay = ((day - 1) % totalDays) + 1;
        var reward = rewardTable.Find(r => r.day == cycleDay);

        if (reward == null)
            Debug.LogWarning($"[Attendance] {cycleDay}일차 보상을 찾을 수 없음 (CSV 확인 필요)");

        return reward;
    }

    public int GetRewardForDayIndex(DateTime serverTime)
    {
        // 기준일 (예: 2025-01-01)
        int todayDay = (int)(serverTime.Date - new DateTime(2025, 1, 1)).TotalDays + 1;
        return ((todayDay - 1) % totalDays) + 1; // 1~14 사이로 변환
    }
}