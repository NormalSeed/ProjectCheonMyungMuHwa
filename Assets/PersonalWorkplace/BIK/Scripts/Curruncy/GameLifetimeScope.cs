using Firebase;
using Firebase.Extensions;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    private bool _firebaseInitialized = false;

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
        // 영웅정보
        builder.RegisterEntryPoint<HeroDataManager>(Lifetime.Singleton);

        // 테이블
        builder.RegisterEntryPoint<TableManager>(Lifetime.Singleton).AsSelf();
    }
}
