using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using GooglePlayGames;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

public class IntroSceneManager : MonoBehaviour
{
    [SerializeField] private string _mainSceneName = "DEMO_GameScene";
    [SerializeField] private LoadingUI loadingUI;
    [SerializeField] private LoadingImageLoader loadingImageLoader;

    [Header("로그인 UI")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject agreementPanel;
    [SerializeField] private Button googleLoginButton;
    [SerializeField] private Button guestLoginButton;
    [SerializeField] private Image serviceAgreementCheck;
    [SerializeField] private Button serviceAgreementButton;
    [SerializeField] private Button lookButton1;
    [SerializeField] private Image personalInformationAgreementCheck;
    [SerializeField] private Button personalInformationAgreementButton;
    [SerializeField] private Button lookButton2;
    [SerializeField] private Image notificationAgreementCheck;
    [SerializeField] private Button notificationAgreementButton;
    [SerializeField] private Button agreeButton;
    [SerializeField] private Button agreeAllButton;

    private AsyncOperation asyncOperation;
    private bool isReady = false;

    public static IntroSceneManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }

        Instance = this;
    }

    public void StartIntroScene()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            Debug.Log("이미 로그인된 유저 있음 → 바로 게임 로딩");
            StartCoroutine(StartLoading(true)); // 자동 로그인 → 바로 로딩
            return;
        }

        SetupLoginUI();

        BackendManager.Instance.OnLoginSuccess += () =>
        {
            loginPanel.SetActive(false);
            agreementPanel.SetActive(false);
            StartCoroutine(StartLoading(true)); // 게임 로딩 시작
        };

    }

    private void SetupLoginUI()
    {
        loginPanel.SetActive(true);
        serviceAgreementCheck.enabled = false;
        personalInformationAgreementCheck.enabled = false;
        notificationAgreementCheck.enabled = false;
        agreeButton.interactable = false;
        agreementPanel.SetActive(false);

        googleLoginButton.onClick.AddListener(() =>
        {
            agreementPanel.SetActive(true);
        });

        guestLoginButton.onClick.AddListener(() =>
        {
            BackendManager.Instance.SignInAsGuestAsync();
        });

        serviceAgreementButton.onClick.AddListener(() =>
        {
            serviceAgreementCheck.enabled = !serviceAgreementButton.enabled;
            UpdateAgreeButtonState();
        });

        lookButton1.onClick.AddListener(() =>
        {
            Application.OpenURL("https://cafe.naver.com/cheonmyungmuhwa/4");
        });

        personalInformationAgreementButton.onClick.AddListener(() =>
        {
            personalInformationAgreementCheck.enabled = !personalInformationAgreementCheck.enabled;
            UpdateAgreeButtonState();
        });

        lookButton2.onClick.AddListener(() =>
        {
            Application.OpenURL("https://cafe.naver.com/cheonmyungmuhwa/3");
            UpdateAgreeButtonState();
        });

        notificationAgreementButton.onClick.AddListener(() =>
        {
            notificationAgreementCheck.enabled = !notificationAgreementCheck.enabled;
        });

        agreeAllButton.onClick.AddListener(() =>
        {
            serviceAgreementCheck.enabled = true;
            personalInformationAgreementCheck.enabled = true;
            notificationAgreementCheck.enabled = true;
            UpdateAgreeButtonState();
        });

        agreeButton.onClick.AddListener(() =>
        {
            if (!AllAgreementsChecked())
            {
                ShowAgreementWarning();
                return;
            }

            FirebaseApp app = FirebaseApp.DefaultInstance;
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            FirebaseDatabase db = FirebaseDatabase.DefaultInstance;

            BackendManager.Instance.Init(app, auth, db);
        });
    }

    private bool AllAgreementsChecked()
    {
        return serviceAgreementCheck.enabled &&
               personalInformationAgreementCheck.enabled &&
               notificationAgreementCheck.enabled;
    }

    private void ShowAgreementWarning()
    {
        Debug.LogWarning("모든 약관에 동의해야 로그인할 수 있습니다.");
        // 여기에 경고 UI 띄우는 로직 추가 가능
    }

    private void UpdateAgreeButtonState()
    {
        agreeButton.interactable = AllAgreementsChecked();
    }

    private IEnumerator StartLoading(bool loginAlreadyCompleted)
    {
        loadingImageLoader.ShowRandomLoadingImage();
        //// Firebase 초기화
        //var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        //yield return new WaitUntil(() => dependencyTask.IsCompleted);

        //if (dependencyTask.Result != DependencyStatus.Available)
        //{
        //    Debug.LogError($"Firebase 초기화 실패: {dependencyTask.Result}");
        //    yield break;
        //}

        // VContainer Scope 대기
        var scope = FindObjectOfType<GameLifetimeScope>();
        if (scope != null)
            yield return new WaitUntil(() => scope.Container != null);

        yield return new WaitUntil(() => BackendManager.Instance != null);

        bool loginCompleted = loginAlreadyCompleted;

        if (!loginAlreadyCompleted)
        {
            BackendManager.Instance.OnLoginSuccess += () => loginCompleted = true;
            yield return new WaitUntil(() => loginCompleted);
        }

        SetText("[IntroScene] 로그인 & 데이터 로드 완료!");

        // 테이블 로딩 대기
        if (scope != null)
        {
            var tableManager = scope.Container.Resolve<TableManager>();
            yield return new WaitUntil(() => tableManager.AllInitialized);
            SetText("[IntroScene] 모든 테이블 로딩 완료!");
        }

        // 장비 매니저 초기화 대기
        if (scope != null)
        {
            var equipmentManager = scope.Container.Resolve<EquipmentManager>();
            yield return new WaitUntil(() => equipmentManager.IsInitialized);
            SetText("[IntroScene] 장비 매니저 초기화 완료!");

        }

        if (scope != null)
        {
            var heroModels = scope.Container.Resolve<HeroModels>();
            heroModels.Init();
            yield return new WaitUntil(() => heroModels.IsInitialized);
            SetText("[IntroScene] 영웅 Model SO 로딩 완료!");
        }

        if (scope != null)
        {
            var heroDataManager = scope.Container.Resolve<HeroDataManager>();
            SetText("[IntroScene] 영웅 데이터 매니저 초기화 대기 시작");
            yield return new WaitUntil(() => heroDataManager.IsInitialized);
            SetText("[IntroScene] 영웅 데이터 매니저 초기화 완료!");
        }

        if (scope != null)
        {
            var heroSkillSets = scope.Container.Resolve<HeroSkillSets>();
            heroSkillSets.Init();
            yield return new WaitUntil(() => heroSkillSets.IsInitialized);
            SetText("[IntroScene] 영웅 스킬셋 로딩 완료!");
        }

        if (scope != null)
        {
            var heroSprites = scope.Container.Resolve<HeroSprites>();
            heroSprites.Init();
            yield return new WaitUntil(() => heroSprites.IsInitialized);
            SetText("[IntroScene] 영웅 스프라이트 로딩 완료!");
        }

        if (scope != null)
        {
            if (scope.Container.TryResolve(out MonsterLoader monsterLoader))
            {
                monsterLoader.Init();
                yield return new WaitUntil(() => monsterLoader.IsInitialized);
                SetText("[IntroScene] 몬스터 로딩 완료!");
            }
        }
        if (scope != null)
        {
            if (scope.Container.TryResolve(out AudioManager audioManager))
            {
                audioManager.Init();
                yield return new WaitUntil(() => audioManager.IsInitialized);
                SetText("[IntroScene] 사운드 로딩 완료!");
            }
        }
        if (scope != null)
        {
            if (scope.Container.TryResolve(out ParticleManager particleManager))
            {
                particleManager.Init();
                yield return new WaitUntil(() => particleManager.IsInitialized);
                SetText("[IntroScene] 파티클 로딩 완료!");
            }
        }

        // 메인 씬 비동기 로드
        asyncOperation = SceneManager.LoadSceneAsync(_mainSceneName);
        asyncOperation.allowSceneActivation = false;

        // 최소 로딩 시간 보장
        float minLoadingTime = 2f;
        float elapsed = 0f;
        float displayedProgress = 0f;

        while (!asyncOperation.isDone)
        {
            elapsed += Time.deltaTime;

            // 실제 로딩 진행도 (0 ~ 0.9)
            float target = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            // 최소 로딩 시간 동안은 Progress를 서서히 올리기
            if (elapsed < minLoadingTime)
            {
                target = Mathf.Min(target, elapsed / minLoadingTime);
            }

            // 부드럽게 보간
            displayedProgress = Mathf.MoveTowards(displayedProgress, target, Time.deltaTime * 0.5f);
            loadingUI.UpdateProgress(displayedProgress);

            // 로딩이 다 끝나고 Progress도 1.0에 도달하면 Tap to Start로 이동
            if (asyncOperation.progress >= 0.9f && displayedProgress >= 0.99f)
                break;

            yield return null;
        }

        // Tap to Start 연출
        loadingUI.ShowTapToStart();
        yield return new WaitUntil(() => Input.anyKeyDown || Input.touchCount > 0);

        Debug.Log("[IntroScene] 게임 시작!");
        loadingUI.gameObject.SetActive(false);

        asyncOperation.allowSceneActivation = true;
    }
    private void SetText(string text)
    {
        Debug.Log(text);
        loadingUI.Text = text;

    }
}