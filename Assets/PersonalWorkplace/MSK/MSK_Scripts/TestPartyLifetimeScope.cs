using VContainer;
using VContainer.Unity;

public class TestPartyLifetimeScope : LifetimeScope
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
        builder.Register<PartyManager>(Lifetime.Singleton)
            .As<IStartable>();

        // 프로필
        builder.RegisterEntryPoint<CurrencyManager>(Lifetime.Singleton);

        
        
    }
}
