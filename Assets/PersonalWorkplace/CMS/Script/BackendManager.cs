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

    private float autoSaveInterval = 30f; // 30초마다 자동 저장
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
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp = FirebaseApp.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;
                Database = FirebaseDatabase.DefaultInstance;

                OnFirebaseReady?.Invoke(); // 초기화 완료 알림
                TryGoogleLogin();
            }
        });
    }
    private void TryGoogleLogin()
    {
        Debug.Log("구글 로그인 시도...");
        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("GPGS 로그인 성공");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, (serverAuthCode) =>
                {
                    if (string.IsNullOrEmpty(serverAuthCode))
                    {
                        Debug.LogError("ServerAuthCode 획득 실패, 게스트 로그인 fallback");
                        SignInAnonymously();
                        return;
                    }

                    LinkWithGoogle(serverAuthCode);
                });
            }
            else
            {
                Debug.LogWarning("GPGS 로그인 실패, 게스트 로그인 fallback");
                SignInAnonymously();
            }
        });
    }

    public void SignInAnonymously()
    {
        if (Auth == null)
        {
            Debug.LogError("Firebase Auth 초기화 안됨");
            return;
        }

        Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("익명 로그인 실패: " + task.Exception);
                return;
            }

            Debug.Log($"익명 로그인 성공 UID={task.Result.User.UserId}");

            QuestManager.Instance?.InitializeAfterLogin();
        });
    }

    public void LinkWithGoogle(string serverAuthCode)
    {
        Debug.Log("구글 계정 연동 시도...");
        Credential credential = PlayGamesAuthProvider.GetCredential(serverAuthCode);

        if (Auth.CurrentUser == null)
        {
            Debug.LogError("Firebase CurrentUser 없음. 로그인 먼저 필요");
            return;
        }

        if (Auth.CurrentUser.IsAnonymous)
        {
            // 게스트, 구글 계정 승격
            Auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("구글 계정 연동 실패: " + task.Exception);
                    return;
                }

                Debug.Log("구글 계정 연동 성공! 기존 게스트 데이터 유지됨");
                QuestManager.Instance?.InitializeAfterLogin();
            });
        }
        else
        {
            // 이미 로그인된 계정이 있으면 그냥 구글 로그인 처리
            Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("익명 로그인 실패: " + task.Exception);
                    return;
                }

                // AuthResult 안에서 User 꺼내기
                FirebaseUser user = task.Result.User;

                Debug.Log($"익명 로그인 성공 UID={user.UserId}");

                QuestManager.Instance?.InitializeAfterLogin();
            });
        }
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

    // 앱 종료 시에도 안전하게 시도
    private void OnApplicationQuit()
    {
        SafeSave();
    }
}
