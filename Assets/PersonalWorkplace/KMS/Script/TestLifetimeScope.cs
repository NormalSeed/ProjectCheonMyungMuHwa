using VContainer;
using VContainer.Unity;

public class TestLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        var allA = FindObjectsByType<ProjectilePool>(UnityEngine.FindObjectsSortMode.None);
        builder.RegisterComponentInHierarchy<PoolManager>();
        builder.RegisterInstance(allA);
    }
}
