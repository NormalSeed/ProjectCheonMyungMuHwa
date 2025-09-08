using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class InGameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<EquipmentService>(Lifetime.Singleton);
        builder.RegisterComponentInHierarchy<TestEquipmentCreator>();
    }
}
