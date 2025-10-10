using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

public class InventoryUI : UIBase
{
    [Header("Slots & Controls")]
    [SerializeField] private List<ItemSlot> _itemSlots = new();
    [SerializeField] private Scrollbar _itemUseScrollbar;
    [SerializeField] private Button _useButton;
    [SerializeField] private Button _closeButton;

    [Header("External Config")]
    [SerializeField] private CurrencyConfig _currencyConfig;

    [Header("Stage (temp)")]
    [SerializeField] private int _currentStage = 1; // StageManager 연동 전 임시 값

    private ItemSlot _selectedSlot;
    private int _useCount = 1; // InventoryManager 시그니처에 맞춰 int 유지

    // DI
    [Inject] private TableManager _tableManager;

    // Tables
    private TItem _itemTable;
    private bool _itemTableReady = false;

    // State
    private bool _eventsHooked = false;
    private Coroutine _resolveCo;

    private void Awake()
    {
        foreach (var slot in _itemSlots) {
            slot.OnClick += OnItemSlotClicked;
        }
        _itemUseScrollbar.onValueChanged.AddListener(OnScrollbarChanged);
        _useButton.onClick.AddListener(OnClickUseButton);
        _closeButton.onClick.AddListener(OnClickCloseButton);
    }

    private void OnEnable()
    {
        if (_tableManager == null) {
            _resolveCo ??= StartCoroutine(WaitAndResolveTableManager());
        }
        else {
            PostResolveInit();
        }

        UpdateUseButtonInteractable();
    }

    private void OnDisable()
    {
        if (_resolveCo != null) {
            StopCoroutine(_resolveCo);
            _resolveCo = null;
        }
        UnhookEvents();
    }

    // === Lazy Resolve ===
    private IEnumerator WaitAndResolveTableManager()
    {
        IObjectResolver resolver = null;

        while (resolver == null) {
            var scopes = FindObjectsOfType<GameLifetimeScope>(true);
            for (int i = 0; i < scopes.Length; i++) {
                var s = scopes[i];
                if (s != null && s.Container != null) {
                    resolver = s.Container;
                    break;
                }
            }
            if (resolver == null)
                yield return null; // 다음 프레임 대기
        }

        if (_tableManager == null) {
            _tableManager = resolver.Resolve<TableManager>();
        }

        PostResolveInit();
        _resolveCo = null;
    }

    private void PostResolveInit()
    {
        // TItem 준비
        if (_itemTable == null) {
            _itemTable = _tableManager.GetTable<TItem>(TableType.Item);
            if (_itemTable != null) {
                if (_itemTable.IsInitialized) {
                    _itemTableReady = true;
                }
                else {
                    _itemTable.OnLoaded += HandleItemTableLoaded;
                }
            }
        }

        // 이벤트 연결
        if (!_eventsHooked && InventoryManager.Instance != null) {
            if (InventoryManager.Instance.IsInitialized && _itemTableReady) {
                RefreshAllSlots();
            }
            else {
                InventoryManager.OnInitialized += RefreshAllSlots;
            }
            InventoryManager.Instance.OnItemChanged += OnItemChanged; // (string, int)
            _eventsHooked = true;
        }
    }

    private void UnhookEvents()
    {
        if (_itemTable != null) {
            _itemTable.OnLoaded -= HandleItemTableLoaded;
        }
        if (_eventsHooked && InventoryManager.Instance != null) {
            InventoryManager.OnInitialized -= RefreshAllSlots;
            InventoryManager.Instance.OnItemChanged -= OnItemChanged;
        }
        _eventsHooked = false;
    }

    private void HandleItemTableLoaded()
    {
        _itemTableReady = true;
        RefreshAllSlots();
    }

