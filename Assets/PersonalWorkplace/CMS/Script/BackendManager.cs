using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

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
    public void LoadPlayerData(System.Action<int, double> onLoaded)
    {
        if (Auth.CurrentUser == null)
        {
            Debug.LogWarning("로그인된 유저 없음, 데이터 불러오기 실패");
            onLoaded?.Invoke(1, 0);
            return;
        }

        string uid = Auth.CurrentUser.UserId;
        DatabaseReference userRef = Database.RootReference.Child("players").Child(uid);

        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("데이터 불러오기 실패: " + task.Exception);
                onLoaded?.Invoke(1, 0);
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
                {
                    string lastLogoutStr = snapshot.Child("lastLogoutTime").Value.ToString();
                    if (DateTime.TryParse(lastLogoutStr, out DateTime parsedTime))
                        lastLogout = parsedTime;
                }
            }

            // 오프라인 보상 계산
            TimeSpan offlineDuration = DateTime.UtcNow - lastLogout;
            double reward = offlineDuration.TotalSeconds * 1; // 1초당 1골드 예시
            gold += reward;

            Debug.Log($"[오프라인 보상] {offlineDuration.TotalMinutes:F1}분 → {reward} 골드 지급!");

            // PlayerDataManager에 반영
            PlayerDataManager.Instance.ApplyData(stage, gold);

            // 최신 데이터 다시 서버에 저장
            UpdatePlayerData(stage, gold);

            onLoaded?.Invoke(stage, gold);
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
