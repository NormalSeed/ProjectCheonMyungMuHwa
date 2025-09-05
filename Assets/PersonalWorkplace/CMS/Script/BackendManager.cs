using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;

public class BackendManager : MonoBehaviour
{
    public static BackendManager Instance { get; private set; }
    public static FirebaseApp FirebaseApp { get; private set; }
    public static FirebaseAuth Auth { get; private set; }
    public static FirebaseDatabase Database { get; private set; }

    private float autoSaveInterval = 30f; // 30초마다 자동 저장

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

        // 주기적 자동 저장 시작
        StartCoroutine(AutoSaveRoutine());
    }

    private IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SafeSave();
        }
    }

    // 안전한 저장 메서드
    private void SafeSave()
    {
        if (Auth == null || Auth.CurrentUser == null) return;
        if (PlayerDataManager.Instance == null || CurrencyManager.Instance == null) return;

        UpdatePlayerData(
            PlayerDataManager.Instance.ClearedStage,
            CurrencyManager.Instance.Get(CurrencyType.Gold)
        );

        Debug.Log("[자동 저장 완료]");
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

            // CurrencyManager 강제 초기화
            if (CurrencyManager.Instance == null)
            {
                var model = new CurrencyModel(); // ICurrencyModel 구현체
                new CurrencyManager(model).Start();
            }

            QuestManager.Instance?.InitializeAfterLogin();
        });
    }

    public void UpdatePlayerData(int clearedStage, BigCurrency gold)
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
            { "gold", gold.ToString() },
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

    // 앱 종료 시에도 안전하게 시도
    private void OnApplicationQuit()
    {
        SafeSave();
    }
}