    // === UI 갱신 ===
    private void RefreshAllSlots()
    {
        if (!_itemTableReady || InventoryManager.Instance == null) return;

        var items = InventoryManager.Instance.Items; // IReadOnlyDictionary<string,int>

        int slotIndex = 0;
        foreach (var kv in items) {
            if (slotIndex >= _itemSlots.Count) break;

            string invKey = kv.Key;
            int count = kv.Value;

            if (!int.TryParse(invKey, out int itemIdInt)) {
                _itemSlots[slotIndex].SetEmpty();
                slotIndex++;
                continue;
            }

            var data = _itemTable.GetItem(itemIdInt);
            if (data != null && count > 0) {
                _itemSlots[slotIndex].SetItem(data, BigCurrency.FromBaseAmount(count));
            }
            else {
                _itemSlots[slotIndex].SetEmpty();
            }
            slotIndex++;
        }

        for (; slotIndex < _itemSlots.Count; slotIndex++) {
            _itemSlots[slotIndex].SetEmpty();
        }

        _selectedSlot = null;
        _itemUseScrollbar.value = 0;
        _useCount = 1;
        UpdateUseButtonInteractable();
    }

    private void OnItemChanged(string itemIdKey, int count)
    {
        if (!_itemTableReady) return;

        if (!int.TryParse(itemIdKey, out int itemIdInt)) {
            // 문자열 키는 슬롯에서 제거 시도
            for (int i = 0; i < _itemSlots.Count; i++) {
                var slot = _itemSlots[i];
                if (slot.Data != null && slot.Data.Id.ToString() == itemIdKey) {
                    slot.SetEmpty();
                    if (_selectedSlot == slot) {
                        _selectedSlot = null;
                        _itemUseScrollbar.value = 0;
                        _useCount = 1;
                    }
                    UpdateUseButtonInteractable();
                    return;
                }
            }
            return;
        }

        for (int i = 0; i < _itemSlots.Count; i++) {
            var slot = _itemSlots[i];
            if (slot.Data != null && slot.Data.Id == itemIdInt) {
                if (count > 0) {
                    slot.SetItem(slot.Data, BigCurrency.FromBaseAmount(count));
                }
                else {
                    slot.SetEmpty();
                    if (_selectedSlot == slot) {
                        _selectedSlot = null;
                        _itemUseScrollbar.value = 0;
                        _useCount = 1;
                    }
                }
                UpdateUseButtonInteractable();
                return;
            }
        }

        if (count > 0) {
            var data = _itemTable.GetItem(itemIdInt);
            if (data == null) return;

            foreach (var slot in _itemSlots) {
                if (slot.Data == null) {
                    slot.SetItem(data, BigCurrency.FromBaseAmount(count));
                    break;
                }
            }
        }

        UpdateUseButtonInteractable();
    }

    private void OnItemSlotClicked(ItemSlot slot)
    {
        if (slot == null || slot.Data == null) return;

        int owned = ToBaseUnitsInt(slot.Amount);
        if (owned <= 0) return;

        _selectedSlot = slot;
        _useCount = 1;
        _itemUseScrollbar.value = 0;
        UpdateUseButtonInteractable();
    }

    private void OnScrollbarChanged(float value)
    {
        if (_selectedSlot == null || _selectedSlot.Amount == null) return;

        int maxCount = Mathf.Max(1, ToBaseUnitsInt(_selectedSlot.Amount));
        _useCount = Mathf.Max(1, Mathf.RoundToInt(value * maxCount));
        UpdateUseButtonInteractable();
    }

    private void OnClickUseButton()
    {
        if (_selectedSlot == null || _selectedSlot.Data == null) return;

        string invKey = _selectedSlot.Data.Id.ToString();

        // 상자면: 차감 → 즉시 보상 계산/팝업
        if (_selectedSlot.Data.Type == ItemType.Box) {
            if (InventoryManager.Instance.TryUse(invKey, _useCount)) {
                OpenBoxesAndShowRewards(_useCount);
            }
            UpdateUseButtonInteractable();
            return;
        }

        // 일반 아이템 사용
        if (InventoryManager.Instance.TryUse(invKey, _useCount)) {
            // OnItemChanged에서 갱신됨
        }
        UpdateUseButtonInteractable();

        QuestManager.Instance.ReportEvent(QuestTargetType.Growth, _useCount);
    }

    private void OnClickCloseButton()
    {
        base.SetHide();
    }

