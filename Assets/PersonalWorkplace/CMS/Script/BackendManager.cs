using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackendManager : MonoBehaviour
{
    public static BackendManager Instance { get; private set; }
    public static FirebaseApp FirebaseApp { get; private set; }
    public static FirebaseAuth Auth { get; private set; }
    public static FirebaseDatabase Database { get; private set; }

    private float autoSaveInterval = 30f;
    public event Action OnFirebaseReady;
    public event Action OnLoginSuccess; // 로그인 완료 후 이벤트

    // 상태 플래그
    private bool isLoginInProgress = false;
    private bool isLoadingPlayerData = false;
    private bool isSaving = false;

    BigCurrency gold;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;

            //  여기서 Init 보장
            FirebaseApp app = FirebaseApp.DefaultInstance;
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            FirebaseDatabase db = FirebaseDatabase.DefaultInstance;
            Init(app, auth, db);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Init(FirebaseApp firebaseApp, FirebaseAuth auth, FirebaseDatabase database)
    {
        FirebaseApp = firebaseApp;
        Auth = auth;
        Database = database;

        InitializeGPGS();
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

    #region Google Play 로그인

    private Task<SignInStatus> PlayGamesAuthenticateAsync()
    {
        var tcs = new TaskCompletionSource<SignInStatus>();
        try
        {
            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                tcs.TrySetResult(status);
            });
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }
        return tcs.Task;
    }

    private Task<string> RequestServerSideAccessAsync(bool refreshToken)
    {
        var tcs = new TaskCompletionSource<string>();
        try
        {
            PlayGamesPlatform.Instance.RequestServerSideAccess(refreshToken, authCode =>
            {
                tcs.TrySetResult(authCode);
            });
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }
        return tcs.Task;
    }

    public void InitializeGPGS()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        Debug.Log("GPGS 초기화 완료, 자동 로그인 시도...");
        _ = SignInWithGPGSAsync().ContinueWith(t =>
        {
            if (t.IsFaulted)
                Debug.LogError("SignInWithGPGSAsync 실패: " + t.Exception);
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public void SignInWithGPGS()
    {
        if (Auth == null)
        {
            Debug.LogError("Firebase Auth 초기화 안 됨. 로그인 시도 중단");
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
                        Debug.LogError("GPGS 서버 인증 코드 받기 실패. 게스트 로그인 시도");
                        SignInAsGuest();
                        return;
                    }

                    Debug.Log("GPGS 서버 인증 코드 수신 완료");
                    Credential credential = PlayGamesAuthProvider.GetCredential(authCode);

                    // FirebaseUser가 null이면 직접 로그인
                    if (Auth.CurrentUser == null)
                    {
                        SignInWithCredential(credential);
                        return;
                    }

                    // 게스트 계정이면 전환 시도
                    if (Auth.CurrentUser.IsAnonymous)
                    {
                        Auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
                        {
                            if (task is Task<AuthResult> authTask)
                            {
                                if (authTask.IsCanceled || authTask.IsFaulted)
                                {
                                    Debug.LogError("게스트 → 구글 계정 전환 실패: " + authTask.Exception);
                                    SignInWithCredential(credential); // 전환 실패 시 직접 로그인 시도
                                    return;
                                }

                                FirebaseUser upgradedUser = authTask.Result.User;
                                Debug.Log($"계정 전환 성공! UID: {upgradedUser.UserId}, DisplayName: {upgradedUser.DisplayName}, Email: {upgradedUser.Email}");

                                LoadPlayerData(() =>
                                {
                                    HandleLoginSuccess();
                                });
                            }
                            else
                            {
                                Debug.LogError("LinkWithCredentialAsync 결과 타입 오류: " + task.GetType());
                                SignInWithCredential(credential); // 타입 오류 시 직접 로그인 시도
                            }
                        });
                    }
                    else
                    {
                        // 이미 로그인된 상태면 직접 로그인
                        SignInWithCredential(credential);
                    }
                });
            }
            else
            {
                Debug.LogWarning("GPGS 로그인 실패. 게스트 로그인 시도: " + status);
                SignInAsGuest();
            }
        });
    }

    public async Task SignInWithGPGSAsync()
    {
        if (Auth == null)
        {
            Debug.LogError("Firebase Auth 초기화 안 됨. 로그인 시도 중단");
            return;
        }

        if (isLoginInProgress) return;
        isLoginInProgress = true;

        try
        {
            SignInStatus status = await PlayGamesAuthenticateAsync();
            if (status != SignInStatus.Success)
            {
                Debug.LogWarning("GPGS 로그인 실패. 게스트 로그인 시도: " + status);
                await SignInAsGuestAsync();
                return;
            }

            Debug.Log("GPGS 로그인 성공!");
            string authCode = await RequestServerSideAccessAsync(true);

            if (string.IsNullOrEmpty(authCode))
            {
                Debug.LogError("GPGS 서버 인증 코드 받기 실패. 게스트 로그인 시도");
                await SignInAsGuestAsync();
                return;
            }

            Credential credential = PlayGamesAuthProvider.GetCredential(authCode);

            if (Auth.CurrentUser == null)
            {
                await SignInWithCredentialAsync(credential);
                return;
            }

            if (Auth.CurrentUser.IsAnonymous)
            {
                try
                {
                    var linkResult = await Auth.CurrentUser.LinkWithCredentialAsync(credential);
                    FirebaseUser upgradedUser = linkResult.User;
                    Debug.Log($"계정 전환 성공! UID: {upgradedUser.UserId}, DisplayName: {upgradedUser.DisplayName}, Email: {upgradedUser.Email}");
                    await LoadPlayerDataAsync();
                    HandleLoginSuccess();
                }
                catch (Exception ex)
                {
                    Debug.LogError("게스트 → 구글 계정 전환 실패: " + ex);
                    await SignInWithCredentialAsync(credential);
                }
                return;
            }

            await SignInWithCredentialAsync(credential);
        }
        finally
        {
            isLoginInProgress = false;
        }
    }

    private void SignInWithCredential(Credential credential)
    {
        Auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Firebase 자격증명 로그인 실패: " + task.Exception);
                SignInAsGuest();
                return;
            }

            // 성공 흐름
            FirebaseUser newUser = task.Result;
            Debug.Log($"Firebase 로그인 성공! UID: {newUser.UserId}, DisplayName: {newUser.DisplayName}, Email: {newUser.Email}");

            LoadPlayerData(() =>
            {
                HandleLoginSuccess();
            });
        });
    }

    private async Task SignInWithCredentialAsync(Credential credential)
    {
        if (Auth == null) return;
        if (isLoginInProgress) return;
        isLoginInProgress = true;

        try
        {
            var authResult = await Auth.SignInWithCredentialAsync(credential);
            FirebaseUser newUser = authResult;
            Debug.Log($"Firebase 로그인 성공! UID: {newUser.UserId}, DisplayName: {newUser.DisplayName}, Email: {newUser.Email}");

            await LoadPlayerDataAsync();
            HandleLoginSuccess();
        }
        catch (Exception ex)
        {
            Debug.LogError("Firebase 자격증명 로그인 실패: " + ex);
            await SignInAsGuestAsync();
        }
        finally
        {
            isLoginInProgress = false;
        }
    }
    #endregion

    #region 게스트 로그인
    public void SignInAsGuest()
    {
        if (Auth == null)
        {
            Debug.LogError("Firebase Auth 초기화 안 됨. 게스트 로그인 불가");
            return;
        }

        var signInTask = Auth.SignInAnonymouslyAsync();

        signInTask.ContinueWithOnMainThread(task =>
        {
            if (signInTask.IsCanceled || signInTask.IsFaulted)
            {
                Debug.LogError("게스트 로그인 실패: " + signInTask.Exception);
                return;
            }

            FirebaseUser guestUser = signInTask.Result.User;
            Debug.Log($"게스트 로그인 성공! UID: {guestUser.UserId}");

            LoadPlayerData(() =>
            {
                HandleLoginSuccess();
            });
        });
    }
    public async Task SignInAsGuestAsync()
    {
        if (Auth == null)
        {
            Debug.LogError("Firebase Auth 초기화 안 됨. 게스트 로그인 불가");
            return;
        }

        if (isLoginInProgress) return;
        isLoginInProgress = true;

        try
        {
            var signInResult = await Auth.SignInAnonymouslyAsync();
            FirebaseUser guestUser = signInResult.User;
            Debug.Log($"게스트 로그인 성공! UID: {guestUser.UserId}");

            await LoadPlayerDataAsync();
            HandleLoginSuccess();
        }
        catch (Exception ex)
        {
            Debug.LogError("게스트 로그인 실패: " + ex);
        }
        finally
        {
            isLoginInProgress = false;
        }
    }

    #endregion

    #region 계정 전환 (게스트 → 구글)
    public void LinkGuestToGoogle()
    {
        if (Auth == null || Auth.CurrentUser == null)
        {
            Debug.LogError("Firebase Auth 초기화 안 됨. 계정 전환 불가");
            return;
        }

        if (!Auth.CurrentUser.IsAnonymous)
        {
            Debug.LogWarning("현재 계정은 게스트가 아님. 전환 불필요");
            return;
        }

        Debug.Log("게스트 계정을 구글 계정으로 전환 시도...");

        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status != SignInStatus.Success)
            {
                Debug.LogError("구글 로그인 실패. 계정 전환 취소");
                return;
            }

            PlayGamesPlatform.Instance.RequestServerSideAccess(true, authCode =>
            {
                if (string.IsNullOrEmpty(authCode))
                {
                    Debug.LogError("구글 인증 코드 받기 실패");
                    return;
                }

                Credential credential = PlayGamesAuthProvider.GetCredential(authCode);

                var linkTask = Auth.CurrentUser.LinkWithCredentialAsync(credential);

                linkTask.ContinueWithOnMainThread(task =>
                {
                    if (linkTask.IsCanceled || linkTask.IsFaulted)
                    {
                        Debug.LogError("게스트 -> 구글 계정 전환 실패: " + linkTask.Exception);
                        return;
                    }

                    FirebaseUser upgradedUser = linkTask.Result.User;
                    Debug.Log($"계정 전환 성공! UID: {upgradedUser.UserId}, DisplayName: {upgradedUser.DisplayName}");

                    LoadPlayerData(() =>
                    {
                        HandleLoginSuccess();
                    });
                });
            });
        });
    }

    public async Task LinkGuestToGoogleAsync()
    {
        if (Auth == null || Auth.CurrentUser == null)
        {
            Debug.LogError("Firebase Auth 초기화 안 됨. 계정 전환 불가");
            return;
        }

        if (!Auth.CurrentUser.IsAnonymous)
        {
            Debug.LogWarning("현재 계정은 게스트가 아님. 전환 불필요");
            return;
        }

        if (isLoginInProgress) return;
        isLoginInProgress = true;

        try
        {
            SignInStatus status = await PlayGamesAuthenticateAsync();
            if (status != SignInStatus.Success)
            {
                Debug.LogError("구글 로그인 실패. 계정 전환 취소");
                return;
            }

            string authCode = await RequestServerSideAccessAsync(true);
            if (string.IsNullOrEmpty(authCode))
            {
                Debug.LogError("구글 인증 코드 받기 실패");
                return;
            }

            Credential credential = PlayGamesAuthProvider.GetCredential(authCode);
            try
            {
                var linkResult = await Auth.CurrentUser.LinkWithCredentialAsync(credential);
                FirebaseUser upgradedUser = linkResult.User;
                Debug.Log($"계정 전환 성공! UID: {upgradedUser.UserId}, DisplayName: {upgradedUser.DisplayName}");
                await LoadPlayerDataAsync();
                HandleLoginSuccess();
            }
            catch (Exception ex)
            {
                Debug.LogError("게스트 -> 구글 계정 전환 실패: " + ex);
            }
        }
        finally
        {
            isLoginInProgress = false;
        }
    }

    #endregion

    public void Logout()
    {
        if (Auth == null) return;

        string uid = Auth.CurrentUser?.UserId;
        bool isGuest = Auth.CurrentUser?.IsAnonymous ?? false;

        Debug.Log($"로그아웃 시도. UID={uid}, Guest={isGuest}");

        Auth.SignOut();

        // 게스트 계정, 로컬 데이터도 삭제
        if (isGuest)
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("게스트 계정 로그아웃. 로컬 데이터 초기화");
        }

        // IntroScene으로 복귀
        SceneManager.LoadScene("IntroScene");
    }

    public async void DeleteAccount(string confirmUid)
    {
        if (Auth.CurrentUser == null)
        {
            Debug.LogError("삭제할 계정이 없음");
            return;
        }

        string uid = Auth.CurrentUser.UserId;
        if (uid != confirmUid)
        {
            Debug.LogWarning("입력한 UID 불일치. 삭제 취소");
            return;
        }

        try
        {
            // 1. DB 데이터 삭제
            await Database.RootReference.Child("players").Child(uid).RemoveValueAsync();

            // 2. Firebase Auth 계정 삭제
            await Auth.CurrentUser.DeleteAsync();

            Debug.Log("계정 삭제 완료");

            // 3. 로컬 데이터 삭제 + IntroScene으로 이동
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene("IntroScene");
        }
        catch (Exception ex)
        {
            Debug.LogError("계정 삭제 실패: " + ex);
        }
    }
    #region 데이터 로드 & 저장
    public void LoadPlayerData(Action onComplete = null)
    {
        if (Auth.CurrentUser == null)
        {
            Debug.LogError("로그인된 유저가 없어 데이터를 불러올 수 없습니다.");
            return;
        }

        string uid = Auth.CurrentUser.UserId;
        Debug.Log($"[BackendManager] LoadPlayerData 시작, UID={uid}");

        DatabaseReference userRef = Database.RootReference.Child("players").Child(uid);

        userRef.GetValueAsync().ContinueWithOnMainThread((Task<DataSnapshot> task) =>
        {
            Debug.Log("[BackendManager] DB 응답 수신");
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("플레이어 데이터 불러오기 실패: " + task.Exception);
                return;
            }

            try
            {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists)
                {
                    Debug.Log("[BackendManager] 신규 유저 데이터 생성");
                    PlayerDataManager.Instance?.InitializeDefaultData();
                    SafeSave();
                }
                else
                {
                    int clearedStage = snapshot.Child("clearedStage").Exists
                        ? int.Parse(snapshot.Child("clearedStage").Value.ToString())
                        : 1;

                    if (snapshot.Child("gold").Exists)
                    {
                        string rawGold = snapshot.Child("gold").Value.ToString();
                        if (!BigCurrency.TryParse(rawGold, out gold))
                        {
                            Debug.LogWarning($"골드 파싱 실패: {rawGold}, 기본값 0 사용");
                            gold = new BigCurrency(0);
                        }
                    }
                    else
                    {
                        gold = new BigCurrency(0);
                    }

                    PlayerDataManager.Instance?.LoadFromServer(clearedStage, gold);
                    Debug.Log($"[BackendManager] 데이터 로드 완료, Stage={clearedStage}, Gold={gold}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[BackendManager] LoadPlayerData 처리 중 예외: " + ex);
            }

            onComplete?.Invoke();
        });
    }

    public async Task LoadPlayerDataAsync()
    {
        if (Auth?.CurrentUser == null)
        {
            Debug.LogError("로그인된 유저가 없어 데이터를 불러올 수 없습니다.");
            return;
        }

        if (isLoadingPlayerData) return;
        isLoadingPlayerData = true;

        try
        {
            string uid = Auth.CurrentUser.UserId;
            Debug.Log($"[BackendManager] LoadPlayerData 시작, UID={uid}");

            var snapshot = await Database.RootReference.Child("players").Child(uid).GetValueAsync();
            Debug.Log("[BackendManager] DB 응답 수신");

            if (!snapshot.Exists)
            {
                Debug.Log("[BackendManager] 신규 유저 데이터 생성");
                PlayerDataManager.Instance?.InitializeDefaultData();
                SafeSave();
            }
            else
            {
                int clearedStage = snapshot.Child("clearedStage").Exists
                    ? int.Parse(snapshot.Child("clearedStage").Value.ToString())
                    : 1;

                if (snapshot.Child("gold").Exists)
                {
                    string rawGold = snapshot.Child("gold").Value.ToString();
                    if (!BigCurrency.TryParse(rawGold, out gold))
                    {
                        Debug.LogWarning($"골드 파싱 실패: {rawGold}, 기본값 0 사용");
                        gold = new BigCurrency(0);
                    }
                }
                else
                {
                    gold = new BigCurrency(0);
                }

                PlayerDataManager.Instance?.LoadFromServer(clearedStage, gold);
                Debug.Log($"[BackendManager] 데이터 로드 완료, Stage={clearedStage}, Gold={gold}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[BackendManager] LoadPlayerData 처리 중 예외: " + ex);
        }
        finally
        {
            isLoadingPlayerData = false;
        }
    }

    private IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SafeSave();
        }
    }

    private void SafeSave1()
    {
        if (Auth == null || Auth.CurrentUser == null) return;
        if (PlayerDataManager.Instance == null || CurrencyManager.Instance == null) return;

        UpdatePlayerData(
            PlayerDataManager.Instance.ClearedStage,
            CurrencyManager.Instance.Get(CurrencyType.Gold)
        );

        Debug.Log("[자동 저장 완료]");
    }

    private async void SafeSave()
    {
        if (Auth == null || Auth.CurrentUser == null) return;
        if (PlayerDataManager.Instance == null || CurrencyManager.Instance == null) return;
        if (isSaving) return;

        isSaving = true;
        try
        {
            int stage = PlayerDataManager.Instance.ClearedStage;
            BigCurrency currentGold = CurrencyManager.Instance.Get(CurrencyType.Gold);
            await UpdatePlayerDataAsync(stage, currentGold);
            Debug.Log("[자동 저장 완료]");
        }
        catch (Exception ex)
        {
            Debug.LogError("[SafeSave] 예외: " + ex);
        }
        finally
        {
            isSaving = false;
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

    public async Task UpdatePlayerDataAsync(int clearedStage, BigCurrency gold)
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

        try
        {
            await userRef.UpdateChildrenAsync(updates);
            Debug.Log($"[서버 저장 완료] Stage={clearedStage}, Gold={gold}");
        }
        catch (Exception ex)
        {
            Debug.LogError("플레이어 데이터 저장 실패: " + ex);
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "DEMO_GameScene")
        {
            Debug.Log("GameScene 로드됨 → QuestManager 초기화 대기 (OnLoginSuccess에서 처리)");
        }
    }
    public void HandleLoginSuccess()
    {
        Debug.Log("로그인 성공!");

        // 이벤트 호출
        OnLoginSuccess?.Invoke();

        // 만약 씬이 GameScene이면 QuestManager 초기화
        if (SceneManager.GetActiveScene().name == "DEMO_GameScene")
        {
            QuestManager.Instance?.InitializeAfterLogin();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnApplicationQuit()
    {
        SafeSave();
    }
    #endregion
}