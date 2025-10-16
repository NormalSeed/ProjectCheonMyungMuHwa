using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleTutorialManager : MonoBehaviour
{
    public static SimpleTutorialManager Instance;

    [Header("튜토리얼 세팅")]
    [SerializeField] private GameObject fingerPrefab;
    [SerializeField] private Transform indicatorRoot;
    [SerializeField] private List<UITutorialStep> steps;

    [Header("UI 차단용 오버레이")]
    [SerializeField] private Image blocker;

    private int currentStepIndex;
    private GameObject fingerInstance;
    private bool tutorialRunning = false;
    private TutorialBlocker tutorialBlocker;
    [SerializeField] private string tutorialKey = "TutorialCompleted";

    private void Awake()
    {
        Instance = this;
        tutorialBlocker = blocker.GetComponent<TutorialBlocker>();
        if (tutorialBlocker == null)
            tutorialBlocker = blocker.gameObject.AddComponent<TutorialBlocker>();

        if (blocker != null)
        {
            blocker.gameObject.SetActive(false);
        }
    }
#if UNITY_EDITOR
    // 유니티 에디터에서 테스트 목적으로만 사용됩니다.
    private void Update()
    {
        // R 키를 누르면 튜토리얼 진행 상태를 초기화합니다.
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetTutorialForTest();
        }
    }
