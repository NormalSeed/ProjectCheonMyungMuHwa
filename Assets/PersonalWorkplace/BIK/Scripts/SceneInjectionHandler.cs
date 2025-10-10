using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

public class SceneInjectionHandler : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var scope = LifetimeScope.Find<GameLifetimeScope>();
        if (scope == null) return;

        foreach (var ui in Object.FindObjectsOfType<CurrencyUI>(true)) {
            scope.Container.Inject(ui);
        }
    }
}
