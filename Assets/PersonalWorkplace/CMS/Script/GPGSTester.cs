using GooglePlayGames.BasicApi;
using UnityEngine;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;
using System.Threading.Tasks;

public class GPGSTester : MonoBehaviour
{
    void Start()
    {
        PlayGamesPlatform.Instance.Authenticate(OnAuthenticated);
    }

    private void OnAuthenticated(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("GPGS 자동 로그인 성공!");
        }
        else
        {
            Debug.Log("GPGS 로그인 실패");
        }
    }
}