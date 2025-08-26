using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
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

                // Firebase 준비가 끝나면 로그인 시도
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
        Debug.Log("SignInAnonymously 호출됨");

        Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            Debug.Log("Firebase SignIn 결과 콜백 도착");

            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("익명 로그인 실패: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result.User;
            Debug.Log($"익명 로그인 성공 UID: {newUser.UserId}");

            if (QuestManager.Instance != null)
            {
                Debug.Log("QuestManager.Instance 찾음, 초기화 호출");
                QuestManager.Instance.InitializeAfterLogin();
            }
            else
            {
                Debug.LogWarning("QuestManager.Instance 없음");
            }
        });
    }
}
