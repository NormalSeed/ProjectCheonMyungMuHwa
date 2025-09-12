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
    [SerializeField] private Button claimButton;

    [Header("Colors")]
    [SerializeField] private Color completeColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;

    private Quest currentQuest;
    private TableManager _tableManager;
    private AsyncOperationHandle<Sprite>? _loadedHandle;

    private string GetCurrencyIconKey(CurrencyType type)
    {
        switch (type)
        {
            case CurrencyType.Gold: return "Icon_Gold";
            default: return null;
        }
    }

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
        currentQuest = quest; // currentQuest를 먼저 업데이트

        if (currentQuest == null)
        {
            // 표시할 퀘스트가 없으면 HUD를 비활성화
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        questTitleText.text = quest.questName;

        bool isClear = quest.state == QuestState.RewardReady;
        progressText.text = isClear ? "클리어" : $"{quest.valueProgress}/{quest.valueGoal}";
        progressText.color = isClear ? completeColor : defaultColor;

        if (quest.rewards != null && quest.rewards.Count > 0)
        {
            var reward = quest.rewards[0];
            if (reward.rewardType == RewardType.Currency)
            {
                string iconKey = GetCurrencyIconKey(reward.currencyType);
                if (!string.IsNullOrEmpty(iconKey))
                {
                    LoadRewardIcon(iconKey);
                    rewardAmount.text = reward.rewardCount > 1 ? $"x{reward.rewardCount}" : string.Empty;
                }
                else
                {
                    rewardIcon.sprite = null;
                    rewardAmount.text = "";
                }
            }
            else
            {
                var itemTable = _tableManager.GetTable<TItem>(TableType.Item);
                var itemData = itemTable.GetItem(int.Parse(reward.rewardID));
                if (itemData != null && !string.IsNullOrEmpty(itemData.ImageKey))
                {
                    LoadRewardIcon(itemData.ImageKey);
                    rewardAmount.text = reward.rewardCount > 1 ? $"x{reward.rewardCount}" : string.Empty;
                }
                else
                {
                    rewardIcon.sprite = null;
                    rewardAmount.text = "";
                }
            }
        }

        claimButton.interactable = isClear;
    }

    private void OnClaimButtonClicked()
    {
        if (currentQuest == null || currentQuest.state != QuestState.RewardReady)
        {
            Debug.Log("[QuestHUD] 아직 보상을 받을 수 없는 상태");
            return;
        }
        QuestManager.Instance.ClaimReward(currentQuest);
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
