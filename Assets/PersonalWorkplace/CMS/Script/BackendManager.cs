using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BackendManager : MonoBehaviour
{
    public static BackendManager Instance { get; private set; }
    public static FirebaseApp FirebaseApp { get; private set; }
    public static FirebaseAuth Auth { get; private set; }
    public static FirebaseDatabase Database { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        if (OfflineRewardSystem.Instance == null)
        {
            var go = new GameObject("OfflineRewardSystem");
            go.AddComponent<OfflineRewardSystem>();
        }
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Firebase 의존성 확인 실패");
                return;
            }

            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp = FirebaseApp.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;
                Database = FirebaseDatabase.DefaultInstance;

                Debug.Log("Firebase 초기화 완료!");
                SignInAnonymously();
            }
            else
            {
                Debug.LogError($"Firebase 해결 실패: {dependencyStatus}");
            }
        });
    }

    // 익명 로그인
    private void SignInAnonymously()
    {
        Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("익명 로그인 실패: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result.User;
            Debug.Log($"익명 로그인 성공 UID: {newUser.UserId}");

            if (QuestManager.Instance != null)
                QuestManager.Instance.InitializeAfterLogin();
        });
    }

    // PlayerData 저장
    public void UpdatePlayerData(int clearedStage, double gold)
    {
        if (Auth.CurrentUser == null)
        {
            Debug.LogWarning("로그인된 유저 없음 → 저장 불가");
            return;
        }

        string uid = Auth.CurrentUser.UserId;
        DatabaseReference userRef = Database.RootReference.Child("players").Child(uid);

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "clearedStage", clearedStage },
            { "gold", gold },
            { "lastLogoutTime", System.DateTime.UtcNow.ToString("o") }
        };

        userRef.UpdateChildrenAsync(updates).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
                Debug.LogError("플레이어 데이터 저장 실패: " + task.Exception);
            else
                Debug.Log($"[서버 저장 완료] Stage={clearedStage}, Gold={gold}");
        });
    }

    // PlayerData 불러오기
    public void LoadPlayerData(Action<int, double, Dictionary<CurrencyType, int>> onLoaded)
    {
        if (Auth.CurrentUser == null)
        {
            onLoaded?.Invoke(1, 0, new Dictionary<CurrencyType, int>());
            return;
        }

        string uid = Auth.CurrentUser.UserId;
        DatabaseReference userRef = Database.RootReference.Child("players").Child(uid);

        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                onLoaded?.Invoke(1, 0, new Dictionary<CurrencyType, int>());
                return;
            }

            DataSnapshot snapshot = task.Result;
            int stage = 1;
            double gold = 0;
            DateTime lastLogout = DateTime.UtcNow;

            if (snapshot.Exists)
            {
                if (snapshot.HasChild("clearedStage"))
                    stage = int.Parse(snapshot.Child("clearedStage").Value.ToString());
                if (snapshot.HasChild("gold"))
                    gold = double.Parse(snapshot.Child("gold").Value.ToString());
                if (snapshot.HasChild("lastLogoutTime"))
                    DateTime.TryParse(snapshot.Child("lastLogoutTime").Value.ToString(), out lastLogout);
            }

            // 오프라인 보상 계산 (서버 시간 기준)
            TimeSpan offlineDuration = DateTime.UtcNow - lastLogout;
            offlineDuration = offlineDuration.TotalHours > 8 ? TimeSpan.FromHours(8) : offlineDuration;

            var rewards = OfflineRewardSystem.Instance.CalculateRewards(offlineDuration, stage);
            double totalGold = gold + rewards
                .Where(r => r.Key == CurrencyType.Gold)
                .Sum(r => r.Value);

            // 서버에 새 데이터 저장
            UpdatePlayerData(stage, totalGold);

            // 로컬 반영
            PlayerDataManager.Instance.ApplyData(stage, totalGold);
            onLoaded?.Invoke(stage, totalGold, rewards);
        });
    }

    // 앱 종료 시 저장
    private void OnApplicationQuit()
    {
        if (Auth.CurrentUser != null)
        {
            UpdatePlayerData(PlayerDataManager.Instance.ClearedStage, PlayerDataManager.Instance.Gold);
        }
    }
}
