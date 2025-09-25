using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class InGameLifetimeScope : LifetimeScope
{
    protected override void Awake()
    {
        base.Awake();
        Build();
    }
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<EquipmentService>(Lifetime.Singleton);
        builder.RegisterComponentInHierarchy<TestEquipmentCreator>();
        builder.RegisterComponentInHierarchy<EquipGachaManager>();
        builder.RegisterComponentInHierarchy<HeroInfoUI>();
        builder.RegisterComponentInHierarchy<EquipmentItemList>();
        builder.RegisterComponentInHierarchy<EquipmentInfoPanel>();
        builder.RegisterComponentInHierarchy<EquipmentChange>();
        builder.RegisterComponentInHierarchy<HeroDataManager>();
    }
}
