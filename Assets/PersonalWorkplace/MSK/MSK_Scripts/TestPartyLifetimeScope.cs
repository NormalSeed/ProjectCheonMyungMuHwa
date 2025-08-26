using VContainer;
using VContainer.Unity;

public class TestPartyLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<PartyManager>(Lifetime.Singleton);
        builder.Register<HeroUI>(Lifetime.Scoped);
        builder.RegisterEntryPoint<HeroUI>(Lifetime.Scoped);
        builder.RegisterEntryPoint<PartyManager>(Lifetime.Singleton);
    }
}