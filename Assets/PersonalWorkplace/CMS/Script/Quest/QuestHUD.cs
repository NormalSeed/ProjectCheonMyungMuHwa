using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using VContainer;

public class QuestHUD : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TMP_Text rewardAmount;
    [SerializeField] private TMP_Text questTitleText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Button claimButton;   // 버튼 연결

    [Header("Colors")]
    [SerializeField] private Color completeColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;

    private Quest currentQuest;
    private TableManager _tableManager;

    private AsyncOperationHandle<Sprite>? _loadedHandle;

    [Inject]
    public void Construct(TableManager tableManager)
    {
        _tableManager = tableManager;
    }

    private void Awake()
    {
        if (claimButton != null)
            claimButton.onClick.AddListener(OnClaimButtonClicked);
    }

    public void ShowQuest(Quest quest)
    {
        if (quest == null)
        {
            questTitleText.text = "퀘스트 없음";
            progressText.text = "";
            rewardIcon.sprite = null;
            rewardAmount.text = "";
            return;
        }

        currentQuest = quest;
        if (currentQuest == null)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);

        questTitleText.text = quest.questName;

        // 진행도 표시
        bool isClear = quest.state == QuestState.RewardReady;
        progressText.text = isClear ? "클리어" : $"{quest.valueProgress}/{quest.valueGoal}";
        progressText.color = isClear ? completeColor : defaultColor;

        // 보상 표시 (첫 번째 보상만)
        if (quest.rewards != null && quest.rewards.Count > 0)
        {
            var reward = quest.rewards[0];
            var itemTable = _tableManager.GetTable<TItem>(TableType.Item);

            ItemData itemData = null;

            if (reward.rewardType == RewardType.Currency)
                itemData = itemTable.GetItem((int)reward.currencyType);
            else
                itemData = itemTable.GetItem(int.Parse(reward.rewardID));

            if (itemData != null)
            {
                LoadRewardIcon(itemData.ImageKey);
                rewardAmount.text = reward.rewardCount > 1 ? $"x{reward.rewardCount}" : string.Empty;
            }
            else
            {
                Debug.LogWarning($"[QuestHUD] 아이템 데이터 없음: {reward.rewardID}");
            }
        }

        // 버튼 상태 갱신
        claimButton.interactable = isClear;
    }

    private void OnClaimButtonClicked()
    {
        if (currentQuest == null) return;

        if (currentQuest.state == QuestState.RewardReady)
        {
            // MissionQuestManager를 통해 보상 수령
            MissionQuestManager.Instance.ClaimReward();
        }
        else
        {
            Debug.Log("[QuestHUD] 아직 보상을 받을 수 없는 상태");
        }
    }

    private void LoadRewardIcon(string key)
    {
        ReleaseImageHandle();

        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("[QuestHUD] ImageKey is empty");
            return;
        }

        _loadedHandle = Addressables.LoadAssetAsync<Sprite>(key);
        _loadedHandle.Value.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                rewardIcon.sprite = handle.Result;
            else
                Debug.LogWarning($"[QuestHUD] Failed to load sprite: {key}");
        };
    }

    private void ReleaseImageHandle()
    {
        if (_loadedHandle.HasValue)
        {
            Addressables.Release(_loadedHandle.Value);
            _loadedHandle = null;
        }
    }

    private void OnDestroy()
    {
        ReleaseImageHandle();

        if (claimButton != null)
            claimButton.onClick.RemoveListener(OnClaimButtonClicked);
    }
}