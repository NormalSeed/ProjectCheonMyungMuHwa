using Firebase;
using Firebase.Extensions;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    private bool _firebaseInitialized = false;

    [SerializeField]
    private List<EquipmentSO> equipmentTemplates;

    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
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

        // 장비
        builder.RegisterInstance(equipmentTemplates);
        builder.Register<EquipmentManager>(Lifetime.Singleton)
               .WithParameter("allTemplates", equipmentTemplates);
        // 장비 획득, 장착, 강화, 필터링을 담당하는 EquipmentService 구현 후 등록 필요
        // UI와 데이터를 연결하는 EquipmentView 구현 후 등록 필요
    }
}
