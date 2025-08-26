using UnityEngine;
using System.Collections;

public class QuestUIManager : MonoBehaviour
{
    [SerializeField] private GameObject questUIPrefab;
    [SerializeField] private Transform contentParent;

    private Coroutine waitRoutine;

    private void OnEnable()
    {
        // 데이터 준비까지 대기
        waitRoutine = StartCoroutine(WaitAndBind());
    }

    private void OnDisable()
    {
        if (waitRoutine != null)
        {
            StopCoroutine(waitRoutine);
            waitRoutine = null;
        }

        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestsUpdated -= RefreshQuestUI;
    }

    private IEnumerator WaitAndBind()
    {
        // QuestManager 인스턴스가 생길 때까지
        while (QuestManager.Instance == null)
            yield return null;

        // 데이터 준비(IsReady) 완료될 때까지
        while (!QuestManager.Instance.IsReady)
            yield return null;

        // 이제 안전하게 이벤트 구독 + 최초 1회 렌더
        QuestManager.Instance.OnQuestsUpdated += RefreshQuestUI;
        RefreshQuestUI();
    }

    public void RefreshQuestUI()
    {
        if (QuestManager.Instance == null || QuestManager.Instance.activeQuests == null)
            return;

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (var quest in QuestManager.Instance.activeQuests.Values)
        {
            var ui = Instantiate(questUIPrefab, contentParent);
            var ctrl = ui.GetComponent<QuestUIController>();
            if (ctrl != null) ctrl.SetData(quest);
        }
    }
}