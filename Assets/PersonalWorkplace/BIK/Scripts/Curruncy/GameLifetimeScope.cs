using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    protected override void Configure(IContainerBuilder builder)
    {
        // 재화
        builder.Register<CurrencyModel>(Lifetime.Singleton)
               .As<ICurrencyModel>();

        builder.RegisterEntryPoint<CurrencyManager>(Lifetime.Singleton);

        // 프로필
        builder.RegisterEntryPoint<PlayerProfileManager>(Lifetime.Singleton);
    }
}
