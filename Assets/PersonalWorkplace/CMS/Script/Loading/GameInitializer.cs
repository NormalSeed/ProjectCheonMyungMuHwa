using UnityEngine;
using System.Collections;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private LoadingUI loadingUI;

    private void Start()
    {
        StartCoroutine(LoadGameRoutine());
    }

    private IEnumerator LoadGameRoutine()
    {
        float progress = 0f;
        bool loginReady = false;

        BackendManager.Instance.OnFirebaseReady += () => { loginReady = true; };

        while (progress < 1f || !loginReady)
        {
            if (progress < 1f)
                progress += Time.deltaTime * 0.3f;

            loadingUI.UpdateProgress(progress);
            yield return null;
        }

        loadingUI.UpdateProgress(1f);
        loadingUI.ShowTapToStart();
        yield return new WaitUntil(() => Input.anyKeyDown || Input.touchCount > 0);

        Debug.Log("게임 시작!");
        loadingUI.gameObject.SetActive(false);
        OnGameStart();
    }

    private void OnGameStart()
    {
        // 여기에 메인 UI 켜거나, 인게임 씬 진입 로직 넣으면 됨
        // 예: UIManager.Instance.ShowMainUI();
        Debug.Log("게임 준비 완료!");
    }
}