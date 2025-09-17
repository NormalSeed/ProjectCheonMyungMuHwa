using Firebase;
using Firebase.Extensions;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

public class IntroSceneManager : MonoBehaviour
{
    [SerializeField] private string _mainSceneName = "DEMO_GameScene";
    [SerializeField] private LoadingUI loadingUI;

    private AsyncOperation asyncOperation;
    private bool isReady = false;

    private IEnumerator Start()
    {
        // Firebase 초기화
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        if (dependencyTask.Result != DependencyStatus.Available)
        {
            Debug.LogError($"Firebase 초기화 실패: {dependencyTask.Result}");
            yield break;
        }

        // VContainer Scope 대기
        var scope = FindObjectOfType<GameLifetimeScope>();
        if (scope != null)
            yield return new WaitUntil(() => scope.Container != null);

        // 테이블 로딩 대기
        if (scope != null)
        {
            var tableManager = scope.Container.Resolve<TableManager>();
            yield return new WaitUntil(() => tableManager.AllInitialized);
            Debug.Log("[IntroScene] 모든 테이블 로딩 완료!");
        }

        // 장비 매니저 초기화 대기
        if (scope != null)
        {
            var equipmentManager = scope.Container.Resolve<EquipmentManager>();
            yield return new WaitUntil(() => equipmentManager.IsInitialized);
            Debug.Log("[IntroScene] 장비 매니저 초기화 완료!");
        }

        if (scope != null)
        {
            var heroModels = scope.Container.Resolve<HeroModels>();
            heroModels.Init();
            yield return new WaitUntil(() => heroModels.IsInitialized);
            Debug.Log("[IntroScene] 영웅 Model SO 로딩 완료!");
        }

        if (scope != null)
        {
            var heroSkillSets = scope.Container.Resolve<HeroSkillSets>();
            heroSkillSets.Init();
            yield return new WaitUntil(() => heroSkillSets.IsInitialized);
            Debug.Log("[IntroScene] 영웅 스킬셋 로딩 완료!");
        }

        // 메인 씬 비동기 로드
        asyncOperation = SceneManager.LoadSceneAsync(_mainSceneName);
        asyncOperation.allowSceneActivation = false;

        // 최소 로딩 시간 보장
        float minLoadingTime = 2f;
        float elapsed = 0f;
        float displayedProgress = 0f;

        while (!asyncOperation.isDone)
        {
            elapsed += Time.deltaTime;

            // 실제 로딩 진행도 (0 ~ 0.9)
            float target = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            // 최소 로딩 시간 동안은 Progress를 서서히 올리기
            if (elapsed < minLoadingTime)
            {
                target = Mathf.Min(target, elapsed / minLoadingTime);
            }

            // 부드럽게 보간
            displayedProgress = Mathf.MoveTowards(displayedProgress, target, Time.deltaTime * 0.5f);
            loadingUI.UpdateProgress(displayedProgress);

            // 로딩이 다 끝나고 Progress도 1.0에 도달하면 Tap to Start로 이동
            if (asyncOperation.progress >= 0.9f && displayedProgress >= 0.99f)
                break;

            yield return null;
        }

        // Tap to Start 연출
        loadingUI.ShowTapToStart();
        yield return new WaitUntil(() => Input.anyKeyDown || Input.touchCount > 0);

        Debug.Log("[IntroScene] 게임 시작!");
        loadingUI.gameObject.SetActive(false);

        asyncOperation.allowSceneActivation = true;
    }
}