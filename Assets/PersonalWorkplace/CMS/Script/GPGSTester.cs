using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

public class GPGSTester : MonoBehaviour
{
    public static GPGSTester Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 🔹 자동 로그인 시도
    public void GPGSAutoLogin()
    {
        PlayGamesPlatform.Instance.Authenticate(OnAuthenticated);
    }

    private void OnAuthenticated(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("GPGS 자동 로그인 성공");

            PlayGamesPlatform.Instance.RequestServerSideAccess(true, (serverAuthCode) =>
            {
                if (string.IsNullOrEmpty(serverAuthCode))
                {
                    Debug.LogError("ServerAuthCode 가져오기 실패, 게스트 로그인 fallback");
                    BackendManager.Instance.SignInAnonymously();
                    return;
                }

                BackendManager.Instance.LinkWithGoogle(serverAuthCode);
            });
        }
        else
        {
            Debug.LogError("GPGS 자동 로그인 실패, 게스트 로그인 fallback");
            BackendManager.Instance.SignInAnonymously();
        }
    }
}