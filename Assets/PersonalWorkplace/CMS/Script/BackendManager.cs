using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Database;
using UnityEngine;

public class BackendManager : MonoBehaviour
{
    public static BackendManager Instance { get; private set; }
    public static FirebaseApp FirebaseApp { get; private set; }
    public static FirebaseAuth Auth { get; private set; }
    public static FirebaseDatabase Database { get; private set; }

    private DatabaseReference userRef;

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

                Debug.Log("Firebase 초기화 완료!");
            }
            else
            {
                Debug.LogError($"Firebase 해결 실패: {dependencyStatus}");
            }
        });
    }

    //이메일 회원가입
    public void SignUpWithEmail(string email, string password)
    {
        Auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("회원가입 실패: " + task.Exception);
                    return;
                }

                FirebaseUser newUser = task.Result.User;
                Database = FirebaseDatabase.DefaultInstance;
                userRef = Database.RootReference.Child("users").Child(newUser.UserId);

                Debug.Log($"회원가입 성공! UID: {newUser.UserId}");
            });
    }

    //이메일 로그인
    public void SignInWithEmail(string email, string password)
    {
        Auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("이메일 로그인 실패: " + task.Exception);
                    return;
                }

                FirebaseUser newUser = task.Result.User;
                Database = FirebaseDatabase.DefaultInstance;
                userRef = Database.RootReference.Child("users").Child(newUser.UserId);

                Debug.Log($"이메일 로그인 성공! UID: {newUser.UserId}");

                // 로그인 성공 후 테스트 실행
                TestSaveData();
                TestLoadData();
            });
    }

    //테스트 데이터 저장
    public void TestSaveData()
    {
        if (userRef == null)
        {
            Debug.LogError("Database 참조 없음! 로그인 먼저 하세요.");
            return;
        }

        userRef.Child("testValue").SetValueAsync("Hello Firebase (Email)!")
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    Debug.Log("테스트 데이터 저장 성공!");
                else
                    Debug.LogError("테스트 데이터 저장 실패: " + task.Exception);
            });
    }

    //테스트 데이터 불러오기
    public void TestLoadData()
    {
        if (userRef == null)
        {
            Debug.LogError("Database 참조 없음! 로그인 먼저 하세요.");
            return;
        }

        userRef.Child("testValue").GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("데이터 불러오기 실패: " + task.Exception);
                    return;
                }

                DataSnapshot snapshot = task.Result;
                Debug.Log("불러온 값: " + snapshot.Value);
            });
    }
}