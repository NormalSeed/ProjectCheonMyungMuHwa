using System;

[Serializable]
public class PlayerProfileData
{
    public string Nickname;       // 플레이어 닉네임
    public string Uid;            // Firebase UID
    public string Title;          // 칭호
    public string Background;     // 배경화면 (리소스 이름 or 경로)
    public string ProfileImage;   // 프로필 사진 (리소스 이름 or 경로)

    public PlayerProfileData(string nickname, string uid, string title, string background, string profileImage)
    {
        Nickname = nickname;
        Uid = uid;
        Title = title;
        Background = background;
        ProfileImage = profileImage;
    }
}
