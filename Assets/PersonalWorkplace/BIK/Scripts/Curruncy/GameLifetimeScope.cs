using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<CurrencyModel>(Lifetime.Singleton)
               .As<ICurrencyModel>();

        builder.Register<GameCurrencyController>(Lifetime.Singleton)
               .As<IGameCurrencyController>();

        builder.RegisterEntryPoint<CurrencySaver>(Lifetime.Singleton);

        // 씬 시작 시 1회 초기화 훅
        builder.RegisterEntryPoint<CurrencyBootstrapper>(Lifetime.Singleton);
    }
}