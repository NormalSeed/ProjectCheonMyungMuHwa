using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AttendanceManager : MonoBehaviour
{
    public static AttendanceManager Instance;

    [SerializeField] private AttendanceCSVLoader csvLoader;
    private Dictionary<int, AttendanceReward> rewardDict = new();
    private const int totalDays = 14;

    private DatabaseReference dbRef;
    private string uid;

    public event Action OnAttendanceUpdated;

    private DateTime cachedFirstLogin;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private async void Start()
    {
        // 보상 테이블 로드
        foreach (var reward in csvLoader.LoadRewards())
            rewardDict[reward.day] = reward;

        uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId ?? "dev-local-test";
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        await EnsureFirstLoginDate();
        await RefreshAttendance();
    }

    // 최초 로그인일 없으면 서버 기준으로 저장
    private async Task EnsureFirstLoginDate()
    {
        var snapshot = await dbRef.Child("users").Child(uid).Child("attendance/firstLoginDate").GetValueAsync();
        if (snapshot.Exists)
        {
            cachedFirstLogin = DateTime.Parse(snapshot.Value.ToString());
            return;
        }

        DateTime serverTime = await ServerTimeManager.GetServerTime();
        cachedFirstLogin = serverTime.Date;
        await dbRef.Child("users").Child(uid).Child("attendance/firstLoginDate").SetValueAsync(cachedFirstLogin.ToString("yyyy-MM-dd"));
    }

    // 오늘이 몇 일차인지 계산
    public int GetTodayIndex(DateTime serverTime)
    {
        int daysPassed = (serverTime.Date - cachedFirstLogin.Date).Days;
        return (daysPassed % totalDays) + 1;
    }

    public AttendanceReward GetRewardForDay(int day)
    {
        int cycleDay = ((day - 1) % totalDays) + 1;
        return rewardDict.TryGetValue(cycleDay, out var reward) ? reward : null;
    }

    // 보상 수령
    public async Task ClaimTodayReward()
    {
        DateTime serverTime = await ServerTimeManager.GetServerTime();
        int todayDay = GetTodayIndex(serverTime);

        var snapshot = await dbRef.Child("users").Child(uid).Child("attendance/lastDay").GetValueAsync();
        int lastDay = snapshot.Exists ? int.Parse(snapshot.Value.ToString()) : 0;

        if (todayDay <= lastDay)
        {
            Debug.Log("[Attendance] 이미 보상 수령 완료");
            return;
        }

        var rewardData = GetRewardForDay(todayDay);
        if (rewardData != null)
        {
            foreach (var reward in rewardData.rewards)
                GrantReward(reward);
        }

        await dbRef.Child("users").Child(uid).Child("attendance/lastDay").SetValueAsync(todayDay);
        Debug.Log($"[Attendance] {todayDay}일차 보상 지급 완료");

        await RefreshAttendance();
    }

    private void GrantReward(Reward reward)
    {
        switch (reward.rewardType)
        {
            case RewardType.Currency:
                if (reward.currencyType != null)
                    CurrencyManager.Instance.Model.Add(
                        reward.currencyType.Value,
                        new BigCurrency(reward.rewardCount, 0)
                    );
                break;
            case RewardType.Item:
            case RewardType.Equipment:
                InventoryManager.Instance.Add(reward.rewardID, reward.rewardCount);
                break;
        }
    }

    private async Task RefreshAttendance()
    {
        OnAttendanceUpdated?.Invoke();
    }

    public async Task<bool> IsClaimed(int day)
    {
        var snapshot = await dbRef.Child("users").Child(uid).Child("attendance/lastDay").GetValueAsync();
        int lastDay = snapshot.Exists ? int.Parse(snapshot.Value.ToString()) : 0;
        return day <= lastDay;
    }
}