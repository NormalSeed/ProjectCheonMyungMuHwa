using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [SerializeField] private GameObject fingerPrefab;
    [SerializeField] private Transform indicatorRoot;
    [SerializeField] private List<TutorialScenario> tutorialScenarios;

    private List<TutorialStep> steps;
    private int currentStepIndex;
    private GameObject fingerInstance;
    private CanvasGroup previousHighlight;

    private void Awake() => Instance = this;

    public void StartTutorial(string questID)
    {
        Debug.Log($"[튜토리얼] StartTutorial 호출됨: {questID}");

        var scenario = tutorialScenarios.Find(s => s.QuestID == questID);
        if (scenario == null)
        {
            Debug.LogWarning($"[튜토리얼] {questID} 시나리오 없음");
            return;
        }

        Debug.Log($"[튜토리얼] {questID} 시나리오 찾음, 단계 수: {scenario.Steps.Count}");

        foreach (var step in scenario.Steps)
        {
            var obj = GameObject.Find(step.FingerPosName);
            if (obj != null)
            {
                step.Target = obj.transform;
                Debug.Log($"[튜토리얼] {step.FingerPosName} 연결 완료");
            }
            else
            {
                Debug.LogWarning($"[튜토리얼] {step.FingerPosName} 오브젝트를 찾을 수 없습니다.");
            }
        }

        steps = scenario.Steps;
        currentStepIndex = 0;
        Debug.Log("[튜토리얼] ShowStep 호출 시작");
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

        if (fingerInstance == null)
        {
            fingerInstance = Instantiate(fingerPrefab, indicatorRoot);
        }
        else
        {
            // 이미 손가락이 있다면 다시 활성화시켜줍니다.
            fingerInstance.SetActive(true);
        }

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, step.Target.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            indicatorRoot as RectTransform,
            screenPos,
            Camera.main,
            out Vector2 localPos
        );
        fingerInstance.GetComponent<RectTransform>().anchoredPosition = localPos + (Vector2)step.Offset;

        if (previousHighlight != null)
            previousHighlight.interactable = false;

        CanvasGroup cg = step.Target.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.interactable = true;
            previousHighlight = cg;
        }

        if (step.WaitForClick)
            StartCoroutine(WaitForClick(step.Target));
        else if (step.Condition != null)
            StartCoroutine(WaitForCondition(step.Condition));
    }

    private IEnumerator WaitForClick(Transform target)
    {
        bool clicked = false;

        // 손가락 RectTransform
        RectTransform fingerRect = target.GetComponent<RectTransform>();
        if (fingerRect == null)
        {
            Debug.LogError("[튜토리얼] Target에 RectTransform이 없습니다!");
            yield break;
        }

        Canvas rootCanvas = indicatorRoot.GetComponentInParent<Canvas>();
        if (rootCanvas == null)
        {
            Debug.LogError("[튜토리얼] indicatorRoot의 Canvas를 찾을 수 없습니다!");
            yield break;
        }

        Debug.Log("[튜토리얼] 손가락 클릭 감지 대기 시작");

        while (!clicked)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current == null)
                {
                    Debug.LogWarning("[튜토리얼] EventSystem이 없습니다!");
                    yield break;
                }

                Vector2 inputPos = Input.mousePosition;
                PointerEventData eventData = new PointerEventData(EventSystem.current)
                {
                    position = inputPos
                };

                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);

                foreach (var r in results)
                {
                    Button btn = r.gameObject.GetComponent<Button>();
                    if (btn == null) continue;

                    // 클릭된 버튼의 Canvas 기준 좌표 계산
                    RectTransform btnRect = btn.GetComponent<RectTransform>();
                    Vector2 btnLocalPos, fingerLocalPos;

                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        rootCanvas.GetComponent<RectTransform>(),
                        RectTransformUtility.WorldToScreenPoint(null, btnRect.position),
                        null,
                        out btnLocalPos
                    );

                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        rootCanvas.GetComponent<RectTransform>(),
                        RectTransformUtility.WorldToScreenPoint(null, fingerRect.position),
                        null,
                        out fingerLocalPos
                    );

                    float dist = Vector2.Distance(btnLocalPos, fingerLocalPos);
                    Debug.Log($"[튜토리얼] 클릭된 버튼: {btn.name}, 손가락과 거리: {dist:F1}px");

                    // 반경 내라면 클릭 인정
                    if (dist <= 200f)
                    {
                        Debug.Log($"[튜토리얼] 클릭 인정 → {btn.name}");
                        clicked = true;
                        break;
                    }
                }
            }

            yield return null;
        }

        Debug.Log("[튜토리얼] 클릭 감지 완료 → 다음 단계로 이동");
        NextStep();
    }

    private IEnumerator WaitForCondition(System.Func<bool> condition)
    {
        yield return new WaitUntil(() => condition());
        NextStep();
    }

    private void NextStep()
    {
        currentStepIndex++;
        ShowStep();
    }

    private void EndTutorial()
    {
        if (fingerInstance != null)
            fingerInstance.SetActive(false);
        if (previousHighlight != null)
            previousHighlight.interactable = false;

        Debug.Log("튜토리얼 완료!");
    }
}

public enum TutorialWaitType { Click, Condition }
[System.Serializable]
public class TutorialStep
{
    public string Id;
    public string FingerPosName; // 예: "FingerPos_Training_Open"
    public Vector3 Offset;
    public bool WaitForClick;
    public System.Func<bool> Condition;

    [HideInInspector] public Transform Target; // 런타임에 자동 할당됨
}

[System.Serializable]
public class TutorialScenario
{
    public string QuestID;                   // ex) QT001
    public List<TutorialStep> Steps;         // 단계별 손가락 / 설명
}