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
    private bool _initialized = false;

    public PlayerProfileData CurrentProfile { get; private set; }

    public event Action<PlayerProfileData> OnProfileChanged;

    public PlayerProfileManager()
    {
        Instance = this;

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

                Debug.Log($"[ProfileManager] 프로필 로드 완료:  {CurrentProfile.Nickname}, {CurrentProfile.Title}, {CurrentProfile.Background}, {CurrentProfile.ProfileImage}");
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

    public void SaveProfileToFirebase()
    {
        if (string.IsNullOrEmpty(_uid)) return;

        string json = JsonUtility.ToJson(CurrentProfile);
        _dbRef.Child("users").Child(_uid).Child("profile").SetRawJsonValueAsync(json);

        Debug.Log($"[ProfileManager] 프로필 저장됨: {CurrentProfile.Nickname}, {CurrentProfile.Title}, {CurrentProfile.Background}, {CurrentProfile.ProfileImage}, ");

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
            background: "DefaultBG",       // Resources/Backgrounds/DefaultBG.png
            profileImage: "DefaultProfile" // Resources/Profile/DefaultProfile.png
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
}
