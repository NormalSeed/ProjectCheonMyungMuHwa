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
                Database = FirebaseDatabase.DefaultInstance;

                Debug.Log("Firebase 초기화 완료!");

                //Firebase 준비가 끝나면 익명 로그인 시도
                SignInAnonymously();
            }
            else
            {
                Debug.LogError($"Firebase 해결 실패: {dependencyStatus}");
            }
        });
    }

    //익명 로그인
    public void SignInAnonymously()
    {
        Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("익명 로그인 실패: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result.User;
            userRef = Database.RootReference.Child("users").Child(newUser.UserId);

            Debug.Log($"익명 로그인 성공 UID: {newUser.UserId}");

            TestLoadQuests();
        });
    }

    //퀘스트 저장
    public void SaveQuest(Quest quest)
    {
        if (userRef == null)
        {
            Debug.LogError("Database 참조 없음! 로그인 먼저 하세요.");
            return;
        }

        string json = JsonUtility.ToJson(quest);
        userRef.Child("quests").Child(quest.questID).SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    Debug.Log($"퀘스트 저장 완료: {quest.questName}");
                else
                    Debug.LogError("퀘스트 저장 실패: " + task.Exception);
            });
    }

    //퀘스트 불러오기
    public void LoadQuests()
    {
        if (userRef == null)
        {
            Debug.LogError("Database 참조 없음! 로그인 먼저 하세요.");
            return;
        }

        userRef.Child("quests").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("퀘스트 불러오기 실패: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;
            foreach (var child in snapshot.Children)
            {
                Quest quest = JsonUtility.FromJson<Quest>(child.GetRawJsonValue());
                Debug.Log($"불러온 퀘스트: {quest.questID}, 진행도 {quest.valueProgress}/{quest.valueGoal}");
            }
        });
    }
    public void TestSaveQuest(Quest quest)
    {
        if (userRef == null || Auth.CurrentUser == null)
        {
            Debug.LogError("Database 참조 없음! 로그인 먼저 하세요.");
            return;
        }

        string json = JsonUtility.ToJson(quest);

        // 여기서 dbRef 대신 userRef 사용
        userRef.Child("testQuest")
            .SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"[DB 저장 완료] {quest.questName} ({quest.valueProgress}/{quest.valueGoal}) 완료={quest.isComplete}, 보상={quest.isClaimed}");
                }
                else
                {
                    Debug.LogError("퀘스트 저장 실패: " + task.Exception);
                }
            });
    }

    //테스트용 퀘스트 불러오기
    private void TestLoadQuests()
    {
        LoadQuests();
    }
}