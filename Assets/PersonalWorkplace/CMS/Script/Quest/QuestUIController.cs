using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class QuestUIController : UIBase
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private Image progressFill;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Button claimButton;
    [SerializeField] private GameObject completeMark;
    [SerializeField] private Image dimOverlay;

    [Header("보상 UI")]
    [SerializeField] private Transform rewardIconsParent; // RewardIcons 오브젝트
    [SerializeField] private GameObject itemSlotPrefab;   // ItemSlot 프리팹

    private Quest currentQuest;

    public void SetData(Quest quest)
    {
        currentQuest = quest;

        questNameText.text = quest.questName;
        progressFill.fillAmount = (float)quest.valueProgress / quest.valueGoal;
        progressText.text = $"{quest.valueProgress}/{quest.valueGoal}";

        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(OnClickClaim);

        // Addressables 초기화 보장 후 리워드 슬롯 생성
        StartCoroutine(InitAndRefreshRewardSlots());
        UpdateStateUI();
    }

    private IEnumerator InitAndRefreshRewardSlots()
    {
        // Addressables 준비 안 되었으면 초기화 기다림
        if (!Addressables.ResourceLocators.GetEnumerator().MoveNext())
        {
            Debug.Log("[QuestUIController] Addressables 초기화 중...");
            yield return Addressables.InitializeAsync();
        }

        RefreshRewardSlots();
    }

    private void RefreshRewardSlots()
    {
        if (rewardIconsParent == null || itemSlotPrefab == null)
        {
            Debug.LogWarning("[QuestUIController] RewardIcons 또는 ItemSlotPrefab이 지정되지 않았습니다.");
            return;
        }

        // 기존 슬롯 삭제
        foreach (Transform child in rewardIconsParent)
            Destroy(child.gameObject);

        if (currentQuest.rewards == null || currentQuest.rewards.Count == 0)
        {
            Debug.LogWarning("[QuestUIController] 보상 데이터 없음");
            return;
        }

        foreach (var reward in currentQuest.rewards)
        {
            var slotObj = Instantiate(itemSlotPrefab, rewardIconsParent);
            slotObj.SetActive(true); // 프리팹 비활성 방지
            var slot = slotObj.GetComponent<ItemSlot>();
            if (slot == null)
            {
                Debug.LogWarning("[QuestUIController] ItemSlot 컴포넌트 누락됨");
                continue;
            }

            // 아이템 데이터 생성
            ItemData item = null;
            switch (reward.currencyType)
            {
                case CurrencyType.Gold:
                    item = new ItemData(11002, "", "", "GoldImage", true, ItemType.Currency); break;
                case CurrencyType.Soul:
                    item = new ItemData(11003, "", "", "SoulImage", true, ItemType.Currency); break;
                case CurrencyType.SpiritStone:
                    item = new ItemData(11004, "", "", "SpiritImage", true, ItemType.Currency); break;
                case CurrencyType.SummonTicket:
                    item = new ItemData(11006, "", "", "HeroTicketImage", true, ItemType.Currency); break;
                case CurrencyType.EquipmentSummonTicket:
                    item = new ItemData(11011, "", "", "EquipTicketImage", true, ItemType.Currency); break;
                default:
                    item = new ItemData(0, reward.GetDisplayName(), "", "DefaultItemIcon", true, ItemType.Currency); break;
            }

            // 수량 표시
            var amount = new BigCurrency(reward.rewardCount, 0);

            // 안전한 이미지 로드
            StartCoroutine(SafeSetItem(slot, item, amount));
        }
    }

    private IEnumerator SafeSetItem(ItemSlot slot, ItemData item, BigCurrency amount)
    {
        yield return null;

        slot.SetItem(item, amount);

        // 추가 안정성: Completed 콜백이 느릴 수 있으니 0.2초 후에도 이미지 없으면 기본 아이콘
        yield return new WaitForSeconds(0.2f);

        var img = slot.GetComponentInChildren<Image>();
        if (img != null && img.sprite == null)
        {
            Debug.LogWarning($"[QuestUIController] {item.ImageKey} 로드 실패로 Default 아이콘 대체 시도");
            var handle = Addressables.LoadAssetAsync<Sprite>("DefaultItemIcon");
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
                img.sprite = handle.Result;
        }
    }

    private void UpdateStateUI()
    {
        bool isRewardReady = currentQuest.state == QuestState.RewardReady;
        bool isCompletedOrDisabled =
            currentQuest.state == QuestState.Completed ||
            currentQuest.state == QuestState.Disabled;

        claimButton.interactable = isRewardReady;

        if (completeMark != null)
            completeMark.SetActive(isCompletedOrDisabled);

        if (dimOverlay != null)
            dimOverlay.gameObject.SetActive(isCompletedOrDisabled);
    }

    private void OnClickClaim()
    {
        if (currentQuest == null) return;
        QuestManager.Instance.ClaimReward(currentQuest);
    }

    public override void RefreshUI()
    {
        if (currentQuest != null &&
            QuestManager.Instance.activeQuests.TryGetValue(currentQuest.questID, out Quest updatedQuest))
        {
            currentQuest.valueProgress = updatedQuest.valueProgress;
            progressFill.fillAmount = (float)updatedQuest.valueProgress / updatedQuest.valueGoal;
            progressText.text = $"{updatedQuest.valueProgress}/{updatedQuest.valueGoal}";
        }
    }

    private void OnEnable()
    {
        QuestManager.Instance.OnQuestProgressChanged += HandleProgressChanged;
        QuestManager.Instance.OnQuestsUpdated += RefreshUI;
    }

    private void OnDisable()
    {
        QuestManager.Instance.OnQuestProgressChanged -= HandleProgressChanged;
        QuestManager.Instance.OnQuestsUpdated -= RefreshUI;
    }

    private void HandleProgressChanged(Quest quest)
    {
        if (currentQuest != null && currentQuest.questID == quest.questID)
        {
            progressFill.fillAmount = (float)quest.valueProgress / quest.valueGoal;
            progressText.text = $"{quest.valueProgress}/{quest.valueGoal}";
        }
    }
}