    private void UpdateUseButtonInteractable()
    {
        bool canUse =
            _selectedSlot != null &&
            _selectedSlot.Data != null &&
            _selectedSlot.Amount != null &&
            ToBaseUnitsInt(_selectedSlot.Amount) > 0 &&
            _useCount > 0;

        if (_useButton != null) _useButton.interactable = canUse;
    }

    // === 상자 → 보상 집계 → 보상 팝업 ===
    private void OpenBoxesAndShowRewards(int openCount)
    {
        if (openCount <= 0) return;

        var tChest = _tableManager.GetTable<TNormalChest>(TableType.NormalChest);
        if (tChest == null || !tChest.IsInitialized) {
            Debug.LogWarning("[InventoryUI] TNormalChest 테이블이 준비되지 않았습니다.");
            return;
        }
        if (_itemTable == null || !_itemTableReady) {
            Debug.LogWarning("[InventoryUI] TItem 테이블이 준비되지 않았습니다.");
            return;
        }

        // itemId -> 누적 BigCurrency
        var acc = new Dictionary<int, BigCurrency>();

        for (int i = 0; i < openCount; i++) {
            var kv = tChest.GetSingleReward(_currentStage); // ("Gold", 5000.0) 등
            if (string.IsNullOrEmpty(kv.Key) || kv.Value <= 0) continue;

            if (!TryMapRewardKeyToItemId(kv.Key, out int itemId)) {
                Debug.LogWarning($"[InventoryUI] 매핑되지 않은 보상 키: {kv.Key}");
                continue;
            }

            var add = BigCurrency.FromBaseAmount(kv.Value); // 무단위 → BigCurrency
            if (!acc.TryGetValue(itemId, out var cur)) cur = new BigCurrency(0, 0);
            acc[itemId] = cur + add;
        }

        if (acc.Count == 0) {
            Debug.Log("[InventoryUI] 지급할 보상이 없습니다.");
            return;
        }

        var rewardItems = new List<ItemData>();
        var rewardCounts = new List<BigCurrency>();

        foreach (var pair in acc) {
            var data = _itemTable.GetItem(pair.Key);
            if (data == null) {
                Debug.LogWarning($"[InventoryUI] TItem에 없는 itemId: {pair.Key}");
                continue;
            }
            rewardItems.Add(data);
            rewardCounts.Add(pair.Value);
        }

        if (rewardItems.Count == 0) return;

        PopupManager.Instance.ShowRewardPopup(rewardItems, rewardCounts, false, 0f);
    }

    // 보상 키 → itemId (CurrencyConfig 사용)
    private bool TryMapRewardKeyToItemId(string key, out int itemId)
    {
        itemId = -1;
        if (_currencyConfig == null || string.IsNullOrEmpty(key)) return false;

        if (!TryKeyToCurrencyType(key, out var ctype)) return false;

        itemId = _currencyConfig.GetItemId(ctype);
        return itemId > 0;
    }

    // 시트의 보상 키를 CurrencyType으로 변환 (프로젝트 enum에 맞춰 보완)
    private bool TryKeyToCurrencyType(string key, out CurrencyType type)
    {
        switch (key) {
            case "Gold": type = CurrencyType.Gold; return true;
            case "Soul": type = CurrencyType.Soul; return true;
            case "SpiritStone": type = CurrencyType.SpiritStone; return true;
            case "SummonTicket": type = CurrencyType.SummonTicket; return true;
            case "EquipmentSummonTicket": type = CurrencyType.EquipmentSummonTicket; return true;
            case "GoldChallengeTicket": type = CurrencyType.GoldChallengeTicket; return true;
            case "SoulChallengeTicket": type = CurrencyType.SoulChallengeTicket; return true;
            case "SpiritStoneChallengeTicket": type = CurrencyType.SpiritStoneChallengeTicket; return true;
            default:
                type = CurrencyType.Gold; // dummy
                return false;
        }
    }

    // === BigCurrency 유틸 ===
    private int ToBaseUnitsInt(BigCurrency c)
    {
        if (c == null) return 0;

        double baseAmount = c.Value * System.Math.Pow(1000.0, c.Tier);
        if (baseAmount <= 0) return 0;
        if (baseAmount > int.MaxValue) return int.MaxValue;
        return (int)System.Math.Round(baseAmount);
    }
}
