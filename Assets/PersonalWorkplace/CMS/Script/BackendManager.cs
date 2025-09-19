using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections.Generic;
using System.Collections;
using System;

public class BackendManager : MonoBehaviour
{
    public static BackendManager Instance { get; private set; }
    public static FirebaseApp FirebaseApp { get; private set; }
    public static FirebaseAuth Auth { get; private set; }
    public static FirebaseDatabase Database { get; private set; }

    private float autoSaveInterval = 30f;
    public event Action OnFirebaseReady;

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
        StartCoroutine(AutoSaveRoutine());
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp = FirebaseApp.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;
                Database = FirebaseDatabase.DefaultInstance;

                Debug.Log("Firebase 초기화 완료!");
                OnFirebaseReady?.Invoke(); // Firebase 준비 완료 시 알림
            }
            else
            {
                Debug.LogError($"Firebase 해결 실패: {dependencyStatus}");
            }
        });
    }

    private void InitializeGPGS()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        Debug.Log("GPGS 초기화 완료, 자동 로그인 시도...");
        SignInWithGPGS();
    }

    public void SignInWithGPGS()
    {
        if (Auth == null)
        {
            Debug.LogError("⚠Firebase Auth 초기화 안 됨. 로그인 시도 중단");
            return;
        }

        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("GPGS 로그인 성공!");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, authCode =>
                {
                    if (string.IsNullOrEmpty(authCode))
                    {
                        Debug.LogError("GPGS 서버 인증 코드 받기 실패");
                        return;
                    }

                    Debug.Log("GPGS 서버 인증 코드 수신 완료");
                    Credential credential = PlayGamesAuthProvider.GetCredential(authCode);

                    Auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCanceled || task.IsFaulted)
                        {
                            Debug.LogError("Firebase 자격증명 로그인 실패: " + task.Exception);
                            return;
                        }

                        FirebaseUser newUser = task.Result;
                        Debug.Log($"Firebase 로그인 성공! UID: {newUser.UserId}, DisplayName: {newUser.DisplayName}");

                        QuestManager.Instance?.InitializeAfterLogin();
                    });
                });
            }
            else
            {
                Debug.LogError("GPGS 로그인 실패: " + status);
            }
        });
    }

    private IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SafeSave();
        }
    }

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

    public void UpdatePlayerData(int clearedStage, BigCurrency gold)
    {
        if (Auth.CurrentUser == null)
        {
            Debug.LogWarning("로그인된 유저 없음, 저장 불가");
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

    private void OnApplicationQuit()
    {
        SafeSave();
    }
}