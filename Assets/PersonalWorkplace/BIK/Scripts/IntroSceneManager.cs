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

    private IEnumerator Start()
    {
        var scope = FindObjectOfType<GameLifetimeScope>();
        if (scope != null) {
            yield return new WaitUntil(() => scope.Container != null);
        }

        if (scope != null) {
            var tableManager = scope.Container.Resolve<TableManager>();

            // AllInitialized == true 될 때까지 기다리기
            yield return new WaitUntil(() => tableManager.AllInitialized);

            Debug.Log("[IntroScene] 모든 테이블 로딩 완료!");
        }

        if (scope != null){
            var equipmentManager = scope.Container.Resolve<EquipmentManager>();

            // Initialize가 실행될 때까지 기다리기
            yield return new WaitUntil(() => equipmentManager.IsInitialized);
        }

        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        if (dependencyTask.Result != DependencyStatus.Available)
        {
            Debug.LogError($"Firebase 초기화 실패: {dependencyTask.Result}");
            yield break;
        }

        SceneManager.LoadScene(_mainSceneName);
    }
}
