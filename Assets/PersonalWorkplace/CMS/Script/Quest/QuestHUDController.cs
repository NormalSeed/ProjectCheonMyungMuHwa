using System.Linq;
using UnityEngine;

public class QuestHUDController : MonoBehaviour
{
    [SerializeField] private QuestHUD questHUD;

    private void Start()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.IsReady)
        {
            RefreshHUD();
        }

        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestsUpdated += RefreshHUD;
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestsUpdated -= RefreshHUD;
    }

    private void RefreshHUD()
    {
        Quest questToShow = QuestManager.Instance.GetQuestToDisplayOnHUD();
        questHUD.ShowQuest(questToShow);
    }
}
