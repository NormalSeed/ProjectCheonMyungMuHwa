using VContainer;
using VContainer.Unity;

public class CurrencyDungeonLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<EquipmentService>(Lifetime.Singleton);
    }
}
