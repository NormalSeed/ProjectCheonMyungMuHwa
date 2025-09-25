using System;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using UnityEngine.UI;
using NUnit.Framework.Internal;

public class BossBarLifetimeScope : LifetimeScope
{
    [SerializeField] public StageBarUI BarUI;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(BarUI);

    }
}

