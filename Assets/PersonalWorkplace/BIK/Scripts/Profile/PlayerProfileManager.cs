using Firebase.Auth;
using Firebase.Database;
using System;
using UnityEngine;
using VContainer.Unity;

public class PlayerProfileManager : IStartable, IDisposable
{
    #region Singleton
    public static PlayerProfileManager Instance { get; private set; }
    #endregion

    private readonly DatabaseReference _dbRef;
    private readonly string _uid;
    private readonly TableManager _tableManager; // 레벨 테이블 접근용
    private bool _initialized = false;

    public PlayerProfileData CurrentProfile { get; private set; }

    public event Action<PlayerProfileData> OnProfileChanged;

    public PlayerProfileManager(TableManager tableManager)
    {
        Instance = this;

        _tableManager = tableManager;
        _uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId ?? "dev-local-test";
        _dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void Start()
    {
        LoadProfileFromFirebase();
    }

    public void Dispose()
    {
        // 이벤트 구독 해제 필요 시 여기서 처리
    }

    #region Load / Save

    private async void LoadProfileFromFirebase()
    {
        if (string.IsNullOrEmpty(_uid)) return;

        try {
            var snapshot = await _dbRef.Child("users").Child(_uid).Child("profile").GetValueAsync();

            if (snapshot.Exists) {
                string json = snapshot.GetRawJsonValue();
                CurrentProfile = JsonUtility.FromJson<PlayerProfileData>(json);

                // 레벨/경험치 최소 보정
                NormalizeProfile();

                Debug.Log($"[ProfileManager] 프로필 로드 완료:  {CurrentProfile.Nickname}, {CurrentProfile.Title}, {CurrentProfile.Background}, {CurrentProfile.ProfileImage}, Lv.{CurrentProfile.Level}, Exp={CurrentProfile.Exp}");
            }
            else {
                Debug.Log("[ProfileManager] 서버에 프로필 없음 → 기본값 등록");
                RegisterDefaultProfile();
                SaveProfileToFirebase();
            }
        }
        catch (Exception ex) {
            Debug.LogError($"[ProfileManager] Firebase 로드 실패: {ex.Message}");
            RegisterDefaultProfile();
        }

        _initialized = true;
        OnProfileChanged?.Invoke(CurrentProfile);
    }

    private void NormalizeProfile()
    {
        if (CurrentProfile == null) return;

        if (CurrentProfile.Level <= 0)
            CurrentProfile.Level = 1;

        if (CurrentProfile.Exp < 0)
            CurrentProfile.Exp = 0;
    }

    public void SaveProfileToFirebase()
    {
        if (string.IsNullOrEmpty(_uid)) return;
        if (CurrentProfile == null) return;

        string json = JsonUtility.ToJson(CurrentProfile);
        _dbRef.Child("users").Child(_uid).Child("profile").SetRawJsonValueAsync(json);

        Debug.Log($"[ProfileManager] 프로필 저장됨: {CurrentProfile.Nickname}, {CurrentProfile.Title}, {CurrentProfile.Background}, {CurrentProfile.ProfileImage}, Lv.{CurrentProfile.Level}, Exp={CurrentProfile.Exp}");

        OnProfileChanged?.Invoke(CurrentProfile);
    }

    #endregion

    #region Helpers

    private void RegisterDefaultProfile()
    {
        CurrentProfile = new PlayerProfileData(
            nickname: "새로운 모험가",
            uid: _uid,
            title: "",
            background: "",
            profileImage: "C001_sprite",
            level: 1,
            exp: 0
        );
    }

    public void SetNickname(string nickname)
    {
        if (CurrentProfile == null) return;
        CurrentProfile.Nickname = nickname;
        SaveProfileToFirebase();
    }

    public void SetTitle(string title)
    {
        if (CurrentProfile == null) return;
        CurrentProfile.Title = title;
        SaveProfileToFirebase();
    }

    public void SetBackground(string background)
    {
        if (CurrentProfile == null) return;
        CurrentProfile.Background = background;
        SaveProfileToFirebase();
    }

    public void SetProfileImage(string profileImage)
    {
        if (CurrentProfile == null) return;
        CurrentProfile.ProfileImage = profileImage;
        SaveProfileToFirebase();
    }

    #endregion

    #region Level / Exp

    public int GetCurrentLevelMaxExp()
    {
        if (CurrentProfile == null) return 0;

        var tLevel = _tableManager?.GetTable<TLevel>(TableType.Level);
        if (tLevel == null) {
            Debug.LogWarning("[ProfileManager] TLevel 테이블을 찾을 수 없음");
            return 0;
        }

        return tLevel.GetRequireExp(CurrentProfile.Level);
    }

    public void SetExp(int exp)
    {
        if (CurrentProfile == null) return;

        CurrentProfile.Exp = Mathf.Max(0, exp);
        CheckLevelUpAndNormalize(); // 규칙 적용

        SaveProfileToFirebase();
    }

    public void AddExp(int add)
    {
        if (CurrentProfile == null) return;
        if (add <= 0) return;

        CurrentProfile.Exp += add;
        CheckLevelUpAndNormalize(); // 규칙 적용

        SaveProfileToFirebase();
    }

    public void SetLevel(int level, bool resetExp = true)
    {
        if (CurrentProfile == null) return;
        CurrentProfile.Level = Mathf.Max(1, level);
        if (resetExp) CurrentProfile.Exp = 0;

        // 혹시 잘못된 상태 방지
        CheckLevelUpAndNormalize();

        SaveProfileToFirebase();
    }

    private void CheckLevelUpAndNormalize()
    {
        var tLevel = _tableManager?.GetTable<TLevel>(TableType.Level);
        if (tLevel == null || CurrentProfile == null) return;

        while (true) {
            int req = tLevel.GetRequireExp(CurrentProfile.Level);
            if (req <= 0) break;

            if (CurrentProfile.Exp >= req) {
                CurrentProfile.Exp -= req;
                CurrentProfile.Level += 1;
                Debug.Log($"[ProfileManager] 레벨업! → Lv.{CurrentProfile.Level}");
                PopupManager.Instance.ShowLevelUpPopup(CurrentProfile.Level - 1, CurrentProfile.Level);
            }
            else break;
        }
    }

    #endregion
}
