using Firebase;
using Firebase.Extensions;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    private bool _firebaseInitialized = false;

    [SerializeField] private List<EquipmentSO> allTemplates;

    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available) {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                app.Options.DatabaseUrl = new System.Uri("https://cheonmyungmuhwa-d3fc4-default-rtdb.asia-southeast1.firebasedatabase.app");

                _firebaseInitialized = true;

                // Firebase 초기화가 끝났으니 DI 컨테이너 빌드

                Build(); // 이게 핵심!
            }
        });

    }

    protected override void Configure(IContainerBuilder builder)
    {
        // 재화
        builder.Register<CurrencyModel>(Lifetime.Singleton)
               .As<ICurrencyModel>();
        builder.RegisterEntryPoint<CurrencyManager>(Lifetime.Singleton);
        // 프로필
        builder.RegisterEntryPoint<PlayerProfileManager>(Lifetime.Singleton);

        Debug.Log("[GameLifetimeScope] EquipmentManager 등록 시도");
        // 장비 매니저에 등록할 장비 템플릿
        builder.RegisterInstance(allTemplates);
        // 장비 매니저
        builder.RegisterEntryPoint<EquipmentManager>(Lifetime.Singleton)
            .WithParameter("allTemplates", allTemplates)
            .AsSelf();
        Debug.Log("[GameLifetimeScope] EquipmentManager 등록 완료");

        // 영웅정보
        builder.RegisterEntryPoint<HeroDataManager>(Lifetime.Singleton);

        // 테이블
        builder.RegisterEntryPoint<TableManager>(Lifetime.Singleton).AsSelf();
    }
}
