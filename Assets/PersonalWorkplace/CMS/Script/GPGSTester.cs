using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

public class GPGSTester : MonoBehaviour
{
    public void GPGSTest()
    {
        PlayGamesPlatform.Instance.Authenticate(OnAuthenticated);
    }

    private void OnAuthenticated(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("GPGS 로그인 성공");

            PlayGamesPlatform.Instance.RequestServerSideAccess(true, (authCode) =>
            {
                Debug.Log("ServerAuthCode: " + authCode);
                // 여기서 Firebase Auth 연동
            });
        }
        else
        {
            Debug.LogError("GPGS 로그인 실패");
        }
    }
}