#endif

    /// <summary>
    /// 테스트용: 튜토리얼 완료 기록을 모두 초기화합니다.
    /// </summary>
    public void ResetTutorialForTest()
    {
        // 1. 로컬에 저장된 PlayerPrefs 초기화
        PlayerPrefs.SetInt(tutorialKey, 0);
        PlayerPrefs.Save();
        Debug.Log($"[튜토리얼 테스트] 로컬 저장소({tutorialKey})의 완료 기록을 초기화했습니다.");

        // 2. Firebase에 저장된 데이터 초기화
        if (CurrencyManager.Instance != null)
        {
            var db = CurrencyManager.Instance.DbRef;
            var uid = CurrencyManager.Instance.UserID;
            if (!string.IsNullOrEmpty(uid) && db != null)
            {
                db.Child("users").Child(uid).Child("tutorialCompleted").SetValueAsync(false);
                Debug.Log("[튜토리얼 테스트] Firebase의 완료 기록을 'false'로 초기화했습니다.");
            }
        }

        Debug.LogWarning("[튜토리얼 테스트] 튜토리얼이 리셋되었습니다. 게임을 재시작하거나 씬을 다시 로드하면 튜토리얼이 처음부터 나타납니다.");

        // 만약 즉시 튜토리얼을 다시 시작하고 싶다면, 아래 주석을 해제하세요.
        // StartTutorial(); 

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void Start()
    {
        StartCoroutine(InitTutorial());
    }

    private IEnumerator InitTutorial()
    {
        yield return null; // UI 완성 대기
        yield return new WaitForEndOfFrame(); // 추가 안정화

        if (IsTutorialCompleted())
        {
            Debug.Log("[튜토리얼] 이미 완료된 계정 → 실행하지 않음");
            yield break;
        }

        StartTutorial();
    }

    private bool IsTutorialCompleted()
    {
        return PlayerPrefs.GetInt(tutorialKey, 0) == 1;
    }

    public void StartTutorial()
    {
        if (tutorialRunning) return;

        tutorialRunning = true;
        currentStepIndex = 0;

        if (blocker != null)
        {
            blocker.color = new Color(0, 0, 0, 0.5f);
            blocker.raycastTarget = true;
            blocker.gameObject.SetActive(true);
        }

        if (fingerInstance == null)
            fingerInstance = Instantiate(fingerPrefab, indicatorRoot);

        ShowStep();
    }

    private void ShowStep()
    {
        if (currentStepIndex >= steps.Count)
        {
            EndTutorial();
            return;
        }

        var step = steps[currentStepIndex];
        if (step.Target == null)
        {
            Debug.LogWarning($"[튜토리얼] {currentStepIndex}단계의 Target이 없습니다. 다음 단계로 이동합니다.");
            currentStepIndex++;
            ShowStep();
            return;
        }

        // Canvas 모드와 카메라 확인
        Canvas canvas = indicatorRoot.GetComponentInParent<Canvas>();
        Camera uiCamera = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            uiCamera = null;
        else
            uiCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;

        // 손가락 위치 계산 (UI 완성될 때까지 잠깐 대기)
        StartCoroutine(PositionFingerNextFrame(step, uiCamera));
    }

    private IEnumerator PositionFingerNextFrame(UITutorialStep step, Camera uiCamera)
    {
        // 1프레임 대기 (UI 레이아웃이 안정될 때까지)
        yield return null;
        yield return new WaitForEndOfFrame();

        if (fingerInstance == null)
            fingerInstance = Instantiate(fingerPrefab, indicatorRoot);

        // 안전한 좌표 계산
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, step.Target.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            indicatorRoot as RectTransform,
            screenPos,
            uiCamera,
            out Vector2 localPos
        );

        var fingerRect = fingerInstance.GetComponent<RectTransform>();
        fingerRect.anchoredPosition = localPos + (Vector2)step.Offset;
        fingerInstance.SetActive(true);
        fingerInstance.transform.SetAsLastSibling();

        tutorialBlocker.allowedArea = step.Target as RectTransform;

        //  Button 리스너 중복 방지
        var btn = step.Target.GetComponent<Button>();
        if (btn != null)
        {
            // 일회성으로 사용할 리스너를 선언합니다.
            UnityEngine.Events.UnityAction tutorialClickListener = null;

            tutorialClickListener = () =>
            {
                // 1. 이 리스너의 역할이 끝났으므로 즉시 스스로를 제거합니다. (중복 실행 방지)
                btn.onClick.RemoveListener(tutorialClickListener);

                // 2. 튜토리얼을 다음 단계로 진행합니다.
                if (!tutorialRunning) return;
                Debug.Log($"[튜토리얼] 버튼 클릭: {btn.name}");
                currentStepIndex++;
                ShowStep();
            };

            // 3. 기존 리스너를 지우지 않고, 위에서 만든 일회성 리스너를 '추가'만 합니다.
            btn.onClick.AddListener(tutorialClickListener);
        }
        else
        {
            StartCoroutine(WaitForClick(step.Target));
        }
    }

    private IEnumerator WaitForClick(Transform target)
    {
        bool clicked = false;

        while (!clicked)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 inputPos = Input.mousePosition;
                RectTransform targetRect = target.GetComponent<RectTransform>();

                if (RectTransformUtility.RectangleContainsScreenPoint(targetRect, inputPos))
                {
                    clicked = true;
                    Debug.Log($"[튜토리얼] 클릭 성공: {target.name}");
                }
            }
            yield return null;
        }

        currentStepIndex++;
        ShowStep();
    }

    private void EndTutorial()
    {
        tutorialRunning = false; //  명시적으로 종료 상태 설정

        if (fingerInstance != null)
            fingerInstance.SetActive(false);

        if (blocker != null)
        {
            blocker.raycastTarget = false;

            if (tutorialBlocker != null)
                tutorialBlocker.allowedArea = null;

            blocker.gameObject.SetActive(false);
        }

        PlayerPrefs.SetInt(tutorialKey, 1);
        PlayerPrefs.Save();

        //  Firebase에도 저장 
        if (CurrencyManager.Instance != null)
        {
            var db = CurrencyManager.Instance.DbRef;
            var uid = CurrencyManager.Instance.UserID;
            if (!string.IsNullOrEmpty(uid))
                db.Child("users").Child(uid).Child("tutorialCompleted").SetValueAsync(true);
        }

        Debug.Log("[튜토리얼] 완료. 튜토리얼 상태 저장됨");
    }
}

[System.Serializable]
public class UITutorialStep
{
    public string FingerPosName;  // 예: "FingerPos_Upgrade"
    public Transform Target;
    public Vector3 Offset;
}

public class TutorialBlocker : MonoBehaviour, ICanvasRaycastFilter
{
    public RectTransform allowedArea; // 클릭 통과 허용 영역

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        // allowedArea가 지정되어 있지 않으면 전부 차단
        if (allowedArea == null)
            return true;

        // allowedArea 내부는 클릭 통과시키기 (false 반환 → blocker가 막지 않음)
        return !RectTransformUtility.RectangleContainsScreenPoint(allowedArea, sp, eventCamera);
    }
}