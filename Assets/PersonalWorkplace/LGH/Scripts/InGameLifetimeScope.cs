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
        builder.RegisterComponentInHierarchy<EquipGachaManager>();
        builder.RegisterComponentInHierarchy<HeroInfoUI>();
        builder.RegisterComponentInHierarchy<EquipmentItemList>();
        builder.RegisterComponentInHierarchy<EquipmentInfoPanel>();  
    }
}
